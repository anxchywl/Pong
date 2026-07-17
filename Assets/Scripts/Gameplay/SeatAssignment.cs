using System;

namespace Pong
{
    /// A seat's occupant plus, for a human, the input profile and physical device driving it.
    public readonly struct SeatAssignment : IEquatable<SeatAssignment>
    {
        public const int NoDevice = 0;

        private SeatAssignment(SeatOccupant occupant, string profileId, int deviceId)
        {
            Occupant = occupant;
            ProfileId = profileId;
            DeviceId = deviceId;
        }

        public static SeatAssignment Empty => new SeatAssignment(SeatOccupant.Empty, string.Empty, NoDevice);

        public static SeatAssignment Computer => new SeatAssignment(SeatOccupant.Computer, string.Empty, NoDevice);

        public static SeatAssignment Human(string profileId, int deviceId)
        {
            if (string.IsNullOrEmpty(profileId))
            {
                throw new ArgumentException("A human seat needs an input profile", nameof(profileId));
            }

            return new SeatAssignment(SeatOccupant.Human, profileId, deviceId);
        }

        public SeatOccupant Occupant { get; }
        public string ProfileId { get; }
        public int DeviceId { get; }

        public bool IsOccupied => Occupant != SeatOccupant.Empty;

        public bool Equals(SeatAssignment other)
        {
            return Occupant == other.Occupant && ProfileId == other.ProfileId && DeviceId == other.DeviceId;
        }

        public override bool Equals(object obj)
        {
            return obj is SeatAssignment other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine((int)Occupant, ProfileId, DeviceId);
        }
    }
}
