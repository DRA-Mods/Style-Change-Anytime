using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using Multiplayer.API;
using RimWorld;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

[HarmonyPatch(typeof(Dialog_BillConfig), nameof(Dialog_BillConfig.DoWindowContents))]
public static class AddOptionToBills
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, MethodBase baseMethod)
    {
        var instr = instructions.ToList();
        
        var methodToInsert = MethodUtilities.MethodOf(InsertBillMenu);
        var ideologyActiveGetter = AccessTools.DeclaredPropertyGetter(typeof(ModsConfig), nameof(ModsConfig.IdeologyActive));
        var ideoManagerGetter = AccessTools.DeclaredPropertyGetter(typeof(Find), nameof(Find.IdeoManager));

        var patched = 0;

        for (var i = 0; i < instr.Count; i++)
        {
            var ci = instr[i];

            // There's 2 places where ModsConfig.IdeologyActive is called, we need to taget only one of them.
            if (i + 2 < instr.Count && ci.Calls(ideologyActiveGetter) && instr[i + 2].Calls(ideoManagerGetter))
            {
                // Insert this
                yield return CodeInstruction.LoadArgument(0);
                // Load the last Listing_Standard
                yield return CodeInstruction.LoadLocal(25);
                // Load the first "rect" argument as ref
                yield return CodeInstruction.LoadLocal(1, true);
                // Call the inserted method
                yield return new CodeInstruction(OpCodes.Call, methodToInsert);

                // Call ModsConfig.IdeologyActive
                yield return ci;

                // Only remove ideology check if ideology is active, no point otherwise.
                if (ModsConfig.IdeologyActive)
                {
                    // Push false (0) onto stack
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                    // Bitwise & the two values together
                    yield return new CodeInstruction(OpCodes.And);
                }

                patched++;
            }
            else yield return ci;
        }

        const int expected = 1;
        if (expected != patched)
            Log.Error($"[{StyleChangeAnytimeMod.ModName}] - removing vanilla style failed, patched {patched} out of {expected} calls. Method: {baseMethod.GetMethodNameWithNamespace()}");
        else if (StyleChangeAnytimeMod.settings.devModeLogs)
            Log.Message($"[{StyleChangeAnytimeMod.ModName}] - successfully patched {patched} out of {expected} calls for {baseMethod.GetMethodNameWithNamespace()}");
    }

    private static void InsertBillMenu(Dialog_BillConfig instance, Listing_Standard listing, ref Rect rect)
    {
        listing.Gap(rect.height - listing.CurHeight - 90f);
        var producedThing = instance.bill.recipe.ProducedThingDef;

        var relevantStyleCategories = StyleUtilities.FilterCategories(producedThing.RelevantStyleCategories);

        if (!StyleUtilities.CanBeStyled(producedThing, relevantStyleCategories))
        {
            var rect4 = listing.GetRect(30f);
            Widgets.DrawHighlight(rect4);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect4, "StyleChangeAnytimeChangeAppearanceDisabledNoCategories".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        var global = Faction.OfPlayer.ideos.PrimaryIdeo.style.StyleForThingDef(producedThing);
        string globalStyleLabel = global == null ? "StyleChangeAnytimeNoStyle".Translate().CapitalizeFirst() : global.category.LabelCap;
        string currentStyleLabel = instance.bill.style?.Category?.LabelCap ?? "StyleChangeAnytimeNoStyle".Translate().CapitalizeFirst();

        var buttonText = instance.bill.globalStyle && ModsConfig.IdeologyActive ? $"{"UseGlobalStyle".Translate()} ({globalStyleLabel})" : currentStyleLabel;
        if ((!instance.bill.globalStyle || !ModsConfig.IdeologyActive) && instance.bill.style != null && instance.bill.graphicIndexOverride != null)
            buttonText = $"{buttonText} {instance.bill.graphicIndexOverride.Value + 1}";

        if (!listing.ButtonText(buttonText))
            return;

        var options = new List<FloatMenuOption>();
        var stuff = producedThing.MadeFromStuff ? GenStuff.DefaultStuffFor(producedThing) : null;
        var color = stuff == null ? Color.white : producedThing.GetColorForStuff(stuff);

        AddOptions(producedThing.graphicData.Graphic, null, "StyleChangeAnytimeNoStyle".Translate().CapitalizeFirst());

        if (producedThing.CanBeStyled())
        {
            if (ModsConfig.IdeologyActive && !relevantStyleCategories.NullOrEmpty())
            {
                // UseGlobalStyle requires ideology, but so should this branch
                options.Add(new FloatMenuOption($"{"UseGlobalStyle".Translate()} ({globalStyleLabel})", () => SetStyle(instance.bill, global?.styleDef, null, true), instance.bill.recipe.UIIconThing, instance.bill.recipe.UIIcon, global?.styleDef, orderInPriority: 1000));

                foreach (var styleCategoryDef in relevantStyleCategories)
                {
                    foreach (var style in styleCategoryDef.thingDefStyles)
                    {
                        if (producedThing != style.ThingDef)
                            continue;

                        AddOptions(style.StyleDef.Graphic, style.StyleDef, styleCategoryDef.LabelCap);
                        break;
                    }
                }
            }

            if (!producedThing.randomStyle.NullOrEmpty())
            {
                foreach (var randomStyle in producedThing.randomStyle)
                {
                    AddOptions(randomStyle.StyleDef.Graphic, randomStyle.StyleDef, StyleUtilities.GetRandomStyleString(randomStyle));
                }
            }
        }

        if (options.Empty())
            options.Add(new FloatMenuOption("StyleChangeAnytimeChangeAppearanceDisabledNoCategories".Translate(), () => { }) { Disabled = true});

        Find.WindowStack.Add(new FloatMenu(options));

        void AddOptions(Graphic graphic, ThingStyleDef style, string label)
        {
            if (graphic is Graphic_Collection collection && StyleUtilities.IsSupportedGraphic(collection))
            {
                for (var i = 0; i < collection.subGraphics.Length; i++)
                {
                    var localIndex = i;
                    var icon = StyleUtilities.GetInnerGraphicFor(graphic, localIndex);

                    if (localIndex == 0)
                    {
                        options.Add(new FloatMenuOption(
                            "StyleChangeAnytimeRandomDefault".Translate(label.Named("LABEL")),
                            () => SetStyle(instance.bill, style, null, false),
                            icon,
                            color));
                    }

                    options.Add(new FloatMenuOption(
                        "StyleChangeAnytimeRandomIndexed".Translate(label.Named("LABEL"), (localIndex + 1).Named("INDEX")),
                        () => SetStyle(instance.bill, style, localIndex, false),
                        icon,
                        color));
                }
            }
            else
            {
                options.Add(new FloatMenuOption(
                    label,
                    () => SetStyle(instance.bill, style, null, false),
                    Widgets.GetIconFor(producedThing, stuff, style),
                    color));
            }
        }
    }

    [SyncMethod]
    private static void SetStyle(Bill bill, ThingStyleDef styleDef, int? index, bool globalStyle)
    {
        bill.style = styleDef;
        bill.globalStyle = globalStyle;
        bill.graphicIndexOverride = index;
    }
}