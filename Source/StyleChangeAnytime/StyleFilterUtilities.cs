using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace StyleChangeAnytime;

public static class StyleFilterUtilities
{
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