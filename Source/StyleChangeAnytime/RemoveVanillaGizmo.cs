using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using Verse;

namespace StyleChangeAnytime;

[HarmonyPatch(typeof(Blueprint_Build), nameof(Blueprint_Build.GetGizmos), MethodType.Enumerator)]
internal static class RemoveVanillaGizmo
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase baseMethod)
    {
        var instr = instructions.ToArray();

        var ideoManagerGetter = AccessTools.DeclaredPropertyGetter(typeof(Find), nameof(Find.IdeoManager));
        var ideoManagerClassicModeField = AccessTools.DeclaredField(typeof(IdeoManager), nameof(IdeoManager.classicMode));

        var patched = 0;

        for (var index = 0; index < instr.Length; index++)
        {
            var ci = instr[index];

            yield return ci;

            if (ci.Calls(ideoManagerGetter) && instr[index + 1].LoadsField(ideoManagerClassicModeField))
            {
                // Push false (0) onto stack
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                // Bitwise & the two values together
                yield return new CodeInstruction(OpCodes.And);

                patched++;

                if (StyleChangeAnytimeMod.settings.devModeLogs)
                    Log.Message($"[{StyleChangeAnytimeMod.ModName}] - removing vanilla style change gizmo for {baseMethod.GetMethodNameWithNamespace()}");
            }
        }

        const int expected = 1;
        if (expected != patched)
            Log.Error($"[{StyleChangeAnytimeMod.ModName}] - removing vanilla style failed, patched {patched} out of {expected} calls. Method: {baseMethod.GetMethodNameWithNamespace()}");
        else if (StyleChangeAnytimeMod.settings.devModeLogs)
            Log.Message($"[{StyleChangeAnytimeMod.ModName}] - successfully patched {patched} out of {expected} calls for {baseMethod.GetMethodNameWithNamespace()}");
    }
}