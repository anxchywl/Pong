using System;
using System.Collections.Generic;

namespace Pong
{
    public enum SeatRole
    {
        Goalkeeper,
        Attacker
    }

    public enum SeatOccupant
    {
        Empty,
        Human,
        Computer
    }

    public readonly struct CourtSeat : IEquatable<CourtSeat>
    {
        private static readonly CourtSeat[] all =
        {
            new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper),
            new CourtSeat(PlayerSide.Left, SeatRole.Attacker),
            new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper),
            new CourtSeat(PlayerSide.Right, SeatRole.Attacker)
        };

        public CourtSeat(PlayerSide side, SeatRole role)
        {
            Side = side;
            Role = role;
        }

        public static IReadOnlyList<CourtSeat> All => all;

        public PlayerSide Side { get; }
        public SeatRole Role { get; }

        public bool Equals(CourtSeat other)
        {
            return Side == other.Side && Role == other.Role;
        }

        public override bool Equals(object obj)
        {
            return obj is CourtSeat other && Equals(other);
        }

        public override int GetHashCode()
        {
            return ((int)Side << 1) | (int)Role;
        }

        public override string ToString()
        {
            return $"{Side} {Role}";
        }
    }
}
