using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class UiThemePresenter
    {
        private readonly VisualElement root;
        private readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();
        private StyleSheet activeStyleSheet;

        public UiThemePresenter(VisualElement root)
        {
            this.root = root;
        }

        public void Apply(GameTheme theme, float effectIntensity)
        {
            if (activeStyleSheet != null)
            {
                root.styleSheets.Remove(activeStyleSheet);
            }

            activeStyleSheet = theme.StyleSheet;
            if (activeStyleSheet != null)
            {
                root.styleSheets.Add(activeStyleSheet);
            }

            ApplyFont(theme);
            ApplyCopy(theme);
            BuildOverlay(theme, effectIntensity);
        }

        private void ApplyFont(GameTheme theme)
        {
            if (!fonts.TryGetValue(theme.Id, out Font font))
            {
                font = Font.CreateDynamicFontFromOSFont(theme.PreferredFontNames, 18);
                if (font != null && font.material != null && font.material.mainTexture != null)
                {
                    font.material.mainTexture.filterMode = theme.OverlayPattern == ThemeOverlayPattern.Scanlines
                        ? FilterMode.Point
                        : FilterMode.Bilinear;
                }
                fonts.Add(theme.Id, font);
            }

            root.style.unityFont = font;
        }

        private void ApplyCopy(GameTheme theme)
        {
            root.Q<Label>("menu-eyebrow").text = theme.MenuEyebrow;
            root.Q<Label>("menu-title").text = theme.MenuTitle;
            root.Q<Label>("menu-subtitle").text = theme.MenuSubtitle;
            root.Q<Label>("pause-eyebrow").text = theme.PauseEyebrow;
            root.Q<Label>("pause-title").text = theme.PauseTitle;
            root.Q<Label>("left-score-caption").text = theme.PlayerScoreLabel;
            root.Q<Label>("right-score-caption").text = theme.OpponentScoreLabel;
            root.Q<Button>("play-button").text = $"{theme.PlayIcon}  PLAY";
            root.Q<Button>("game-mode-button").text = $"{theme.GameModeIcon}  GAME MODE";
            root.Q<Button>("skins-button").text = $"{theme.SkinsIcon}  SKINS";
            root.Q<Button>("background-button").text = $"{theme.BackgroundIcon}  BACKGROUND";
            root.Q<Button>("theme-button").text = $"{theme.ThemeIcon}  WORLD";
            root.Q<Button>("settings-button").text = $"{theme.SettingsIcon}  SETTINGS";
            root.Q<Button>("credits-button").text = $"{theme.CreditsIcon}  CREDITS";
            root.Q<Button>("quit-button").text = $"{theme.QuitIcon}  QUIT";
            root.Q<Button>("hud-pause-button").text = theme.PauseIcon;
            root.Q<Button>("resume-button").text = $"{theme.PlayIcon}  RESUME";
            root.Q<Button>("pause-restart-button").text = $"{theme.RestartIcon}  RESTART";
            root.Q<Button>("pause-settings-button").text = $"{theme.SettingsIcon}  SETTINGS";
            root.Q<Button>("pause-main-menu-button").text = $"{theme.HomeIcon}  MAIN MENU";
            root.Q<Button>("pause-quit-button").text = $"{theme.QuitIcon}  QUIT";
            root.Q<Button>("win-restart-button").text = $"{theme.RestartIcon}  PLAY AGAIN";
            root.Q<Button>("win-main-menu-button").text = $"{theme.HomeIcon}  MAIN MENU";

            root.Query<Button>(className: "back-button").ForEach(button =>
                button.text = $"{theme.BackIcon}  BACK");
        }

        private void BuildOverlay(GameTheme theme, float effectIntensity)
        {
            VisualElement overlay = root.Q<VisualElement>("theme-overlay");
            overlay.Clear();
            overlay.style.opacity = theme.OverlayIntensity * Mathf.Lerp(0.65f, 1.25f, effectIntensity);
            overlay.ClearClassList();
            overlay.AddToClassList("theme-overlay");

            if (theme.OverlayPattern == ThemeOverlayPattern.Scanlines)
            {
                overlay.AddToClassList("theme-overlay--scanlines");
                for (int index = 0; index < 72; index++)
                {
                    VisualElement line = new VisualElement();
                    line.AddToClassList("theme-overlay__scanline");
                    overlay.Add(line);
                }
                return;
            }

            overlay.AddToClassList("theme-overlay--circuit");
            for (int index = 0; index < 8; index++)
            {
                VisualElement vertical = new VisualElement();
                vertical.AddToClassList("theme-overlay__circuit-line");
                vertical.AddToClassList("theme-overlay__circuit-line--vertical");
                vertical.style.left = Length.Percent(10f + index * 12.5f);
                overlay.Add(vertical);

                VisualElement horizontal = new VisualElement();
                horizontal.AddToClassList("theme-overlay__circuit-line");
                horizontal.AddToClassList("theme-overlay__circuit-line--horizontal");
                horizontal.style.top = Length.Percent(12f + index * 12f);
                overlay.Add(horizontal);
            }
        }
    }
}
