using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
    /// The workshop. Edits a draft selection and previews it live; the saved selection only
    /// changes on Apply, so a player can try anything and walk away from it.
    public sealed class CustomizationView
    {
        private const string ThemeCategory = "theme";

        private static readonly string[] Categories =
        {
            ThemeCategory, "arena", "paddle", "ball", "hud", "effects", "audio"
        };

        private readonly VisualElement root;
        private readonly ThemeCatalog themes;
        private readonly CosmeticCatalog cosmetics;
        private readonly PlayerPreferences preferences;
        private readonly Action<CosmeticSelection> preview;
        private readonly Action clickFeedback;
        private readonly VisualElement options;
        private readonly Button applyButton;
        private readonly Button resetButton;
        private readonly Label dirtyLabel;

        private CosmeticSelection draft;
        private string selectedCategory = ThemeCategory;

        public CustomizationView(
            VisualElement root,
            ThemeCatalog themes,
            CosmeticCatalog cosmetics,
            PlayerPreferences preferences,
            Action<CosmeticSelection> preview,
            Action clickFeedback
        )
        {
            this.root = root;
            this.themes = themes;
            this.cosmetics = cosmetics;
            this.preferences = preferences;
            this.preview = preview;
            this.clickFeedback = clickFeedback;

            options = root.Q<VisualElement>("shop-options");
            applyButton = root.Q<Button>("workshop-apply-button");
            resetButton = root.Q<Button>("workshop-reset-button");
            dirtyLabel = root.Q<Label>("workshop-dirty");

            draft = preferences.Selection.Clone();

            foreach (string id in Categories)
            {
                string captured = id;
                root.Q<Button>("shop-tab-" + id).clicked += () =>
                {
                    clickFeedback();
                    Select(captured);
                };
            }

            applyButton.clicked += Apply;
            resetButton.clicked += Reset;
            root.Q<Button>("workshop-randomize-button").clicked += Randomize;

            Select(Categories[0]);
        }

        /// Discards an unapplied draft. Leaving the screen must not leave the game wearing
        /// something the player never committed to.
        public void Discard()
        {
            draft = preferences.Selection.Clone();
            preview(draft);
            Render();
        }

        private void Select(string id)
        {
            selectedCategory = id;
            foreach (string category in Categories)
            {
                root.Q<Button>("shop-tab-" + category).EnableInClassList("is-selected", category == id);
            }

            Render();
        }

        private void Render()
        {
            options.Clear();

            if (selectedCategory == ThemeCategory)
            {
                RenderThemes();
            }
            else
            {
                RenderCosmetics(ParseCategory(selectedCategory));
            }

            bool dirty = !draft.Matches(preferences.Selection);
            applyButton.SetEnabled(dirty);
            resetButton.SetEnabled(dirty);
            dirtyLabel.text = dirty ? "UNAPPLIED CHANGES" : string.Empty;
            RenderPreview();
            RenderCollection();
        }

        /// Paints the preview court from the draft. Without this the centre of the workshop is a
        /// decorative picture that never changes, which is the opposite of what it promises.
        private void RenderPreview()
        {
            string themeId = draft.ThemeId;
            GameTheme theme = themes.Find(themeId) ?? themes.Default;
            if (theme == null)
            {
                return;
            }

            CosmeticDefinition paddle = Find(themeId, CosmeticCategory.Paddle);
            CosmeticDefinition ball = Find(themeId, CosmeticCategory.Ball);
            CosmeticDefinition arena = Find(themeId, CosmeticCategory.Arena);
            CosmeticDefinition effects = Find(themeId, CosmeticCategory.Effects);
            CosmeticDefinition hud = Find(themeId, CosmeticCategory.Hud);

            Paint("preview-paddle-left", paddle == null ? theme.PlayerColor : paddle.PrimaryColor);
            Paint("preview-paddle-right", paddle == null ? theme.OpponentColor : paddle.SecondaryColor);
            Paint("preview-ball", ball == null ? theme.BallColor : ball.PrimaryColor);
            Paint("preview-net", arena == null ? theme.CenterLineColor : arena.SecondaryColor);
            Paint("preview-court", effects == null ? theme.BackgroundColor : effects.PrimaryColor);

            root.Q<Label>("preview-score").style.color = hud == null ? theme.PrimaryAccent : hud.PrimaryColor;
        }

        private void Paint(string elementName, Color color)
        {
            VisualElement element = root.Q<VisualElement>(elementName);
            if (element != null)
            {
                element.style.backgroundColor = color;
            }
        }

        private CosmeticDefinition Find(string themeId, CosmeticCategory category)
        {
            return cosmetics.FindSelected(themeId, category, draft.Get(themeId, category));
        }

        private void RenderThemes()
        {
            foreach (GameTheme theme in themes.Themes)
            {
                GameTheme captured = theme;
                options.Add(BuildCard(
                    theme.DisplayName,
                    theme.PrimaryAccent,
                    theme.SecondaryAccent,
                    theme.Id == draft.ThemeId,
                    true,
                    () => SelectTheme(captured)
                ));
            }
        }

        private void RenderCosmetics(CosmeticCategory category)
        {
            string themeId = draft.ThemeId;
            CosmeticDefinition active = cosmetics.FindSelected(themeId, category, draft.Get(themeId, category));

            foreach (CosmeticDefinition cosmetic in cosmetics.Cosmetics)
            {
                if (cosmetic.ThemeId != themeId || cosmetic.Category != category)
                {
                    continue;
                }

                CosmeticDefinition captured = cosmetic;
                options.Add(BuildCard(
                    cosmetic.DisplayName,
                    cosmetic.PrimaryColor,
                    cosmetic.SecondaryColor,
                    cosmetic == active,
                    cosmetic.Unlocked,
                    () => SelectCosmetic(captured)
                ));
            }

            if (options.childCount == 0)
            {
                Label empty = new Label("This world has nothing here yet.");
                empty.AddToClassList("collection__note");
                options.Add(empty);
            }
        }

        private Button BuildCard(
            string title,
            Color primary,
            Color secondary,
            bool selected,
            bool unlocked,
            Action activate
        )
        {
            Button card = new Button(() =>
            {
                if (!unlocked)
                {
                    return;
                }

                clickFeedback();
                activate();
            });
            card.AddToClassList("option-card");
            card.EnableInClassList("is-selected", selected);
            card.EnableInClassList("is-locked", !unlocked);

            VisualElement swatches = new VisualElement();
            swatches.AddToClassList("option-card__swatches");
            swatches.Add(Swatch(primary));
            swatches.Add(Swatch(secondary));

            Label label = new Label(unlocked ? title : "LOCKED");
            label.AddToClassList("option-card__title");

            card.Add(swatches);
            card.Add(label);
            return card;
        }

        private static VisualElement Swatch(Color color)
        {
            VisualElement swatch = new VisualElement();
            swatch.AddToClassList("cosmetic-card__swatch");
            swatch.style.backgroundColor = color;
            return swatch;
        }

        private void RenderCollection()
        {
            string themeId = draft.ThemeId;
            GameTheme theme = themes.Find(themeId);
            root.Q<Label>("collection-theme-value").text = theme == null ? themeId : theme.DisplayName;

            if (selectedCategory == ThemeCategory)
            {
                root.Q<Label>("collection-title").text = theme == null ? "—" : theme.DisplayName;
                root.Q<Label>("collection-description").text = theme == null ? string.Empty : theme.Description;
                RenderSwatches(
                    theme == null ? Color.white : theme.PrimaryAccent,
                    theme == null ? Color.white : theme.SecondaryAccent
                );
                root.Q<Label>("collection-owned-value").text = $"{themes.Themes.Count} / {themes.Themes.Count}";
                return;
            }

            CosmeticCategory category = ParseCategory(selectedCategory);
            CosmeticDefinition active = cosmetics.FindSelected(themeId, category, draft.Get(themeId, category));
            int owned = 0;
            int total = 0;
            foreach (CosmeticDefinition cosmetic in cosmetics.Cosmetics)
            {
                if (cosmetic.ThemeId != themeId || cosmetic.Category != category)
                {
                    continue;
                }

                total++;
                if (cosmetic.Unlocked)
                {
                    owned++;
                }
            }

            root.Q<Label>("collection-title").text = active == null ? "—" : active.DisplayName;
            root.Q<Label>("collection-description").text = active == null ? string.Empty : active.Description;
            RenderSwatches(
                active == null ? Color.clear : active.PrimaryColor,
                active == null ? Color.clear : active.SecondaryColor
            );
            root.Q<Label>("collection-owned-value").text = $"{owned} / {total}";
        }

        private void RenderSwatches(Color primary, Color secondary)
        {
            VisualElement host = root.Q<VisualElement>("collection-swatches");
            host.Clear();
            host.Add(Swatch(primary));
            host.Add(Swatch(secondary));
        }

        private void SelectTheme(GameTheme theme)
        {
            draft.ThemeId = theme.Id;
            PreviewAndRender();
        }

        private void SelectCosmetic(CosmeticDefinition cosmetic)
        {
            draft.Set(cosmetic.ThemeId, cosmetic.Category, cosmetic.Id);
            PreviewAndRender();
        }

        /// Rolls every unlocked cosmetic in the current world. Confined to one theme, so a
        /// shuffle can never assemble a look that crosses two design systems.
        private void Randomize()
        {
            clickFeedback();
            string themeId = draft.ThemeId;

            foreach (CosmeticCategory category in Enum.GetValues(typeof(CosmeticCategory)))
            {
                List<CosmeticDefinition> available = new List<CosmeticDefinition>();
                foreach (CosmeticDefinition cosmetic in cosmetics.Cosmetics)
                {
                    if (cosmetic.ThemeId == themeId && cosmetic.Category == category && cosmetic.Unlocked)
                    {
                        available.Add(cosmetic);
                    }
                }

                if (available.Count > 0)
                {
                    draft.Set(themeId, category, available[UnityEngine.Random.Range(0, available.Count)].Id);
                }
            }

            PreviewAndRender();
        }

        private void Reset()
        {
            clickFeedback();
            Discard();
        }

        private void Apply()
        {
            clickFeedback();
            preferences.Commit(draft);
            draft = preferences.Selection.Clone();
            PreviewAndRender();
        }

        private void PreviewAndRender()
        {
            preview(draft);
            Render();
        }

        private static CosmeticCategory ParseCategory(string id)
        {
            return (CosmeticCategory)Enum.Parse(typeof(CosmeticCategory), id, true);
        }
    }
}
