using System.Collections.Generic;
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
        foreach (var ci in instructions)
        {
            var methodToInsert = MethodUtilities.MethodOf(Insert);
            var ideologyActiveGetter = AccessTools.DeclaredPropertyGetter(typeof(ModsConfig), nameof(ModsConfig.IdeologyActive));

            if (ci.Calls(ideologyActiveGetter))
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
                // Push false (0) onto stack
                yield return new CodeInstruction(OpCodes.Ldc_I4_0);
                // Bitwise & the two values together
                yield return new CodeInstruction(OpCodes.And);
            }
            else yield return ci;
        }
    }

    private static void Insert(Dialog_BillConfig instance, Listing_Standard listing, ref Rect rect)
    {
        listing.Gap(rect.height - listing.CurHeight - 90f);
        var producedThing = instance.bill.recipe.ProducedThingDef;

        var relevantStyleCategories = StyleUtilities.FilterCategories(producedThing.RelevantStyleCategories);

        if (!StyleUtilities.CanBeStyled(producedThing, relevantStyleCategories))
        {
            var rect4 = listing.GetRect(30f);
            Widgets.DrawHighlight(rect4);
            Text.Anchor = TextAnchor.MiddleCenter;
            Widgets.Label(rect4, "NoStylesAvailable".Translate());
            Text.Anchor = TextAnchor.UpperLeft;
            return;
        }

        var global = Faction.OfPlayer.ideos.PrimaryIdeo.style.StyleForThingDef(producedThing);
        string text7 = global == null ? "Basic".Translate().CapitalizeFirst() : global.category.LabelCap;
        string text8 = instance.bill.style?.Category?.LabelCap ?? "Basic".Translate().CapitalizeFirst();
        string text9 = instance.bill.globalStyle ? "UseGlobalStyle".Translate().ToString() + " (" + text7 + ")" : text8;
        if (!instance.bill.globalStyle && instance.bill.style != null && instance.bill.graphicIndexOverride != null)
            text9 = text9 + " " + (instance.bill.graphicIndexOverride.Value + 1);

        if (!listing.ButtonText(text9))
            return;

        var options = new List<FloatMenuOption>();
        var stuff = producedThing.MadeFromStuff ? GenStuff.DefaultStuffFor(producedThing) : null;
        var color = stuff == null ? Color.white : producedThing.GetColorForStuff(stuff);

        AddOptions(producedThing.graphicData.Graphic, null, "Basic".Translate().CapitalizeFirst());

        if (producedThing.CanBeStyled())
        {
            if (!relevantStyleCategories.NullOrEmpty())
            {
                options.Add(new FloatMenuOption($"{"UseGlobalStyle".Translate()} ({text7})", () => SetStyle(instance.bill, global?.styleDef, null, true), instance.bill.recipe.UIIconThing, instance.bill.recipe.UIIcon, global?.styleDef, orderInPriority: 1000));

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
            options.Add(new FloatMenuOption("Empty".Translate(), () => { }) { Disabled = true});

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
                            $"{label} default (TODO: Translate)",
                            () => SetStyle(instance.bill, style, null, false),
                            icon,
                            color));
                    }

                    options.Add(new FloatMenuOption(
                        $"{label} {localIndex + 1}",
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