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
    private static readonly CachedTexture ChangeStyleTex;

    static AddOptionToThings()
    {
        if (ModsConfig.IdeologyActive)
            ChangeStyleTex = new("UI/Gizmos/ChangeStyle");
        else if (ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core") || ModsConfig.IsActive("OskarPotocki.VanillaFactionsExpanded.Core_steam"))
            ChangeStyleTex = new("UI/VEF_ChangeGraphic");
        else
            ChangeStyleTex = new("UI/Icons/SwitchFaction");
    }

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
        if (thing?.def == null)
            return null;

        // Only stuff owned by the player
        if (!CanModify(thing, MP.CanUseDevMode))
            return null;

        var shouldShow = (thing, thing.def.category) switch
        {
            (Frame, _) => StyleChangeAnytimeMod.settings.showOnFrames,
            (Blueprint, _) => StyleChangeAnytimeMod.settings.showOnBlueprints,
            (Building, _) => StyleChangeAnytimeMod.settings.showOnBuildings,
            (_, ThingCategory.Item) => StyleChangeAnytimeMod.settings.showOnItems,
            _ => StyleChangeAnytimeSettings.ShowRestrictions.Never,
        };
        switch (shouldShow)
        {
            case StyleChangeAnytimeSettings.ShowRestrictions.Never:
            case StyleChangeAnytimeSettings.ShowRestrictions.ClassicMode when !Find.IdeoManager.classicMode:
                return null;
            case StyleChangeAnytimeSettings.ShowRestrictions.Always:
            default:
                break;
        }

        var canBeStyled = thing.def.CanBeStyled();
        var randomStyles = thing.def.randomStyle;
        var supportedGraphic = StyleUtilities.IsSupportedGraphic(thing.Graphic);

        if ((!canBeStyled || (thing.def.RelevantStyleCategories.NullOrEmpty() && randomStyles.NullOrEmpty())) && !supportedGraphic)
            return null;

        var relevantStyles = StyleUtilities.FilterCategories(thing.def.RelevantStyleCategories);

        var gizmo = new Command_Action
        {
            defaultLabel = "ChangeStyle".Translate().CapitalizeFirst(),
            defaultDesc = "ChangeStyleDesc".Translate(),
            icon = ChangeStyleTex.Texture,
            Order = 15f,
            action = () => OnGizmo(thing, relevantStyles),
        };
        if ((!canBeStyled || (relevantStyles.NullOrEmpty() || randomStyles.NullOrEmpty())) && !supportedGraphic)
            gizmo.Disable("ChangeStyleDisabledNoCategories".Translate());

        return gizmo;
    }

    private static void OnGizmo(ThingWithComps thing, List<StyleCategoryDef> styles)
    {
        var thingDef = GenConstruct.BuiltDefOf(thing.def) as ThingDef ?? thing.def;
        var stuff = thing.Stuff;
        var color = stuff == null ? Color.white : thing.def.GetColorForStuff(stuff);

        var options = new List<FloatMenuOption>();

        AddOption(null, "Basic".Translate().CapitalizeFirst());

        foreach (var styleCategoryDef in styles)
        {
            foreach (var thingDefStyle in styleCategoryDef.thingDefStyles)
            {
                if (thingDefStyle.ThingDef != thingDef)
                    continue;

                AddOption(thingDefStyle, thingDefStyle.StyleDef.LabelCap);
                break;
            }
        }

        // Should always be true, but check for extra safety
        if (options.Any())
            Find.WindowStack.Add(new FloatMenu(options));

        void AddOption(ThingDefStyle thingDefStyle, string label)
        {
            var style = thingDefStyle.StyleDef;
            if (style.Graphic is Graphic_Random random)
            {
                for (var index = 0; index < random.SubGraphicsCount; index++)
                {
                    var localIndex = index;
                    var icon = StyleUtilities.GetInnerGraphicFor(thing.Graphic, localIndex);

                    if (localIndex == 0)
                    {
                        options.Add(new FloatMenuOption(
                            $"{label} default (TODO: Translate)",
                            () => ChangeStyleOfAllAffected(thingDef, style, null, MP.CanUseDevMode),
                            icon,
                            color));
                    }

                    options.Add(new FloatMenuOption(
                        $"{label} {localIndex + 1}",
                        () => ChangeStyleOfAllAffected(thingDef, style, localIndex, MP.CanUseDevMode),
                        icon,
                        color));
                }
            }
            else
            {
                options.Add(new FloatMenuOption(
                    label,
                    () => ChangeStyleOfAllAffected(thingDef, style, -1, MP.CanUseDevMode),
                    Widgets.GetIconFor(thing.def, stuff, thingDefStyle.StyleDef),
                    color));
            }
        }
    }

    [SyncMethod(SyncContext.MapSelected)]
    private static void ChangeStyleOfAllAffected(ThingDef defToChange, ThingStyleDef styleDef, int? index, bool canUseDevMode)
    {
        foreach (var thing in Find.Selector.SelectedObjects.OfType<Thing>())
        {
            if ((GenConstruct.BuiltDefOf(thing.def) as ThingDef ?? thing.def) != defToChange)
                continue;
            if (CanModify(thing, canUseDevMode))
                continue;

            thing.StyleDef = styleDef;
            if (index is null or >= 0)
                thing.overrideGraphicIndex = index;
            thing.DirtyMapMesh(thing.Map);
        }
    }

    private static bool CanModify(Thing thing, bool canUseDevMode)
    {
        if (!thing.def.CanHaveFaction)
            return true;
        if ((!MP.IsInMultiplayer || canUseDevMode) && StyleChangeAnytimeMod.settings.ignoreFactionCheck)
            return true;

        return thing.Faction == Faction.OfPlayer;
    }
}