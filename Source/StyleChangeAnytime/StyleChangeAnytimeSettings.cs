using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

public class StyleChangeAnytimeSettings : ModSettings
{
    public bool showAllStyles = false;
    public bool usePrimaryIdeoOnly = true;
    public bool ignoreFactionCheck = false;

    public ShowRestrictions showOnBuildings = ShowRestrictions.Always;
    public ShowRestrictions showOnBlueprints = ShowRestrictions.Always;
    public ShowRestrictions showOnFrames = ShowRestrictions.Always;
    public ShowRestrictions showOnItems = ShowRestrictions.Always;
    public ShowRestrictions showOnPlants = ShowRestrictions.Never;
    public ShowRestrictions showOnChunks = ShowRestrictions.Never;
    public ShowRestrictions showOnBillConfig = ShowRestrictions.Always;

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
        Scribe_Values.Look(ref ignoreFactionCheck, nameof(ignoreFactionCheck), false);

        Scribe_Values.Look(ref showOnBuildings, nameof(showOnBuildings), ShowRestrictions.Always);
        Scribe_Values.Look(ref showOnBlueprints, nameof(showOnBlueprints), ShowRestrictions.Always);
        Scribe_Values.Look(ref showOnFrames, nameof(showOnFrames), ShowRestrictions.Always);
        Scribe_Values.Look(ref showOnItems, nameof(showOnItems), ShowRestrictions.Always);
        Scribe_Values.Look(ref showOnPlants, nameof(showOnPlants), ShowRestrictions.Never);
        Scribe_Values.Look(ref showOnChunks, nameof(showOnChunks), ShowRestrictions.Never);
        Scribe_Values.Look(ref showOnBillConfig, nameof(showOnBillConfig), ShowRestrictions.Always);

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
        listing.CheckboxLabeled(
            "StyleChangeAnytimeIgnoreFactionCheck".Translate().CapitalizeFirst(),
            ref ignoreFactionCheck,
            "StyleChangeAnytimeIgnoreFactionCheckTooltip".Translate().CapitalizeFirst());

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
                "StyleChangeAnytimeApplyToBlueprints".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnBlueprints}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToBlueprintsTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnBlueprints = val);
        }

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToFrames".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnFrames}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToFramesTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnFrames = val);
        }

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToItems".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnItems}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToItemsTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnItems = val);
        }

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToPlants".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnPlants}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToPlantsTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnPlants = val);
        }

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToChunks".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnChunks}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToChunksTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnChunks = val);
        }

        if (listing.ButtonTextLabeledPct(
                "StyleChangeAnytimeApplyToBills".Translate().CapitalizeFirst(),
                $"StyleChangeAnytimeApply{showOnBillConfig}".Translate().CapitalizeFirst(),
                0.7f,
                tooltip: "StyleChangeAnytimeApplyToBillsTooltip".Translate().CapitalizeFirst()))
        {
            HandleShowRestrictionsMenu(val => showOnBillConfig = val);
        }

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