using HarmonyLib;
using JetBrains.Annotations;
using UnityEngine;
using Verse;

namespace StyleChangeAnytime
{
    [UsedImplicitly]
    public class StyleChangeAnytimeMod : Mod
    {
        private static Harmony harmony;
        internal static Harmony Harmony => harmony ??= new Harmony("Dra.AlwaysAvailableIdeoChange");
        public static StyleChangeAnytimeSettings anytimeSettings;

        public StyleChangeAnytimeMod(ModContentPack content) : base(content)
        {
            anytimeSettings = GetSettings<StyleChangeAnytimeSettings>();

            Harmony.PatchAll();

            if (!ModsConfig.IdeologyActive)
                Log.Error("[Always Available Style Change] - Ideology is inactive, this mod is completely pointless.");
        }

        public override void DoSettingsWindowContents(Rect inRect) => anytimeSettings.DoSettingsWindowContents(inRect);

        public override string SettingsCategory() => "Always Available Style Change";
    }
}