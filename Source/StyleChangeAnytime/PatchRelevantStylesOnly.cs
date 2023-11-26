using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace StyleChangeAnytime;

[HarmonyPatch]
internal static class PatchRelevantStylesOnly
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // Gizmo
        var type = AccessTools.FirstInner(typeof(Blueprint_Build), t => AccessTools.Field(t, "stuffColor") != null);
        yield return AccessTools.FirstMethod(type, m => m.ReturnType == typeof(void));

        // Bill config
        yield return AccessTools.DeclaredMethod(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase baseMethod)
    {
        var target = AccessTools.DeclaredPropertyGetter(typeof(ThingDef), nameof(ThingDef.RelevantStyleCategories));
        var replacement = AccessTools.Method(typeof(PatchRelevantStylesOnly), nameof(FilterCategories));

        foreach (var ci in instructions)
        {
            yield return ci;

            if (ci.Calls(target))
            {
                yield return new CodeInstruction(OpCodes.Call, replacement);

                if (StyleChangeAnytimeMod.settings.devModeLogs)
                {
                    var name = (baseMethod.DeclaringType?.Namespace).NullOrEmpty() ? baseMethod.Name : $"{baseMethod.DeclaringType!.Name}:{baseMethod.Name}";
                    Log.Message($"[Style Change Anytime] - adding relevant category filtering for method {name}");
                }
            }
        }
    }

    private static List<StyleCategoryDef> FilterCategories(List<StyleCategoryDef> list)
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