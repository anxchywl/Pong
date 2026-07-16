using NUnit.Framework;

namespace Pong.Tests
{
    public sealed class MatchRosterTests
    {
        private static readonly CourtSeat LeftKeeper = new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper);
        private static readonly CourtSeat LeftAttacker = new CourtSeat(PlayerSide.Left, SeatRole.Attacker);
        private static readonly CourtSeat RightKeeper = new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper);
        private static readonly CourtSeat RightAttacker = new CourtSeat(PlayerSide.Right, SeatRole.Attacker);

        [Test]
        public void Reset_SeatsOneHumanAgainstOneComputer()
        {
            MatchRoster roster = new MatchRoster();

            Assert.That(roster.Get(LeftKeeper).Occupant, Is.EqualTo(SeatOccupant.Human));
            Assert.That(roster.Get(RightKeeper).Occupant, Is.EqualTo(SeatOccupant.Computer));
            Assert.That(roster.Get(LeftAttacker).IsOccupied, Is.False);
            Assert.That(roster.Get(RightAttacker).IsOccupied, Is.False);
            Assert.That(roster.HumanCount, Is.EqualTo(1));
            Assert.That(roster.IsPlayable, Is.True);
        }

        [Test]
        public void IsPlayable_RequiresAGoalkeeperOnBothSides()
        {
            MatchRoster roster = new MatchRoster();

            roster.Clear(RightKeeper);

            Assert.That(roster.IsPlayable, Is.False);
        }

        [Test]
        public void Assign_PromotesAnAttackerWhenTheGoalkeeperLeaves()
        {
            MatchRoster roster = new MatchRoster();
            roster.Assign(LeftAttacker, SeatAssignment.Human("keyboard-arrows", SeatAssignment.NoDevice));

            roster.Clear(LeftKeeper);

            Assert.That(roster.Get(LeftKeeper).ProfileId, Is.EqualTo("keyboard-arrows"));
            Assert.That(roster.Get(LeftAttacker).IsOccupied, Is.False);
            Assert.That(roster.IsPlayable, Is.True);
        }

        [Test]
        public void Assign_MovesAProfileRatherThanLettingItDriveTwoPaddles()
        {
            MatchRoster roster = new MatchRoster();
            SeatAssignment player = SeatAssignment.Human("gamepad", 7);
            roster.Assign(LeftAttacker, player);

            roster.Assign(RightAttacker, player);

            Assert.That(roster.Get(LeftAttacker).IsOccupied, Is.False);
            Assert.That(roster.Get(RightAttacker).DeviceId, Is.EqualTo(7));
            Assert.That(roster.HumanCount, Is.EqualTo(2));
        }

        [Test]
        public void Assign_TreatsTheSameProfileOnDifferentDevicesAsDifferentPlayers()
        {
            MatchRoster roster = new MatchRoster();

            roster.Assign(LeftAttacker, SeatAssignment.Human("gamepad", 1));
            roster.Assign(RightAttacker, SeatAssignment.Human("gamepad", 2));

            Assert.That(roster.Get(LeftAttacker).IsOccupied, Is.True);
            Assert.That(roster.Get(RightAttacker).IsOccupied, Is.True);
            Assert.That(roster.HumanCount, Is.EqualTo(3));
        }

        [Test]
        public void OccupiedCount_ReportsPerSideOccupancy()
        {
            MatchRoster roster = new MatchRoster();

            roster.Assign(LeftAttacker, SeatAssignment.Human("keyboard-arrows", SeatAssignment.NoDevice));

            Assert.That(roster.OccupiedCount(PlayerSide.Left), Is.EqualTo(2));
            Assert.That(roster.OccupiedCount(PlayerSide.Right), Is.EqualTo(1));
        }

        [Test]
        public void Assign_RaisesChangedSoScreensCanRerender()
        {
            MatchRoster roster = new MatchRoster();
            int changes = 0;
            roster.Changed += () => changes++;

            roster.Assign(RightAttacker, SeatAssignment.Computer);

            Assert.That(changes, Is.EqualTo(1));
        }

        [Test]
        public void Human_RejectsAnAssignmentWithoutAnInputProfile()
        {
            Assert.That(
                () => SeatAssignment.Human(string.Empty, SeatAssignment.NoDevice),
                Throws.ArgumentException
            );
        }
    }
}
