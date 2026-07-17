using UnityEngine.UIElements;

namespace Pong
{
    /// Puts the room a layout has onto the root as classes, so USS decides what to do with it.
    ///
    /// The measurement is the panel's own resolved size, which is the space the UI actually has.
    /// Nothing here asks what device it is running on, and nothing downstream should either: a
    /// desktop window dragged narrow gets the same layout a phone does, because it has the same room.
    public sealed class ResponsiveLayout
    {
        private readonly VisualElement root;

        private LayoutSize lastSize = (LayoutSize)(-1);
        private bool lastTall;
        private bool applied;

        public ResponsiveLayout(VisualElement root)
        {
            this.root = root;
        }

        public LayoutSize Size { get; private set; }

        /// Cheap to call every frame: classes only change when the panel does.
        public void Refresh()
        {
            float width = root.resolvedStyle.width;
            float height = root.resolvedStyle.height;
            if (width <= 0f || height <= 0f)
            {
                return;
            }

            LayoutSize size = LayoutBreakpoints.For(width);
            bool tall = LayoutBreakpoints.IsTall(new UnityEngine.Vector2(width, height));
            if (applied && size == lastSize && tall == lastTall)
            {
                return;
            }

            applied = true;
            lastSize = size;
            lastTall = tall;
            Size = size;

            foreach (LayoutSize candidate in new[] { LayoutSize.Compact, LayoutSize.Medium, LayoutSize.Expanded })
            {
                root.EnableInClassList(LayoutBreakpoints.ClassFor(candidate), candidate == size);
            }

            root.EnableInClassList("layout--tall", tall);
            root.EnableInClassList("layout--wide", !tall);
        }
    }
}
