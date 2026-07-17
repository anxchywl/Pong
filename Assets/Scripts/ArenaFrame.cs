using UnityEngine;

namespace Pong
{
    /// How the court is framed at a given aspect. Pure maths, so every shape a screen can be is
    /// cheap to check.
    public static class ArenaFrame
    {
        /// True when the screen is taller than it is wide, and the court should lie along it.
        public static bool IsPortrait(float aspect)
        {
            return aspect < 1f;
        }

        /// Turns the camera a quarter so the court's long axis runs down the screen's long axis.
        /// The world does not move: this is the same match seen sideways.
        public static float RollDegrees(float aspect)
        {
            return IsPortrait(aspect) ? 90f : 0f;
        }

        /// The orthographic size that keeps the whole court on screen.
        ///
        /// Size is half the screen's height in world units, and width follows from the aspect. In
        /// portrait the turned camera maps the screen's height onto the court's width, so the two
        /// extents swap. The landscape minimum is what preserves the framing the game already has:
        /// a 16:9 window needs far less than it, and the band above the wall is where the HUD lives.
        public static float OrthographicSize(
            float aspect,
            float halfWidth,
            float halfHeight,
            float landscapeMinimum
        )
        {
            bool portrait = IsPortrait(aspect);
            float alongScreenHeight = portrait ? halfWidth : halfHeight;
            float alongScreenWidth = portrait ? halfHeight : halfWidth;
            float size = Mathf.Max(alongScreenHeight, alongScreenWidth / aspect);
            return portrait ? size : Mathf.Max(size, landscapeMinimum);
        }
    }
}
