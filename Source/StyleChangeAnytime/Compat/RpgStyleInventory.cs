using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime.Compat;

public static class RpgStyleInventory
{
    private const string PatchCategory = "RPG Style Inventory";
    private static readonly bool CanPatch;

    public static bool IsPatched { get; private set; } = false;

    static RpgStyleInventory() => CanPatch = AddOptionToWornItem.TargetMethod() != null;

    public static void Patch()
    {
        if (!CanPatch)
            return;

        try
        {
            StyleChangeAnytimeMod.Harmony.PatchCategory(PatchCategory);
            IsPatched = true;
        }
        catch (Exception e)
        {
            Log.Error($"[{StyleChangeAnytimeMod.ModName}] - encountered an error trying to patch RPG Style Inventory:\n{e}");
            IsPatched = false;
        }
    }

    public static void Unpatch()
    {
        if (!CanPatch || !IsPatched)
            return;

        try
        {
            StyleChangeAnytimeMod.Harmony.UnpatchCategory(PatchCategory);
            IsPatched = false;
        }
        catch (Exception e)
        {
            Log.Error($"[{StyleChangeAnytimeMod.ModName}] - encountered an error trying to unpatch RPG Style Inventory:\n{e}");
        }
    }

    [HarmonyPatch]
    [HarmonyPatchCategory(PatchCategory)]
    private static class AddOptionToWornItem
    {
        private static bool Prepare(MethodBase baseMethod) => baseMethod != null || TargetMethod() != null;

        internal static MethodBase TargetMethod() => AccessTools.DeclaredMethod("Sandy_Detailed_RPG_GearTab:PopupMenu");

        private static void Postfix(List<FloatMenuOption> __result, Pawn __0, Thing __1)
        {
            if (__result == null || __0 == null || __1 == null)
                return;

            if (!StyleChangeAnytimeMod.settings.ignoreFactionCheck && __0.Faction != Faction.OfPlayer)
                return;

            if (__1 is not ThingWithComps thing)
                return;

            if (!StyleUtilities.ShouldShow(thing))
                return;

            var relevantStyles = StyleUtilities.FilterCategories(thing.def.RelevantStyleCategories);
            if (!StyleUtilities.CanBeStyled(thing.def, relevantStyles))
                return;

            __result.Add(new FloatMenuOption(
                "StyleChangeAnytimeChangeAppearance".Translate(),
                () => AddOptionToThings.DisplayMenuFor(thing, relevantStyles, true),
                AddOptionToThings.ChangeStyleTex.Texture,
                Color.white));
        }
    }
}