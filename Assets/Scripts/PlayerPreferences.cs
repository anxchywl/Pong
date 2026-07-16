using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pong
{
    public sealed class PlayerPreferences
    {
        private const string ModeKey = "pong.selection.mode";
        private const string ThemeKey = "pong.selection.theme";
        private const string CosmeticPrefix = "pong.selection.cosmetic.";
        private const string CosmeticIndexKey = "pong.selection.cosmetic.keys";
        private const string DefaultThemeId = "futuristic";

        public string SelectedModeId { get; private set; } = "classic";

        /// The committed selection. The workshop edits a clone and only calls Commit on Apply,
        /// so a previewed change that is never applied does not survive the session.
        public CosmeticSelection Selection { get; private set; } = new CosmeticSelection(DefaultThemeId);

        public string SelectedThemeId => Selection.ThemeId;

        public void Load()
        {
            SelectedModeId = PlayerPrefs.GetString(ModeKey, "classic");
            Selection = new CosmeticSelection(PlayerPrefs.GetString(ThemeKey, DefaultThemeId));

            string index = PlayerPrefs.GetString(CosmeticIndexKey, string.Empty);
            if (string.IsNullOrEmpty(index))
            {
                return;
            }

            foreach (string key in index.Split('|', StringSplitOptions.RemoveEmptyEntries))
            {
                if (!CosmeticSelection.TryParseKey(key, out string themeId, out CosmeticCategory category))
                {
                    continue;
                }

                string id = PlayerPrefs.GetString(CosmeticPrefix + key, string.Empty);
                if (!string.IsNullOrEmpty(id))
                {
                    Selection.Set(themeId, category, id);
                }
            }
        }

        public void SelectMode(string id)
        {
            SelectedModeId = id;
            PlayerPrefs.SetString(ModeKey, id);
            PlayerPrefs.Save();
        }

        public void SelectTheme(string id)
        {
            Selection.ThemeId = id;
            PlayerPrefs.SetString(ThemeKey, id);
            PlayerPrefs.Save();
        }

        /// Commits a draft from the workshop. An index of written keys is kept alongside them
        /// because PlayerPrefs cannot enumerate its own contents.
        public void Commit(CosmeticSelection selection)
        {
            Selection = selection.Clone();
            List<string> keys = new List<string>();

            foreach (KeyValuePair<string, string> entry in Selection.Entries)
            {
                PlayerPrefs.SetString(CosmeticPrefix + entry.Key, entry.Value);
                keys.Add(entry.Key);
            }

            PlayerPrefs.SetString(CosmeticIndexKey, string.Join("|", keys));
            PlayerPrefs.SetString(ThemeKey, Selection.ThemeId);
            PlayerPrefs.Save();
        }
    }
}
