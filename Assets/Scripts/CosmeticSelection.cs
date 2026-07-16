using System;
using System.Collections.Generic;

namespace Pong
{
    /// What a player has chosen: a theme, and one cosmetic per category within each theme.
    /// Copyable so the workshop can edit a draft and preview it live while the saved
    /// selection stays untouched until Apply.
    public sealed class CosmeticSelection
    {
        private readonly Dictionary<string, string> values;

        public CosmeticSelection(string themeId)
        {
            ThemeId = themeId;
            values = new Dictionary<string, string>();
        }

        private CosmeticSelection(string themeId, Dictionary<string, string> source)
        {
            ThemeId = themeId;
            values = new Dictionary<string, string>(source);
        }

        public string ThemeId { get; set; }

        public string Get(string themeId, CosmeticCategory category)
        {
            return values.TryGetValue(Key(themeId, category), out string id) ? id : string.Empty;
        }

        public void Set(string themeId, CosmeticCategory category, string id)
        {
            values[Key(themeId, category)] = id;
        }

        public CosmeticSelection Clone()
        {
            return new CosmeticSelection(ThemeId, values);
        }

        /// True when a draft differs from what is saved, so Apply can be offered only when
        /// there is something to apply.
        public bool Matches(CosmeticSelection other)
        {
            if (other == null || other.ThemeId != ThemeId || other.values.Count != values.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, string> pair in values)
            {
                if (!other.values.TryGetValue(pair.Key, out string id) || id != pair.Value)
                {
                    return false;
                }
            }

            return true;
        }

        public IEnumerable<KeyValuePair<string, string>> Entries => values;

        public static string Key(string themeId, CosmeticCategory category)
        {
            return themeId + "." + category.ToString().ToLowerInvariant();
        }

        public static bool TryParseKey(string key, out string themeId, out CosmeticCategory category)
        {
            themeId = string.Empty;
            category = default;
            int split = key.LastIndexOf('.');
            if (split <= 0)
            {
                return false;
            }

            themeId = key.Substring(0, split);
            return Enum.TryParse(key.Substring(split + 1), true, out category);
        }
    }
}
