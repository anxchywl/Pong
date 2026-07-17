using NUnit.Framework;

namespace Pong.Tests
{
    /// The court has to survive every shape a screen comes in. The framing is pure maths, so each
    /// one is a case rather than a device.
    public sealed class ArenaFrameTests
    {
        private const float HalfWidth = 9.6f;
        private const float HalfHeight = 4.7f;
        private const float LandscapeMinimum = 6.9f;

        private const float Ultrawide = 21f / 9f;
        private const float Widescreen = 16f / 9f;
        private const float SixteenTen = 16f / 10f;
        private const float FourThree = 4f / 3f;
        private const float Square = 1f;
        private const float TabletPortrait = 3f / 4f;
        private const float PhonePortrait = 9f / 16f;

        [TestCase(Widescreen)]
        [TestCase(SixteenTen)]
        [TestCase(FourThree)]
        [TestCase(Ultrawide)]
        [TestCase(Square)]
        public void Landscape_KeepsTheCourtUpright(float aspect)
        {
            Assert.That(ArenaFrame.IsPortrait(aspect), Is.False);
            Assert.That(ArenaFrame.RollDegrees(aspect), Is.Zero);
        }

        [TestCase(PhonePortrait)]
        [TestCase(TabletPortrait)]
        public void Portrait_LaysTheCourtAlongTheScreen(float aspect)
        {
            Assert.That(ArenaFrame.IsPortrait(aspect), Is.True);
            Assert.That(ArenaFrame.RollDegrees(aspect), Is.EqualTo(90f));
        }

        /// The framing the game already had must not shift under it.
        [TestCase(Widescreen)]
        [TestCase(SixteenTen)]
        [TestCase(Ultrawide)]
        public void AWideWindowKeepsTheFramingItAlwaysHad(float aspect)
        {
            Assert.That(Size(aspect), Is.EqualTo(LandscapeMinimum).Within(0.001f));
        }

        /// 4:3 cleared the goals by a tenth of a unit before. Now it pulls back instead.
        [Test]
        public void ANarrowLandscapeWindowPullsBackRatherThanCropTheGoals()
        {
            float size = Size(FourThree);

            Assert.That(size, Is.GreaterThan(LandscapeMinimum), "4:3 kept a framing that barely fit");
            Assert.That(size * FourThree, Is.GreaterThanOrEqualTo(HalfWidth));
        }

        [TestCase(Ultrawide)]
        [TestCase(Widescreen)]
        [TestCase(SixteenTen)]
        [TestCase(FourThree)]
        [TestCase(Square)]
        [TestCase(TabletPortrait)]
        [TestCase(PhonePortrait)]
        public void EveryShapeOfScreenShowsTheWholeCourt(float aspect)
        {
            float size = Size(aspect);
            bool portrait = ArenaFrame.IsPortrait(aspect);

            // a turned camera maps the screen's height onto the court's width
            float shownAcrossCourtWidth = portrait ? size : size * aspect;
            float shownAcrossCourtHeight = portrait ? size * aspect : size;

            Assert.That(shownAcrossCourtWidth, Is.GreaterThanOrEqualTo(HalfWidth),
                $"a goal falls off screen at aspect {aspect}");
            Assert.That(shownAcrossCourtHeight, Is.GreaterThanOrEqualTo(HalfHeight),
                $"a wall falls off screen at aspect {aspect}");
        }

        /// Portrait is framed for the court, not zoomed out until a landscape layout happens to fit.
        [Test]
        public void PhonePortraitFramesTheCourtRatherThanShrinkIt()
        {
            float size = Size(PhonePortrait);

            Assert.That(size, Is.EqualTo(HalfWidth).Within(0.001f),
                "portrait framed wider than the court needs");
        }

        private static float Size(float aspect)
        {
            return ArenaFrame.OrthographicSize(aspect, HalfWidth, HalfHeight, LandscapeMinimum);
        }
    }
}
