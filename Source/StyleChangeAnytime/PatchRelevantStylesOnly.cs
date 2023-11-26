using System.Collections.Generic;
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
        var replacement = AccessTools.Method(typeof(StyleFilterUtilities), nameof(StyleFilterUtilities.FilterCategories));

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

}