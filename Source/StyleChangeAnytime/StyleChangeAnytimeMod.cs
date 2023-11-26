using HarmonyLib;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime;

public class StyleChangeAnytimeMod : Mod
{
    private static Harmony harmony;
    internal static Harmony Harmony => harmony ??= new Harmony("Dra.StyleChangeAnytime");
    public static StyleChangeAnytimeSettings settings;

    public StyleChangeAnytimeMod(ModContentPack content) : base(content)
    {
        settings = GetSettings<StyleChangeAnytimeSettings>();

        Harmony.PatchAll();

        if (!ModsConfig.IdeologyActive)
            Log.Error("[Style Change Anytime] - Ideology is inactive, this mod is completely pointless.");
    }

    public override void DoSettingsWindowContents(Rect inRect) => settings.DoSettingsWindowContents(inRect);

    public override string SettingsCategory() => "Style Change Anytime";
}