using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.ID;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterUI.Components.BetterResourcePackMenu;

public class ResourcePackMenuinGame : ModSystem
{
	static readonly MethodInfo IngameOptions_Draw_Method = typeof(IngameOptions).GetMethod("Draw", BindingFlags.Public | BindingFlags.Static);
	static readonly int IngameOptions_leftScale_Size = IngameOptions.leftScale.Length;

	public override void Load()
	{
		if (Main.netMode == NetmodeID.Server) return;
		MonoModHooks.Modify(IngameOptions_Draw_Method, IL_IngameOptions_Draw);
		Array.Resize(ref IngameOptions.leftScale, IngameOptions_leftScale_Size + 1);
		for (int i = 0; i < IngameOptions.leftScale.Length; i++)
		{
			IngameOptions.leftScale[i] = 0.7f;
		}
	}
	/*
		public override void Unload()
		{
			if (Main.netMode == NetmodeID.Server) return;
			Array.Resize(ref IngameOptions.leftScale, IngameOptions_leftScale_Size-1);
		}
	*/
	static void IL_IngameOptions_Draw(ILContext il)
	{
		ILCursor c = new(il);

		/*
			// num8++;
			// sequence point: (line 186, col 3) to (line 186, col 10) in D:\a\tModLoader\tModLoader\src\tModLoader\Terraria\IngameOptions.cs
			IL_04a0: ldloc.s 18
			IL_04a2: ldc.i4.1
			IL_04a3: add
			IL_04a4: stloc.s 18
		*/
		if (!c.TryGotoNext(MoveType.After,
			i => i.MatchLdloc(18),
			i => i.MatchLdcI4(1),
			i => i.MatchAdd(),
			i => i.MatchStloc(18)
		))
		{
			throw new Exception("IL Sequence not found");
		}

		// num8++;
		c.Emit(OpCodes.Ldloc, 18);
		c.Emit(OpCodes.Ldc_I4, 1);
		c.Emit(OpCodes.Add);
		c.Emit(OpCodes.Stloc, 18);

		/*
			// if (DrawLeftSide(sb, Language.GetTextValue("tModLoader.ModConfiguration"), num10, vector4, vector5, leftScale))
			// sequence point: (line 260, col 3) to (line 260, col 133) in D:\a\tModLoader\tModLoader\src\tModLoader\Terraria\IngameOptions.cs
			IL_076f: ldarg.1
			IL_0770: ldstr "tModLoader.ModConfiguration"
			IL_0775: call string Terraria.Localization.Language::GetTextValue(string)
			IL_077a: ldloc.s 21
			IL_077c: ldloc.s 19
			IL_077e: ldloc.s 20
			IL_0780: ldsfld float32[] Terraria.IngameOptions::leftScale
			IL_0785: ldc.r4 0.7
			IL_078a: ldc.r4 0.8
			IL_078f: ldc.r4 0.01
			IL_0794: call bool Terraria.IngameOptions::DrawLeftSide(class [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch, string, int32, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2, float32[], float32, float32, float32)
			IL_0799: brfalse.s IL_07db
		*/
		if (!c.TryGotoNext(MoveType.Before,
			i => i.MatchLdarg(1),
			i => i.MatchLdstr("tModLoader.ModConfiguration"),
			i => i.MatchCall(typeof(Language), nameof(Language.GetTextValue))
		// Shouldn't need any more.
		))
		{
			throw new Exception("IL Sequence not found");
		}

		c.Emit(OpCodes.Ldarg, 1);
		c.Emit(OpCodes.Ldloc, 21);
		c.Emit(OpCodes.Ldloc, 19);
		c.Emit(OpCodes.Ldloc, 20);
		c.Emit(OpCodes.Ldloc, 9);
		c.EmitDelegate<Action<SpriteBatch, int, Vector2, Vector2, bool>>((sb, num2, vector, vector2, flag4) =>
		{
			if (IngameOptions.DrawLeftSide(sb, "Resource Packs", num2, vector, vector2, IngameOptions.leftScale))
			{
				IngameOptions.leftHover = num2;
				if (flag4)
				{
					IngameOptions.Close();
					IngameFancyUI.CoverNextFrame();
					Main.playerInventory = false;
					Main.editChest = false;
					Main.npcChatText = "";
					Main.inFancyUI = true;
					Main.InGameUI.SetState(new UIResourcePackSelectionMenu(new ForceResourcePackReload(), Main.AssetSourceController, AssetInitializer.CreateResourcePackList(Main.instance.Services)));
				}
			}
		});

		// num10++;
		c.Emit(OpCodes.Ldloc, 21);
		c.Emit(OpCodes.Ldc_I4, 1);
		c.Emit(OpCodes.Add);
		c.Emit(OpCodes.Stloc, 21);
	}
}

// Fixes Resources Packs not reloading on close.
public class ForceResourcePackReload : UIState
{
	public override void OnActivate()
	{
		if (!Main.gameMenu)
		{
			Main.instance.TilePaintSystem.Reset();
			IngameFancyUI.Close();
		}
	}
}