using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

[HarmonyPatch(typeof(ThingWithComps), nameof(ThingWithComps.GetGizmos))]
public static class AddOptionToThings
{
    private static readonly CachedTexture ChangeStyleTex = new("UI/Gizmos/ChangeStyle");

    private static IEnumerable<Gizmo> Postfix(IEnumerable<Gizmo> gizmos, ThingWithComps __instance)
    {
        foreach (var gizmo in gizmos)
            yield return gizmo;

        var changeStyleGizmo = GetChangeStyleGizmo(__instance);
        if (changeStyleGizmo != null)
            yield return changeStyleGizmo;
    }

    private static Gizmo GetChangeStyleGizmo(ThingWithComps thing)
    {
        if (!ModsConfig.IdeologyActive)
            return null;

        if (thing is Pawn or Blueprint)
            return null;

        // Only stuff owned by the player
        if (thing is Building && thing.Faction != Faction.OfPlayer)
            return null;

        var shouldShow = thing is Building
            ? StyleChangeAnytimeMod.settings.showOnBuildings
            : StyleChangeAnytimeMod.settings.showOnItems;
        switch (shouldShow)
        {
            case StyleChangeAnytimeSettings.ShowRestrictions.Never:
            case StyleChangeAnytimeSettings.ShowRestrictions.ClassicMode when !Find.IdeoManager.classicMode:
                return null;
        }

        if (thing.def == null || !thing.def.CanBeStyled())
            return null;

        var relevantStyles = StyleFilterUtilities.FilterCategories(thing.def.RelevantStyleCategories);

        var gizmo = new Command_Action
        {
            defaultLabel = "ChangeStyle".Translate().CapitalizeFirst(),
            defaultDesc = "ChangeStyleDesc".Translate(),
            icon = ChangeStyleTex.Texture,
            Order = 15f,
            action = () => OnGizmo(thing, relevantStyles),
        };
        if (!relevantStyles.Any())
            gizmo.Disable("ChangeStyleDisabledNoCategories".Translate());

        return gizmo;
    }

    private static void OnGizmo(ThingWithComps thing, List<StyleCategoryDef> styles)
    {
        var thingDef = thing.def;
        var stuff = thing.Stuff;
        var color = stuff == null ? Color.white : thing.def.GetColorForStuff(stuff);

        var options = new List<FloatMenuOption>
        {
            new("Basic".Translate().CapitalizeFirst(),
                () => ChangeStyleOfAllAffected(thingDef),
                Widgets.GetIconFor(thing.def, stuff),
                color)
        };

        foreach (var styleCategoryDef in styles)
        {
            foreach (var thingDefStyle in styleCategoryDef.thingDefStyles)
            {
                if (thingDefStyle.ThingDef != thing.def)
                    continue;

                var style = thingDefStyle.StyleDef;
                if (style.Graphic is Graphic_Random random)
                {
                    for (var index = 0; index < random.SubGraphicsCount; index++)
                    {
                        var localIndex = index;
                        options.Add(new FloatMenuOption(
                            styleCategoryDef.LabelCap,
                            () => ChangeStyleOfAllAffected(thingDef, style, index: localIndex),
                            Widgets.GetIconFor(thing.def, stuff, thingDefStyle.StyleDef, index),
                            color));
                    }
                }
                else
                {
                    options.Add(new FloatMenuOption(
                        styleCategoryDef.LabelCap,
                        () => ChangeStyleOfAllAffected(thingDef, style),
                        Widgets.GetIconFor(thing.def, stuff, thingDefStyle.StyleDef),
                        color));
                }

                break;
            }
        }

        // Should always be true, but check for extra safety
        if (options.Any())
            Find.WindowStack.Add(new FloatMenu(options));
    }

    [SyncMethod(SyncContext.MapSelected)]
    private static void ChangeStyleOfAllAffected(ThingDef defToChange, ThingStyleDef styleDef = null, int? index = null)
    {
        foreach (var thing in Find.Selector.SelectedObjects.OfType<Thing>())
        {
            if (thing.def != defToChange)
                continue;
            if (thing is Building && thing.Faction != Faction.OfPlayer)
                continue;

            thing.StyleDef = styleDef;
            if (index != null)
                thing.overrideGraphicIndex = index;
            thing.DirtyMapMesh(thing.Map);
        }
    }
}