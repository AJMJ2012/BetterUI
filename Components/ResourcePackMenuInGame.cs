using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using MonoMod.RuntimeDetour.HookGen;
using System;
using System.Reflection;
using Terraria;
using Terraria.GameContent.UI.States;
using Terraria.Initializers;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;

namespace BetterUI.Components;

public class ResourcePackMenuinGame : ModSystem
{
	static readonly MethodInfo IngameOptions_Draw_Method = typeof(IngameOptions).GetMethod("Draw", BindingFlags.Public | BindingFlags.Static);
	delegate void IngameOptions_Draw_Delegate(Main mainInstance, SpriteBatch sb);

	public override void Load()
	{
		try { HookEndpointManager.Modify<IngameOptions_Draw_Delegate>(IngameOptions_Draw_Method, IL_IngameOptions_Draw); } catch { throw new Exception("IL Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Unmodify<IngameOptions_Draw_Delegate>(IngameOptions_Draw_Method, IL_IngameOptions_Draw); } catch { }
	}

	static void IL_IngameOptions_Draw(ILContext il)
	{
		ILCursor c = new(il);
		/*
			// if (DrawLeftSide(sb, Language.GetTextValue("tModLoader.ModConfiguration"), num10, vector4, vector5, leftScale))
			// sequence point: (line 250, col 4) to (line 250, col 133) in D:\a\tModLoader\tModLoader\src\tModLoader\Terraria\IngameOptions.cs
			IL_076b: ldarg.1
			IL_076c: ldstr "tModLoader.ModConfiguration"
			IL_0771: call string Terraria.Localization.Language::GetTextValue(string)
			IL_0776: ldloc.s 21
			IL_0778: ldloc.s 19
			IL_077a: ldloc.s 20
			IL_077c: ldsfld float32[] Terraria.IngameOptions::leftScale
			IL_0781: ldc.r4 0.7
			IL_0786: ldc.r4 0.8
			IL_078b: ldc.r4 0.01
			IL_0790: call bool Terraria.IngameOptions::DrawLeftSide(class [FNA]Microsoft.Xna.Framework.Graphics.SpriteBatch, string, int32, valuetype [FNA]Microsoft.Xna.Framework.Vector2, valuetype [FNA]Microsoft.Xna.Framework.Vector2, float32[], float32, float32, float32)
			IL_0795: brfalse.s IL_07d7
		*/
		if (!c.TryGotoNext(MoveType.Before,
			i => i.MatchLdarg(1),
			i => i.MatchLdstr("tModLoader.ModConfiguration"),
			i => i.MatchCall(typeof(Language), nameof(Language.GetTextValue))
		// Shouldn't need any more.
		))
		{
			throw new Exception("IL Hook Failed");
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

		// num2++
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