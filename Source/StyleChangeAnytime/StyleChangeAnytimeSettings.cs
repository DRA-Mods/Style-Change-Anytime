using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

public class StyleChangeAnytimeSettings : ModSettings
{
    public bool showAllStyles = false;
    public bool usePrimaryIdeoOnly = true;

    public bool showOnBlueprints = true;
    public bool showOnBillConfig = true;

    public bool devModeLogs = false;

    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref showAllStyles, nameof(showAllStyles), false);
        Scribe_Values.Look(ref usePrimaryIdeoOnly, nameof(usePrimaryIdeoOnly), true);

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
    }
}