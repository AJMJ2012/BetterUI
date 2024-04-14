using Terraria.GameContent.UI.States;
using MonoMod.RuntimeDetour.HookGen;
using System.Reflection;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using MonoMod.Cil;
using System;

namespace BetterUI.Components;

class FixResourcePackMenuClose : ModSystem
{
	static readonly MethodInfo UIResourcePackSelectionMenu_HandleBackButtonUsage_Method = typeof(UIResourcePackSelectionMenu).GetMethod("HandleBackButtonUsage", BindingFlags.Public | BindingFlags.Instance);
	delegate void UIResourcePackSelectionMenu_HandleBackButtonUsage_Delegate(UIResourcePackSelectionMenu self);

	public override void Load()
	{
		try { HookEndpointManager.Modify<UIResourcePackSelectionMenu_HandleBackButtonUsage_Delegate>(UIResourcePackSelectionMenu_HandleBackButtonUsage_Method, IL_UIResourcePackSelectionMenu_HandleBackButtonUsage); } catch { throw new Exception("IL Hook Failed"); }
	}

	public override void Unload()
	{
		try { HookEndpointManager.Unmodify<UIResourcePackSelectionMenu_HandleBackButtonUsage_Delegate>(UIResourcePackSelectionMenu_HandleBackButtonUsage_Method, IL_UIResourcePackSelectionMenu_HandleBackButtonUsage); } catch { }
	}

	static void IL_UIResourcePackSelectionMenu_HandleBackButtonUsage(ILContext il)
	{
		ILCursor c = new(il);
		/*
			// SoundEngine.PlaySound(11);
			IL_0000: ldc.i4.s 11
			IL_0002: ldc.i4.m1
			IL_0003: ldc.i4.m1
			IL_0004: ldc.i4.1
			IL_0005: ldc.r4 1
			IL_000a: ldc.r4 0.0
			IL_000f: call class [FNA]Microsoft.Xna.Framework.Audio.SoundEffectInstance Terraria.Audio.SoundEngine::PlaySound(int32, int32, int32, int32, float32, float32)
			IL_0014: pop
		*/
		if (!c.TryGotoNext(MoveType.After,
			i => i.MatchCall(typeof(SoundEngine), nameof(SoundEngine.PlaySound))
		))
		{
			throw new Exception("IL Hook Failed");
		}

		c.Index-=7;
		c.RemoveRange(8);
	}
}
