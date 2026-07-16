using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pong
{
    /// The workshop's categories, in rail order. Theme is not here: it selects a whole design
    /// system rather than a cosmetic within one.
    public enum CosmeticCategory
    {
        Arena,
        Paddle,
        Ball,
        Hud,
        Effects,
        Audio
    }

    [CreateAssetMenu(menuName = "Pong/UI/Cosmetic Catalog")]
    public sealed class CosmeticCatalog : ScriptableObject
    {
        [SerializeField] private List<CosmeticDefinition> cosmetics = new List<CosmeticDefinition>();

        public IReadOnlyList<CosmeticDefinition> Cosmetics => cosmetics;

        public CosmeticDefinition Find(string id)
        {
            return cosmetics.Find(cosmetic => cosmetic.Id == id);
        }

        public CosmeticDefinition FindSelected(string themeId, CosmeticCategory category, string id)
        {
            CosmeticDefinition selected = cosmetics.Find(cosmetic =>
                cosmetic.ThemeId == themeId && cosmetic.Category == category && cosmetic.Id == id);
            return selected ?? cosmetics.Find(cosmetic =>
                cosmetic.ThemeId == themeId && cosmetic.Category == category && cosmetic.Unlocked);
        }
    }

    [Serializable]
    public sealed class CosmeticDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string themeId;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private CosmeticCategory category;
        [SerializeField] private Color primaryColor = Color.white;
        [SerializeField] private Color secondaryColor = Color.white;
        [SerializeField] private bool unlocked = true;
        [SerializeField, Range(0f, 1f)] private float effectIntensity = 0.5f;

        public string Id => id;
        public string ThemeId => themeId;
        public string DisplayName => displayName;
        public string Description => description;
        public CosmeticCategory Category => category;
        public Color PrimaryColor => primaryColor;
        public Color SecondaryColor => secondaryColor;
        public bool Unlocked => unlocked;
        public float EffectIntensity => effectIntensity;
    }
}
