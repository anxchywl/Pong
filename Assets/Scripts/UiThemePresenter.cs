using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class UiThemePresenter
    {
        private readonly VisualElement root;
        private readonly PanelSettings panel;
        private readonly Dictionary<string, Font> fonts = new Dictionary<string, Font>();

        public UiThemePresenter(VisualElement root, PanelSettings panel)
        {
            this.root = root;
            this.panel = panel;
        }

        public void Apply(GameTheme theme, float effectIntensity)
        {
            // one assignment swaps the controls, the shared structure and this theme's language
            // together, rather than adding and removing sheets behind the panel's back
            if (theme.ThemeStyleSheet != null && panel.themeStyleSheet != theme.ThemeStyleSheet)
            {
                panel.themeStyleSheet = theme.ThemeStyleSheet;
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
            foreach (ThemeCopyEntry entry in theme.Copy)
            {
                string text = entry.Compose();
                root.Query<VisualElement>(name: entry.Element).ForEach(element => SetText(element, text));
                root.Query<VisualElement>(className: entry.Element).ForEach(element => SetText(element, text));
            }
        }

        private static void SetText(VisualElement element, string text)
        {
            switch (element)
            {
                case Button button:
                    button.text = text;
                    break;
                case Label label:
                    label.text = text;
                    break;
            }
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
