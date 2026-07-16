using System.Collections.Generic;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class ScreenHost
    {
        private readonly Dictionary<AppScreen, VisualElement> screens;

        public ScreenHost(VisualElement root)
        {
            screens = new Dictionary<AppScreen, VisualElement>
            {
                [AppScreen.MainMenu] = root.Q<VisualElement>("main-menu-screen"),
                [AppScreen.GameMode] = root.Q<VisualElement>("game-mode-screen"),
                [AppScreen.Customization] = root.Q<VisualElement>("customization-screen"),
                [AppScreen.Settings] = root.Q<VisualElement>("settings-screen"),
                [AppScreen.Credits] = root.Q<VisualElement>("credits-screen")
            };
        }

        public void Show(AppScreen screen)
        {
            foreach (KeyValuePair<AppScreen, VisualElement> item in screens)
            {
                item.Value.EnableInClassList("is-hidden", item.Key != screen);
            }

            VisualElement activeScreen = screens[screen];
            activeScreen.schedule.Execute(() => FocusFirstControl(activeScreen));
        }

        private static void FocusFirstControl(VisualElement screen)
        {
            Button firstButton = null;
            screen.Query<Button>().ForEach(button =>
            {
                if (firstButton == null && button.enabledSelf)
                {
                    firstButton = button;
                }
            });
            firstButton?.Focus();
        }
    }
}
