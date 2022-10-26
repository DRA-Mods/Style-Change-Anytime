using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using JetBrains.Annotations;
using RimWorld;
using Verse;

namespace StyleChangeAnytime
{
    internal static class HarmonyPatches
    {
        [HarmonyPatch]
        private static class PatchGizmoWithoutClassicMode
        {
            [UsedImplicitly]
            private static MethodBase TargetMethod()
            {
                var type = AccessTools.FirstInner(typeof(Blueprint_Build), t => typeof(IEnumerator<Gizmo>).IsAssignableFrom(t));
                return AccessTools.Method(type, nameof(IEnumerator.MoveNext));
            }

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var instr = instructions.ToArray();

                var ideoManagerGetter = AccessTools.DeclaredPropertyGetter(typeof(Find), nameof(Find.IdeoManager));
                var ideoManagerClassicModeField = AccessTools.DeclaredField(typeof(IdeoManager), nameof(IdeoManager.classicMode));

                for (var index = 0; index < instr.Length; index++)
                {
                    var ci = instr[index];

                    if (ci.opcode == OpCodes.Call &&
                        ci.operand is MethodInfo method &&
                        method == ideoManagerGetter &&
                        instr[index + 1].opcode == OpCodes.Ldfld &&
                        instr[index + 1].operand is FieldInfo field &&
                        field == ideoManagerClassicModeField)
                    {
                        index += 2;
                        Log.Message("[Always Available Style Change] - removing classic mode requirement");
                    }
                    else yield return ci;
                }
            }
        }

        [HarmonyPatch]
        private static class PatchRelevantStylesOnly
        {
            [UsedImplicitly]
            private static MethodBase TargetMethod()
            {
                var type = AccessTools.FirstInner(typeof(Blueprint_Build), t => AccessTools.Field(t, "stuffColor") != null);
                return AccessTools.FirstMethod(type, m => m.ReturnType == typeof(void));
            }

            [UsedImplicitly]
            private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var target = AccessTools.DeclaredPropertyGetter(typeof(ThingDef), nameof(ThingDef.RelevantStyleCategories));
                var replacement = AccessTools.Method(typeof(PatchRelevantStylesOnly), nameof(FilterCategories));

                foreach (var ci in instructions)
                {
                    yield return ci;

                    if (ci.opcode == OpCodes.Callvirt && ci.operand is MethodInfo method && method == target)
                    {
                        yield return new CodeInstruction(OpCodes.Call, replacement);
                        Log.Message("[Always Available Style Change] - adding relevant category filtering");
                    }
                }
            }

            private static List<StyleCategoryDef> FilterCategories(List<StyleCategoryDef> list)
            {
                if (Find.IdeoManager.classicMode || StyleChangeAnytimeMod.anytimeSettings.showAllStyles)
                    return list;

                var ideo = Faction.OfPlayer.ideos;
                if (ideo == null)
                    return list;

                var primary = ideo.PrimaryIdeo;

                if (StyleChangeAnytimeMod.anytimeSettings.usePrimaryIdeoOnly && primary != null)
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
    }
}