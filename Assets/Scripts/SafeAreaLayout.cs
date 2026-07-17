using UnityEngine;
using UnityEngine.UIElements;

namespace Pong
{
    /// Keeps readable and interactive content clear of notches, rounded corners and gesture areas.
    ///
    /// Only elements asking for it are inset, and they are inset with padding rather than moved, so
    /// a background, a scrim or an overlay still reaches every edge. A screen should look like it
    /// runs under the cutout; its buttons should not sit beneath one.
    public sealed class SafeAreaLayout
    {
        private readonly UQueryState<VisualElement> inset;

        private Rect lastSafeArea;
        private Vector2 lastScreen;
        private Vector2 lastPanel;

        public SafeAreaLayout(VisualElement root)
        {
            inset = root.Query(className: "safe-area").Build();
            Root = root;
        }

        private VisualElement Root { get; }

        /// Cheap to call every frame: the work only happens when the cutout or the panel changes,
        /// which is a rotation or a resize rather than a frame.
        public void Refresh()
        {
            Rect safeArea = Screen.safeArea;
            Vector2 screen = new Vector2(Screen.width, Screen.height);
            Vector2 panel = new Vector2(Root.resolvedStyle.width, Root.resolvedStyle.height);

            if (safeArea == lastSafeArea && screen == lastScreen && panel == lastPanel)
            {
                return;
            }

            lastSafeArea = safeArea;
            lastScreen = screen;
            lastPanel = panel;

            SafeAreaInsets insets = SafeArea.Insets(safeArea, screen, panel);
            inset.ForEach(element =>
            {
                element.style.paddingLeft = insets.Left;
                element.style.paddingRight = insets.Right;
                element.style.paddingTop = insets.Top;
                element.style.paddingBottom = insets.Bottom;
            });
        }
    }
}
