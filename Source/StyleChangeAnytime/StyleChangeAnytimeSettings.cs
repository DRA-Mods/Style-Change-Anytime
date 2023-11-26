using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

public class StyleChangeAnytimeSettings : ModSettings
{
    public bool showAllStyles = false;
    public bool usePrimaryIdeoOnly = true;

    public ShowRestrictions showOnBuildings = ShowRestrictions.Always;
    public ShowRestrictions showOnItems = ShowRestrictions.Always;
    public bool showOnBlueprints = true;
    public bool showOnBillConfig = true;

    public bool devModeLogs = false;

    public enum ShowRestrictions : byte
    {
        Never,
        ClassicMode,
        Always,
    }

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref showAllStyles, nameof(showAllStyles), false);
        Scribe_Values.Look(ref usePrimaryIdeoOnly, nameof(usePrimaryIdeoOnly), true);

        Scribe_Values.Look(ref showOnBuildings, nameof(showOnBuildings), ShowRestrictions.Always);
        Scribe_Values.Look(ref showOnItems, nameof(showOnItems), ShowRestrictions.Always);
        Scribe_Values.Look(ref showOnBlueprints, nameof(showOnBlueprints), true);
        Scribe_Values.Look(ref showOnBillConfig, nameof(showOnBillConfig), true);

        Scribe_Values.Look(ref devModeLogs, nameof(devModeLogs), false);
    }

    public void DoSettingsWindowContents(Rect inRect)
    {
        var listing = new Listing_Standard();
        listing.Begin(inRect);
        listing.ColumnWidth = 270f;

        listing.CheckboxLabeled(
            "StyleChangeAnytimeShowAllStyles".Translate().CapitalizeFirst(),
            ref showAllStyles,
            "StyleChangeAnytimeShowAllStylesTooltip".Translate().CapitalizeFirst());
        listing.CheckboxLabeled(
            "StyleChangeAnytimeUsePrimaryIdeoOnly".Translate().CapitalizeFirst(),
            ref usePrimaryIdeoOnly,
            "StyleChangeAnytimeUsePrimaryIdeoOnlyTooltip".Translate().CapitalizeFirst());

        listing.GapLine();

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToBuildings".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnBuildings}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToBuildingsTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnBuildings = val);
        }

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToItems".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnItems}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToItemsTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnItems = val);
        }

        listing.CheckboxLabeled(
            "StyleChangeAnytimeApplyToBlueprints".Translate().CapitalizeFirst(),
            ref showOnBlueprints,
            "StyleChangeAnytimeApplyToBlueprintsTooltip".Translate().CapitalizeFirst());
        listing.CheckboxLabeled(
            "StyleChangeAnytimeApplyToBills".Translate().CapitalizeFirst(),
            ref showOnBillConfig,
            "StyleChangeAnytimeApplyToBillsTooltip".Translate().CapitalizeFirst());

        if (devModeLogs || Prefs.DevMode)
        {
            listing.GapLine();

            listing.CheckboxLabeled(
                "StyleChangeAnytimeDevModeLogs".Translate().CapitalizeFirst(),
                ref devModeLogs,
                "StyleChangeAnytimeDevModeLogsTooltip".Translate().CapitalizeFirst());
        }

        listing.End();
        
        return;

        void HandleShowRestrictionsMenu(Action<ShowRestrictions> action)
        {
            var options = new List<FloatMenuOption>();
            foreach (ShowRestrictions value in Enum.GetValues(typeof(ShowRestrictions)))
            {
                var current = value;
                options.Add(new FloatMenuOption(
                    $"StyleChangeAnytimeApply{current}".Translate().CapitalizeFirst(),
                    () => action(current)));
            }

            Find.WindowStack.Add(new FloatMenu(options));
        }
    }
}