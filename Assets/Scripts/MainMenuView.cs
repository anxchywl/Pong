using System;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class MainMenuView
    {
        private readonly Label modeValue;
        private readonly Label themeValue;

        public MainMenuView(
            VisualElement root,
            Action play,
            Action gameMode,
            Action skins,
            Action background,
            Action themes,
            Action settings,
            Action credits,
            Action quit,
            Action clickFeedback
        )
        {
            modeValue = root.Q<Label>("selected-mode-value");
            themeValue = root.Q<Label>("selected-theme-value");
            Bind(root.Q<Button>("play-button"), play, clickFeedback);
            Bind(root.Q<Button>("game-mode-button"), gameMode, clickFeedback);
            Bind(root.Q<Button>("skins-button"), skins, clickFeedback);
            Bind(root.Q<Button>("background-button"), background, clickFeedback);
            Bind(root.Q<Button>("theme-button"), themes, clickFeedback);
            Bind(root.Q<Button>("settings-button"), settings, clickFeedback);
            Bind(root.Q<Button>("credits-button"), credits, clickFeedback);
            Bind(root.Q<Button>("quit-button"), quit, clickFeedback);
        }

        public void SetSelectedMode(string modeName)
        {
            modeValue.text = modeName;
        }

        public void SetSelectedTheme(string themeName)
        {
            themeValue.text = themeName;
        }

        private static void Bind(Button button, Action action, Action clickFeedback)
        {
            button.clicked += () =>
            {
                clickFeedback();
                action();
            };
        }
    }
}
