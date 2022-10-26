using UnityEngine;
using Verse;

namespace StyleChangeAnytime
{
    public class StyleChangeAnytimeSettings : ModSettings
    {
        public bool showAllStyles = false;
        public bool usePrimaryIdeoOnly = true;

        public override void ExposeData()
        {
            base.ExposeData();
            
            Scribe_Values.Look(ref showAllStyles, nameof(showAllStyles), false);
            Scribe_Values.Look(ref usePrimaryIdeoOnly, nameof(usePrimaryIdeoOnly), true);
        }

        public void DoSettingsWindowContents(Rect inRect)
        {
            var listing = new Listing_Standard();
            listing.Begin(inRect);
            listing.ColumnWidth = 270f;

            listing.CheckboxLabeled(
                "AlwaysAvailableStyleChangeShowAllStyles".Translate().CapitalizeFirst(),
                ref showAllStyles, 
                "AlwaysAvailableStyleChangeShowAllStylesTooltip".Translate().CapitalizeFirst());
            listing.CheckboxLabeled(
                "AlwaysAvailableStyleChangeUsePrimaryIdeoOnly".Translate().CapitalizeFirst(),
                ref usePrimaryIdeoOnly,
                "AlwaysAvailableStyleChangeUsePrimaryIdeoOnlyTooltip".Translate().CapitalizeFirst());

            listing.End();
        }
    }
}