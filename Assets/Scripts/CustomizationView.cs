using System;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class CustomizationView
    {
        private readonly VisualElement root;
        private readonly ThemeCatalog themes;
        private readonly CosmeticCatalog cosmetics;
        private readonly PlayerPreferences preferences;
        private readonly Action selectionChanged;
        private readonly Action clickFeedback;
        private readonly VisualElement cardContainer;
        private CosmeticCategory category = CosmeticCategory.Paddle;
        private bool showingThemes;

        public CustomizationView(
            VisualElement root,
            ThemeCatalog themes,
            CosmeticCatalog cosmetics,
            PlayerPreferences preferences,
            Action selectionChanged,
            Action clickFeedback
        )
        {
            this.root = root;
            this.themes = themes;
            this.cosmetics = cosmetics;
            this.preferences = preferences;
            this.selectionChanged = selectionChanged;
            this.clickFeedback = clickFeedback;
            cardContainer = root.Q<VisualElement>("cosmetic-card-list");

            BindCategory("paddle-tab", CosmeticCategory.Paddle);
            BindCategory("ball-tab", CosmeticCategory.Ball);
            BindCategory("arena-tab", CosmeticCategory.Arena);
            BindCategory("background-tab", CosmeticCategory.Background);
            root.Q<Button>("theme-tab").clicked += () =>
            {
                clickFeedback();
                showingThemes = true;
                Render();
            };
            Render();
        }

        public void ShowCategory(CosmeticCategory value)
        {
            category = value;
            showingThemes = false;
            Render();
        }

        public void ShowThemes()
        {
            showingThemes = true;
            Render();
        }

        private void BindCategory(string buttonName, CosmeticCategory value)
        {
            root.Q<Button>(buttonName).clicked += () =>
            {
                clickFeedback();
                ShowCategory(value);
            };
        }

        private void Render()
        {
            cardContainer.Clear();
            UpdateTabs();

            if (showingThemes)
            {
                RenderThemes();
                return;
            }

            RenderCosmetics();
        }

        private void RenderThemes()
        {
            foreach (GameTheme theme in themes.Themes)
            {
                Button card = new Button(() => SelectTheme(theme));
                card.AddToClassList("selection-card");
                card.AddToClassList("theme-card");
                card.EnableInClassList("is-selected", theme.Id == preferences.SelectedThemeId);

                Label identity = new Label("COMPLETE VISUAL WORLD");
                identity.AddToClassList("selection-card__eyebrow");
                Label title = new Label(theme.DisplayName);
                title.AddToClassList("selection-card__title");
                Label description = new Label(theme.Description);
                description.AddToClassList("selection-card__description");
                VisualElement palette = CreatePalette(theme.PrimaryAccent, theme.SecondaryAccent);

                card.Add(identity);
                card.Add(title);
                card.Add(description);
                card.Add(palette);
                cardContainer.Add(card);
            }
        }

        private void RenderCosmetics()
        {
            string themeId = preferences.SelectedThemeId;
            string selectedId = preferences.GetCosmetic(themeId, category);
            CosmeticDefinition fallback = cosmetics.FindSelected(themeId, category, selectedId);

            foreach (CosmeticDefinition cosmetic in cosmetics.Cosmetics)
            {
                if (cosmetic.ThemeId != themeId || cosmetic.Category != category)
                {
                    continue;
                }

                Button card = new Button(() => SelectCosmetic(cosmetic));
                card.AddToClassList("cosmetic-card");
                card.EnableInClassList("is-selected", cosmetic.Id == selectedId ||
                    string.IsNullOrEmpty(selectedId) && cosmetic == fallback);
                card.SetEnabled(cosmetic.Unlocked);

                VisualElement palette = CreatePalette(cosmetic.PrimaryColor, cosmetic.SecondaryColor);
                Label title = new Label(cosmetic.DisplayName);
                title.AddToClassList("cosmetic-card__title");
                Label description = new Label(cosmetic.Unlocked ? cosmetic.Description : "Unlock coming later");
                description.AddToClassList("cosmetic-card__description");
                card.Add(palette);
                card.Add(title);
                card.Add(description);
                cardContainer.Add(card);
            }
        }

        private void UpdateTabs()
        {
            root.Q<Button>("theme-tab").EnableInClassList("is-selected", showingThemes);
            foreach (CosmeticCategory value in Enum.GetValues(typeof(CosmeticCategory)))
            {
                root.Q<Button>(GetTabName(value)).EnableInClassList("is-selected", !showingThemes && value == category);
            }
        }

        private void SelectTheme(GameTheme theme)
        {
            clickFeedback();
            preferences.SelectTheme(theme.Id);
            selectionChanged();
            Render();
        }

        private void SelectCosmetic(CosmeticDefinition cosmetic)
        {
            if (!cosmetic.Unlocked)
            {
                return;
            }

            clickFeedback();
            preferences.SelectCosmetic(cosmetic.ThemeId, cosmetic.Category, cosmetic.Id);
            selectionChanged();
            Render();
        }

        private static VisualElement CreatePalette(UnityEngine.Color primaryColor, UnityEngine.Color secondaryColor)
        {
            VisualElement palette = new VisualElement();
            palette.AddToClassList("cosmetic-card__swatches");
            VisualElement primary = new VisualElement();
            primary.AddToClassList("cosmetic-card__swatch");
            primary.style.backgroundColor = primaryColor;
            VisualElement secondary = new VisualElement();
            secondary.AddToClassList("cosmetic-card__swatch");
            secondary.style.backgroundColor = secondaryColor;
            palette.Add(primary);
            palette.Add(secondary);
            return palette;
        }

        private static string GetTabName(CosmeticCategory value)
        {
            return value switch
            {
                CosmeticCategory.Paddle => "paddle-tab",
                CosmeticCategory.Ball => "ball-tab",
                CosmeticCategory.Arena => "arena-tab",
                CosmeticCategory.Background => "background-tab",
                _ => string.Empty
            };
        }
    }
}
