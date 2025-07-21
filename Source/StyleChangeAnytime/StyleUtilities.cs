using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

public static class StyleUtilities
{
    public static string GetRandomStyleString(ThingStyleChance style)
        => style.StyleDef.overrideLabel.CapitalizeFirst() ?? style.StyleDef.LabelCap.RawText ?? "StyleChangeAnytimeRandomStyle".Translate().CapitalizeFirst();

    public static Texture2D GetInnerGraphicFor(Graphic graphic, int? index)
        => (Texture2D)graphic.ExtractInnerGraphicFor(null, index).MatAt(Rot4.East).mainTexture;

    public static bool CanBeStyled(ThingDef def, List<StyleCategoryDef> styles)
    {
        if (def.graphicData?.Graphic != null && IsSupportedGraphic(def.graphicData.Graphic))
            return true;
        return def.CanBeStyled() && (!styles.NullOrEmpty() || !def.randomStyle.NullOrEmpty());
    }

    public static bool IsSupportedGraphic(Graphic graphic)
    {
        // Consider adding Alpha Craft or whatever had random graphics like that?
        return graphic is Graphic_Random { SubGraphicsCount: > 1 };
    }

    public static List<StyleCategoryDef> FilterCategories(List<StyleCategoryDef> list)
    {
        if (Find.IdeoManager.classicMode || StyleChangeAnytimeMod.settings.showAllStyles || list.NullOrEmpty())
            return list;

        var ideo = Faction.OfPlayer.ideos;
        if (ideo == null)
            return list;

        var primary = ideo.PrimaryIdeo;

        if (StyleChangeAnytimeMod.settings.usePrimaryIdeoOnly && primary != null)
            return list.Where(def => primary.thingStyleCategories.Any(x => x.category == def)).ToList();

        var all = ideo.IdeosMinorListForReading
            .SelectMany(i => i.thingStyleCategories.Select(c => c.category))
            .ConcatIfNotNull(primary?.thingStyleCategories.Select(c => c.category))
            .ToList();

        var result = list.Where(def => all.Contains(def)).ToList();
        if (result.Any())
            return result;
        return list;
    }

    // A bit redundant on the checks, but whatever.
    public static StyleChangeAnytimeSettings.ShowRestrictions GetCategoryFor(Thing thing)
        => (thing, thing.def.category) switch
        {
            _ when thing.def.thingCategories.NotNullAndContains(ThingCategoryDefOf.Chunks) => StyleChangeAnytimeMod.settings.showOnChunks,
            (Frame, _) => StyleChangeAnytimeMod.settings.showOnFrames,
            (Blueprint, _) => StyleChangeAnytimeMod.settings.showOnBlueprints,
            (Building, _) => StyleChangeAnytimeMod.settings.showOnBuildings,
            (Plant, _) => StyleChangeAnytimeMod.settings.showOnPlants,
            (_, ThingCategory.Item) => StyleChangeAnytimeMod.settings.showOnItems,
            (_, ThingCategory.Building) => StyleChangeAnytimeMod.settings.showOnBuildings,
            (_, ThingCategory.Plant) => StyleChangeAnytimeMod.settings.showOnPlants,
            _ => StyleChangeAnytimeSettings.ShowRestrictions.Never,
        };

    public static bool ShouldShow(Thing thing) => GetCategoryFor(thing).ShouldShow();

    public static bool ShouldShow(this StyleChangeAnytimeSettings.ShowRestrictions restriction)
        => restriction switch
        {
            StyleChangeAnytimeSettings.ShowRestrictions.Never => false,
            StyleChangeAnytimeSettings.ShowRestrictions.ClassicMode when !Find.IdeoManager.classicMode => false,
            _ => true,
        };
}