using NUnit.Framework;

namespace Pong.Tests
{
    public sealed class MatchRosterTests
    {
        private static readonly CourtSeat LeftKeeper = new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper);
        private static readonly CourtSeat LeftAttacker = new CourtSeat(PlayerSide.Left, SeatRole.Attacker);
        private static readonly CourtSeat RightKeeper = new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper);
        private static readonly CourtSeat RightAttacker = new CourtSeat(PlayerSide.Right, SeatRole.Attacker);

        /// One touchscreen, several seats. They are told apart by where a finger lands, so the
        /// roster must not treat the second player as the first one moving.
        [Test]
        public void Assign_LetsASharedDriverSitInSeveralSeats()
        {
            MatchRoster roster = new MatchRoster();
            SeatAssignment touch = SeatAssignment.Human("touch", SeatAssignment.NoDevice, exclusive: false);

            roster.Assign(new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper), touch);
            roster.Assign(new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper), touch);

            Assert.That(roster.Get(new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper)).Occupant,
                Is.EqualTo(SeatOccupant.Human), "the second touch player evicted the first");
            Assert.That(roster.HumanCount, Is.EqualTo(2));
        }

        [Test]
        public void Assign_StillMovesAnExclusiveDriverRatherThanCloneIt()
        {
            MatchRoster roster = new MatchRoster();
            SeatAssignment pad = SeatAssignment.Human("gamepad", 7);

            roster.Assign(new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper), pad);
            roster.Assign(new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper), pad);

            Assert.That(roster.Get(new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper)).IsOccupied,
                Is.False, "one pad ended up driving two paddles");
        }

        [Test]
        public void IsDeviceClaimed_FindsTheSeatDrivenByAPad()
        {
            MatchRoster roster = new MatchRoster();
            roster.Assign(
                new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper),
                SeatAssignment.Human("gamepad", 42)
            );

            Assert.That(roster.IsDeviceClaimed(42), Is.True);
            Assert.That(roster.IsDeviceClaimed(7), Is.False);
        }

        [Test]
        public void IsDeviceClaimed_IgnoresKeyboardSeats()
        {
            MatchRoster roster = new MatchRoster();

            // the default lineup seats a keyboard player, whose device id is NoDevice
            Assert.That(roster.IsDeviceClaimed(SeatAssignment.NoDevice), Is.False);
        }

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
