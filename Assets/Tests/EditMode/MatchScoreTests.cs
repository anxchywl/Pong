using System;
using NUnit.Framework;

namespace Pong.Tests
{
    public sealed class MatchScoreTests
    {
        [Test]
        public void AddPoint_TracksEachSideIndependently()
        {
            MatchScore score = new MatchScore(3);

            score.AddPoint(PlayerSide.Left);
            score.AddPoint(PlayerSide.Right);
            score.AddPoint(PlayerSide.Right);

            Assert.That(score.Left, Is.EqualTo(1));
            Assert.That(score.Right, Is.EqualTo(2));
            Assert.That(score.Winner, Is.Null);
        }

        [Test]
        public void AddPoint_DeclaresWinnerAtConfiguredScore()
        {
            MatchScore score = new MatchScore(2);

            score.AddPoint(PlayerSide.Left);
            score.AddPoint(PlayerSide.Left);

            Assert.That(score.Winner, Is.EqualTo(PlayerSide.Left));
        }

        [Test]
        public void AddPoint_IgnoresPointsAfterMatchEnds()
        {
            MatchScore score = new MatchScore(1);

            score.AddPoint(PlayerSide.Right);
            score.AddPoint(PlayerSide.Left);

            Assert.That(score.Left, Is.Zero);
            Assert.That(score.Right, Is.EqualTo(1));
        }

        [Test]
        public void Reset_ClearsScoresAndWinner()
        {
            MatchScore score = new MatchScore(1);
            score.AddPoint(PlayerSide.Left);

            score.Reset();

            Assert.That(score.Left, Is.Zero);
            Assert.That(score.Right, Is.Zero);
            Assert.That(score.Winner, Is.Null);
        }

        [Test]
        public void Constructor_RejectsNonPositiveWinningScore()
        {
            Assert.That(() => new MatchScore(0), Throws.TypeOf<ArgumentOutOfRangeException>());
        }

        [Test]
        public void AddPoint_RejectsUnknownSide()
        {
            MatchScore score = new MatchScore(3);

            Assert.That(
                () => score.AddPoint((PlayerSide)99),
                Throws.TypeOf<ArgumentOutOfRangeException>()
            );
        }
    }
}
