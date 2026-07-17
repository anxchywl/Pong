using NUnit.Framework;
using UnityEngine;

namespace Pong.Tests
{
    /// Cutouts are a device's business, but the maths that dodges them is not. These are the shapes
    /// real screens come in, checked without owning one.
    public sealed class SafeAreaTests
    {
        private static readonly Vector2 Panel = new Vector2(1920f, 1080f);

        [Test]
        public void ADisplayWithoutACutoutInsetsNothing()
        {
            Vector2 screen = new Vector2(1920f, 1080f);

            SafeAreaInsets insets = SafeArea.Insets(new Rect(0f, 0f, 1920f, 1080f), screen, Panel);

            Assert.That(insets.IsNone, Is.True);
        }

        /// A notch above and a home indicator below, as a phone held upright.
        [Test]
        public void APhonePortraitClearsTheNotchAndTheHomeIndicator()
        {
            Vector2 screen = new Vector2(1170f, 2532f);
            Vector2 panel = new Vector2(390f, 844f);
            // safe area runs from 34 above the bottom to 132 below the top
            Rect safe = new Rect(0f, 34f, 1170f, 2532f - 34f - 132f);

            SafeAreaInsets insets = SafeArea.Insets(safe, screen, panel);

            Assert.That(insets.Top, Is.EqualTo(44f).Within(0.01f), "the notch was not cleared");
            Assert.That(insets.Bottom, Is.EqualTo(11.33f).Within(0.01f), "the home indicator was not cleared");
            Assert.That(insets.Left, Is.Zero);
            Assert.That(insets.Right, Is.Zero);
        }

        /// Turned sideways the notch becomes a side inset, which is the case a portrait-only
        /// solution gets wrong.
        [Test]
        public void APhoneLandscapeClearsTheNotchOnTheSide()
        {
            Vector2 screen = new Vector2(2532f, 1170f);
            Vector2 panel = new Vector2(844f, 390f);
            Rect safe = new Rect(132f, 63f, 2532f - 132f - 132f, 1170f - 63f);

            SafeAreaInsets insets = SafeArea.Insets(safe, screen, panel);

            Assert.That(insets.Left, Is.EqualTo(44f).Within(0.01f));
            Assert.That(insets.Right, Is.EqualTo(44f).Within(0.01f));
            Assert.That(insets.Bottom, Is.EqualTo(21f).Within(0.01f));
            Assert.That(insets.Top, Is.Zero);
        }

        /// The panel is measured in points and the cutout in pixels, so a dense display must not
        /// inset twice as far as it should.
        [Test]
        public void AHighDensityDisplayInsetsInPanelPointsNotPixels()
        {
            Vector2 screen = new Vector2(3840f, 2160f);
            Rect safe = new Rect(80f, 0f, 3840f - 80f, 2160f);

            SafeAreaInsets insets = SafeArea.Insets(safe, screen, Panel);

            Assert.That(insets.Left, Is.EqualTo(40f).Within(0.01f), "the inset was measured in pixels");
        }

        [Test]
        public void AnUnreadyScreenInsetsNothingRatherThanDivideByZero()
        {
            Assert.That(SafeArea.Insets(new Rect(0f, 0f, 0f, 0f), Vector2.zero, Panel).IsNone, Is.True);
            Assert.That(SafeArea.Insets(new Rect(0f, 0f, 100f, 100f), new Vector2(100f, 100f), Vector2.zero).IsNone,
                Is.True);
        }

        /// A safe area reported larger than the screen would otherwise push content inwards from
        /// nowhere.
        [Test]
        public void ASafeAreaWiderThanItsScreenNeverInsetsBackwards()
        {
            Vector2 screen = new Vector2(1000f, 1000f);
            Rect safe = new Rect(-10f, -10f, 1030f, 1030f);

            SafeAreaInsets insets = SafeArea.Insets(safe, screen, new Vector2(1000f, 1000f));

            Assert.That(insets.Left, Is.Zero);
            Assert.That(insets.Right, Is.Zero);
            Assert.That(insets.Top, Is.Zero);
            Assert.That(insets.Bottom, Is.Zero);
        }
    }
}
