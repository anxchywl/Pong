using System.Collections.Generic;
using UnityEngine;

namespace Pong
{
    [CreateAssetMenu(menuName = "Pong/UI/Theme Catalog")]
    public sealed class ThemeCatalog : ScriptableObject
    {
        [SerializeField] private List<GameTheme> themes = new List<GameTheme>();

        public IReadOnlyList<GameTheme> Themes => themes;

        public GameTheme Find(string id)
        {
            return themes.Find(theme => theme != null && theme.Id == id);
        }

        public GameTheme Default => themes.Count == 0 ? null : themes[0];
    }
}
