using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace StyleChangeAnytime;

[HarmonyPatch]
internal static class RemoveClassicRequirementPatch
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        // Gizmo
        yield return AccessTools.EnumeratorMoveNext(AccessTools.DeclaredMethod(typeof(Blueprint_Build), nameof(Blueprint_Build.GetGizmos)));

        // Bill config
        yield return AccessTools.DeclaredMethod(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents));
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase baseMethod)
    {
        var instr = instructions.ToArray();

        var ideoManagerGetter = AccessTools.DeclaredPropertyGetter(typeof(Find), nameof(Find.IdeoManager));
        var ideoManagerClassicModeField = AccessTools.DeclaredField(typeof(IdeoManager), nameof(IdeoManager.classicMode));

        var checkMethod = GetExtraCheckMethodFor(baseMethod);

        for (var index = 0; index < instr.Length; index++)
        {
            var ci = instr[index];

            if (ci.Calls(ideoManagerGetter) && instr[index + 1].LoadsField(ideoManagerClassicModeField))
            {

                if (checkMethod == null)
                {
                    index += 2;

                    if (StyleChangeAnytimeMod.settings.devModeLogs)
                    {
                        var name = (baseMethod.DeclaringType?.Namespace).NullOrEmpty() ? baseMethod.Name : $"{baseMethod.DeclaringType!.Name}:{baseMethod.Name}";
                        Log.Message($"[Style Change Anytime] - removing classic mode requirement for method {name}");
                    }
                }
                else
                {
                    // Use the already ready switch, so don't skip it
                    index += 1;
                    yield return new CodeInstruction(OpCodes.Call, checkMethod);

                    if (StyleChangeAnytimeMod.settings.devModeLogs)
                    {
                        var name = (baseMethod.DeclaringType?.Namespace).NullOrEmpty() ? baseMethod.Name : $"{baseMethod.DeclaringType!.Name}:{baseMethod.Name}";
                        Log.Message($"[Style Change Anytime] - removing classic mode requirement and adding settings access for method {name}");
                    }
                }
            }
            else yield return ci;
        }
    }

    private static MethodInfo GetExtraCheckMethodFor(MethodBase method)
    {
        if (method == null)
            return null;
        if (method.DeclaringType?.DeclaringType == typeof(Blueprint_Build))
            return AccessTools.DeclaredMethod(typeof(RemoveClassicRequirementPatch), nameof(ShouldAddToBlueprints));
        if (method.DeclaringType == typeof(Dialog_BillConfig))
            return AccessTools.DeclaredMethod(typeof(RemoveClassicRequirementPatch), nameof(ShouldAddToBill));
        return null;
    }

    private static bool ShouldAddToBlueprints() => StyleChangeAnytimeMod.settings.showOnBlueprints;

    private static bool ShouldAddToBill() => StyleChangeAnytimeMod.settings.showOnBillConfig;
}