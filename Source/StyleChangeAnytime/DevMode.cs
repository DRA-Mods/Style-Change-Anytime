using LudeonTK;
using RimWorld;
using Verse;

namespace StyleChangeAnytime;

public class DevMode
{
    private const string Category = "Style Change Anytime";

    [DebugAction(Category, allowedGameStates = AllowedGameStates.Playing, actionType = DebugActionType.ToolMap)]
    private static void LogStyleChangeCategory()
    {
        var map = Find.CurrentMap;
        var cell = UI.MouseCell();
        if (!cell.InBounds(map))
            return;

        foreach (var thing in cell.GetThingList(map))
        {
            Log.Message($"Thing {thing} category is {GetCategoryStringFor(thing)} and it is allowed {TextForCategory()}. Its style categories are {thing.def?.RelevantStyleCategories.ToStringSafeEnumerable()}.");

            string TextForCategory()
                => StyleUtilities.GetCategoryFor(thing) switch
                {
                    StyleChangeAnytimeSettings.ShowRestrictions.Never => "never",
                    StyleChangeAnytimeSettings.ShowRestrictions.ClassicMode => "in classic mode only",
                    StyleChangeAnytimeSettings.ShowRestrictions.Always => "always",
                    _ => "incorrect output?"
                };
        }
    }

    private static string GetCategoryStringFor(Thing thing)
        => (thing, thing.def.category) switch
        {
            _ when thing.def.thingCategories.NotNullAndContains(ThingCategoryDefOf.Chunks) => "Chunk",
            (Frame, _) => "Frame",
            (Blueprint, _) => "Blueprint",
            (Building, _) => "Building",
            (Plant, _) => "Plant",
            (_, ThingCategory.Item) => "Item",
            (_, ThingCategory.Building) => "Building",
            (_, ThingCategory.Plant) => "Plant",
            _ => "Other/Unknown",
        };
}