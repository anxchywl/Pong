using NUnit.Framework;
using UnityEngine;

namespace Pong.Tests
{
    /// The widths real panels come in, in physical points. These pin the thresholds to the gaps
    /// between devices rather than to any one of them.
    public sealed class LayoutBreakpointTests
    {
        private const float PhonePortrait = 250f;
        private const float PhoneLandscape = 530f;
        private const float TabletPortrait = 600f;
        private const float TabletLandscape = 820f;
        private const float DesktopWindow = 1920f;

        [TestCase(PhonePortrait)]
        [TestCase(PhoneLandscape)]
        public void APhoneIsCompactEitherWayUp(float width)
        {
            Assert.That(LayoutBreakpoints.For(width), Is.EqualTo(LayoutSize.Compact));
        }

        [TestCase(TabletPortrait)]
        [TestCase(TabletLandscape)]
        public void ATabletIsMediumEitherWayUp(float width)
        {
            Assert.That(LayoutBreakpoints.For(width), Is.EqualTo(LayoutSize.Medium));
        }

        [Test]
        public void ADesktopWindowIsExpanded()
        {
            Assert.That(LayoutBreakpoints.For(DesktopWindow), Is.EqualTo(LayoutSize.Expanded));
        }

        /// The point of measuring space: a window dragged narrow has a phone's problem and should
        /// get a phone's answer, on a desktop.
        [Test]
        public void ANarrowDesktopWindowIsCompactBecauseSpaceIsWhatCounts()
        {
            Assert.That(LayoutBreakpoints.For(400f), Is.EqualTo(LayoutSize.Compact));
        }

        [Test]
        public void TheThresholdsThemselvesFallOnTheWiderSide()
        {
            Assert.That(LayoutBreakpoints.For(LayoutBreakpoints.MediumFrom), Is.EqualTo(LayoutSize.Medium));
            Assert.That(LayoutBreakpoints.For(LayoutBreakpoints.MediumFrom - 0.01f), Is.EqualTo(LayoutSize.Compact));
            Assert.That(LayoutBreakpoints.For(LayoutBreakpoints.ExpandedFrom), Is.EqualTo(LayoutSize.Expanded));
            Assert.That(LayoutBreakpoints.For(LayoutBreakpoints.ExpandedFrom - 0.01f), Is.EqualTo(LayoutSize.Medium));
        }

        [Test]
        public void EverySizeAnswersToItsOwnClass()
        {
            Assert.That(LayoutBreakpoints.ClassFor(LayoutSize.Compact), Is.EqualTo("layout--compact"));
            Assert.That(LayoutBreakpoints.ClassFor(LayoutSize.Medium), Is.EqualTo("layout--medium"));
            Assert.That(LayoutBreakpoints.ClassFor(LayoutSize.Expanded), Is.EqualTo("layout--expanded"));
        }

        [Test]
        public void APanelWithMoreHeightThanWidthIsTall()
        {
            Assert.That(LayoutBreakpoints.IsTall(new Vector2(250f, 530f)), Is.True, "a phone upright");
            Assert.That(LayoutBreakpoints.IsTall(new Vector2(600f, 820f)), Is.True, "a tablet upright");
            Assert.That(LayoutBreakpoints.IsTall(new Vector2(1920f, 1080f)), Is.False, "a desktop window");
            Assert.That(LayoutBreakpoints.IsTall(new Vector2(500f, 500f)), Is.False, "a square is not tall");
        }
    }
}
