using System;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class PauseMenuView
    {
        private readonly VisualElement root;

        public PauseMenuView(
            VisualElement documentRoot,
            Action resume,
            Action restart,
            Action settings,
            Action mainMenu,
            Action quit,
            Action clickFeedback
        )
        {
            root = documentRoot.Q<VisualElement>("pause-menu");
            Bind(documentRoot.Q<Button>("resume-button"), resume, clickFeedback);
            Bind(documentRoot.Q<Button>("pause-restart-button"), restart, clickFeedback);
            Bind(documentRoot.Q<Button>("pause-settings-button"), settings, clickFeedback);
            Bind(documentRoot.Q<Button>("pause-main-menu-button"), mainMenu, clickFeedback);
            Bind(documentRoot.Q<Button>("pause-quit-button"), quit, clickFeedback);
        }

        public void SetVisible(bool visible)
        {
            root.EnableInClassList("is-hidden", !visible);
            if (visible)
            {
                root.schedule.Execute(() => root.Q<Button>("resume-button").Focus());
            }
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
