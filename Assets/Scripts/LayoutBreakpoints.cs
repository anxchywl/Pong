using UnityEngine;

namespace Pong
{
    /// How much room a layout has, never which device it is on. A narrow desktop window is narrow;
    /// a tablet held upright is not a phone.
    public enum LayoutSize
    {
        Compact,
        Medium,
        Expanded
    }

    /// Where the layout changes shape.
    ///
    /// The panel is measured in physical points — roughly a hundredth of an inch each — so these are
    /// real widths rather than pixel counts. A phone is about 250 upright and 530 on its side; a
    /// tablet about 600 upright and 820 on its side; a desktop window 1200 and up. The thresholds
    /// sit in the gaps, not on top of any of them.
    public static class LayoutBreakpoints
    {
        public const float MediumFrom = 560f;
        public const float ExpandedFrom = 1000f;

        public static LayoutSize For(float width)
        {
            if (width >= ExpandedFrom)
            {
                return LayoutSize.Expanded;
            }

            return width >= MediumFrom ? LayoutSize.Medium : LayoutSize.Compact;
        }

        /// True when a layout has more height than width to spend. Stacking suits it; a row does not.
        public static bool IsTall(Vector2 panel)
        {
            return panel.y > panel.x;
        }

        /// The class a size answers to in USS.
        public static string ClassFor(LayoutSize size)
        {
            return size switch
            {
                LayoutSize.Compact => "layout--compact",
                LayoutSize.Medium => "layout--medium",
                _ => "layout--expanded"
            };
        }
    }
}
