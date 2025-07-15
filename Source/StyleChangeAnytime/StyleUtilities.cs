using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

public static class StyleUtilities
{
    public static Texture2D GetInnerGraphicFor(Graphic graphic, int? index)
        => (Texture2D)graphic.ExtractInnerGraphicFor(null, index).MatAt(Rot4.East).mainTexture;

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
}