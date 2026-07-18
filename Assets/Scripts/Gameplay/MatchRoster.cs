using System;
using System.Collections.Generic;

namespace Pong
{
    /// Engine-independent lineup for the four court seats. Owns the rules about who may sit where,
    /// so every screen and future game mode reads the same source of truth.
    public sealed class MatchRoster
    {
        private readonly Dictionary<CourtSeat, SeatAssignment> assignments =
            new Dictionary<CourtSeat, SeatAssignment>();

        public MatchRoster()
        {
            Reset();
        }

        public event Action Changed;

        public SeatAssignment Get(CourtSeat seat)
        {
            return assignments[seat];
        }

        /// Both sides need a goalkeeper before a match can be served.
        public bool IsPlayable =>
            Get(new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper)).IsOccupied &&
            Get(new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper)).IsOccupied;

        public int HumanCount => Count(assignment => assignment.Occupant == SeatOccupant.Human);

        public int OccupiedCount(PlayerSide side)
        {
            int count = 0;
            foreach (CourtSeat seat in CourtSeat.All)
            {
                if (seat.Side == side && Get(seat).IsOccupied)
                {
                    count++;
                }
            }

            return count;
        }

        /// The default 1v1 lineup: one human keeper against one computer keeper.
        public void Reset()
        {
            foreach (CourtSeat seat in CourtSeat.All)
            {
                assignments[seat] = SeatAssignment.Empty;
            }

            assignments[new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper)] =
                SeatAssignment.Human(InputProfileCatalog.DefaultProfileId, SeatAssignment.NoDevice);
            assignments[new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper)] = SeatAssignment.Computer;
            Changed?.Invoke();
        }

        public void Assign(CourtSeat seat, SeatAssignment assignment)
        {
            // a shared driver may sit in several seats at once, so it takes nobody's when it claims
            if (assignment.Occupant == SeatOccupant.Human && assignment.Exclusive)
            {
                ReleaseClaim(assignment.ProfileId, assignment.DeviceId, seat);
            }

            assignments[seat] = assignment;
            CollapseSide(seat.Side);
            Changed?.Invoke();
        }

        public void Clear(CourtSeat seat)
        {
            Assign(seat, SeatAssignment.Empty);
        }

        /// True when a seat is driven by this device. Keyboard seats carry no device, so they never
        /// answer to one leaving.
        public bool IsDeviceClaimed(int deviceId)
        {
            if (deviceId == SeatAssignment.NoDevice)
            {
                return false;
            }

            foreach (CourtSeat seat in CourtSeat.All)
            {
                SeatAssignment assignment = Get(seat);
                if (assignment.Occupant == SeatOccupant.Human && assignment.DeviceId == deviceId)
                {
                    return true;
                }
            }

            return false;
        }

        /// Finds the seat currently driven by a profile and device, if any. A shared driver holds
        /// no exclusive claim, so it is never found here.
        public bool TryFindClaim(string profileId, int deviceId, out CourtSeat claimed)
        {
            foreach (CourtSeat seat in CourtSeat.All)
            {
                SeatAssignment assignment = Get(seat);
                if (assignment.Occupant == SeatOccupant.Human &&
                    assignment.Exclusive &&
                    assignment.ProfileId == profileId &&
                    assignment.DeviceId == deviceId)
                {
                    claimed = seat;
                    return true;
                }
            }

            claimed = default;
            return false;
        }

        /// One input profile drives one paddle. A later claim wins and vacates the earlier seat.
        private void ReleaseClaim(string profileId, int deviceId, CourtSeat claiming)
        {
            if (!TryFindClaim(profileId, deviceId, out CourtSeat existing) || existing.Equals(claiming))
            {
                return;
            }

            assignments[existing] = SeatAssignment.Empty;
            CollapseSide(existing.Side);
        }

        /// An attacker without a goalkeeper leaves the goal undefended, so occupancy always
        /// settles into the goalkeeper seat first.
        private void CollapseSide(PlayerSide side)
        {
            CourtSeat keeper = new CourtSeat(side, SeatRole.Goalkeeper);
            CourtSeat attacker = new CourtSeat(side, SeatRole.Attacker);

            if (!assignments[keeper].IsOccupied && assignments[attacker].IsOccupied)
            {
                assignments[keeper] = assignments[attacker];
                assignments[attacker] = SeatAssignment.Empty;
            }
        }

        private int Count(Func<SeatAssignment, bool> predicate)
        {
            int count = 0;
            foreach (CourtSeat seat in CourtSeat.All)
            {
                if (predicate(Get(seat)))
                {
                    count++;
                }
            }

            return count;
        }
    }
}
