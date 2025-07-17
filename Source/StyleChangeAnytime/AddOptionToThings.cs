using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using StyleChangeAnytime.Compat;
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
        if (!CanModify(thing, MpUtils.CanUseDevMode))
            return null;

        if (!StyleUtilities.ShouldShow(thing))
            return null;

        if (!StyleUtilities.CanBeStyled(thing.def, thing.def.RelevantStyleCategories))
            return null;

        var relevantStyles = StyleUtilities.FilterCategories(thing.def.RelevantStyleCategories);

        var gizmo = new Command_Action
        {
            defaultLabel = "StyleChangeAnytimeChangeAppearance".Translate().CapitalizeFirst(),
            defaultDesc = "StyleChangeAnytimeChangeAppearanceDesc".Translate(),
            icon = ChangeStyleTex.Texture,
            Order = 15f,
            action = () => OnGizmo(thing, relevantStyles),
        };
        if (!StyleUtilities.CanBeStyled(thing.def, relevantStyles))
            gizmo.Disable("StyleChangeAnytimeChangeAppearanceDisabledNoCategories".Translate());

        return gizmo;
    }

    private static void OnGizmo(ThingWithComps thing, List<StyleCategoryDef> styles)
    {
        var thingDef = GenConstruct.BuiltDefOf(thing.def) as ThingDef ?? thing.def;
        var stuff = thing.Stuff;
        var color = stuff == null ? Color.white : thing.def.GetColorForStuff(stuff);

        var options = new List<FloatMenuOption>();

        AddOptions(thing.Graphic, null, "StyleChangeAnytimeNoStyle".Translate().CapitalizeFirst());

        if (thing.def.CanBeStyled())
        {
            if (ModsConfig.IdeologyActive && !styles.NullOrEmpty())
            {
                foreach (var styleCategoryDef in styles)
                {
                    foreach (var thingDefStyle in styleCategoryDef.thingDefStyles)
                    {
                        if (thingDefStyle.ThingDef != thingDef)
                            continue;

                        AddOptions(thingDefStyle.StyleDef.Graphic, thingDefStyle.StyleDef, thingDefStyle.StyleDef.Category.LabelCap);
                        break;
                    }
                }
            }

            if (!thing.def.randomStyle.NullOrEmpty())
            {
                foreach (var randomStyle in thing.def.randomStyle)
                {
                    AddOptions(randomStyle.StyleDef.Graphic, randomStyle.StyleDef, StyleUtilities.GetRandomStyleString(randomStyle));
                }
            }
        }

        // Should always be true, but check for extra safety
        if (options.Any())
            Find.WindowStack.Add(new FloatMenu(options));

        void AddOptions(Graphic graphic, ThingStyleDef style, string label)
        {
            if (graphic is Graphic_Collection collection && StyleUtilities.IsSupportedGraphic(collection))
            {
                for (var index = 0; index < collection.subGraphics.Length; index++)
                {
                    var localIndex = index;
                    var icon = StyleUtilities.GetInnerGraphicFor(graphic, localIndex);

                    if (localIndex == 0)
                    {
                        options.Add(new FloatMenuOption(
                            "StyleChangeAnytimeRandomDefault".Translate(label.Named("LABEL")),
                            () => ChangeStyleOfAllAffected(thingDef, style, null, MpUtils.CanUseDevMode),
                            icon,
                            color));
                    }

                    options.Add(new FloatMenuOption(
                        "StyleChangeAnytimeRandomIndexed".Translate(label.Named("LABEL"), (localIndex + 1).Named("INDEX")),
                        () => ChangeStyleOfAllAffected(thingDef, style, localIndex, MpUtils.CanUseDevMode),
                        icon,
                        color));
                }
            }
            else
            {
                options.Add(new FloatMenuOption(
                    label,
                    () => ChangeStyleOfAllAffected(thingDef, style, null, MpUtils.CanUseDevMode),
                    Widgets.GetIconFor(thing.def, stuff, style),
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
            if (!CanModify(thing, canUseDevMode))
                continue;

            thing.StyleDef = styleDef;
            thing.overrideGraphicIndex = index;

            if (thing.Spawned)
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