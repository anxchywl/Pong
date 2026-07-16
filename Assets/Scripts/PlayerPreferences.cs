using UnityEngine;

namespace Pong
{
    public sealed class PlayerPreferences
    {
        private const string ModeKey = "pong.selection.mode";
        private const string ThemeKey = "pong.selection.theme";
        private const string CosmeticPrefix = "pong.selection.cosmetic.";

        public string SelectedModeId { get; private set; } = "classic";
        public string SelectedThemeId { get; private set; } = "futuristic";

        public void Load()
        {
            SelectedModeId = PlayerPrefs.GetString(ModeKey, "classic");
            SelectedThemeId = PlayerPrefs.GetString(ThemeKey, "futuristic");
        }

        public void SelectMode(string id)
        {
            SelectedModeId = id;
            PlayerPrefs.SetString(ModeKey, id);
            PlayerPrefs.Save();
        }

        public void SelectTheme(string id)
        {
            SelectedThemeId = id;
            PlayerPrefs.SetString(ThemeKey, id);
            PlayerPrefs.Save();
        }

        public string GetCosmetic(string themeId, CosmeticCategory category)
        {
            return PlayerPrefs.GetString(GetCosmeticKey(themeId, category), string.Empty);
        }

        public void SelectCosmetic(string themeId, CosmeticCategory category, string id)
        {
            PlayerPrefs.SetString(GetCosmeticKey(themeId, category), id);
            PlayerPrefs.Save();
        }

        private static string GetCosmeticKey(string themeId, CosmeticCategory category)
        {
            return CosmeticPrefix + themeId + "." + category.ToString().ToLowerInvariant();
        }
    }
}
