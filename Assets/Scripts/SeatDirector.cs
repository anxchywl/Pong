using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    /// Owns the lineup and pushes it onto the court's paddles. Screens read and mutate the roster
    /// here rather than reaching for paddles directly.
    public sealed class SeatDirector : MonoBehaviour
    {
        [SerializeField] private PaddleSeat[] seats;
        [SerializeField] private InputProfileCatalog profiles;
        [SerializeField] private InputActionAsset controls;

        [Tooltip("Paddle length when a side is defended by two players, relative to a lone paddle. " +
            "Balances the extra interception depth a pair gains. Needs playtesting.")]
        [SerializeField, Range(0.3f, 1f)] private float pairedPaddleLength = 0.7f;

        [Tooltip("Distance from the centre to the outside of a goal. A lone paddle answers to its " +
            "whole half of the court, out to here.")]
        [SerializeField, Min(0.1f)] private float courtHalfWidth = 9.6f;

        public MatchRoster Roster { get; } = new MatchRoster();

        public InputProfileCatalog Profiles => profiles;

        private void OnEnable()
        {
            Roster.Changed += Apply;
            Apply();
        }

        private void OnDisable()
        {
            Roster.Changed -= Apply;
        }

        /// Rebinds the court without the lineup having changed. A pad that left or came back is the
        /// same roster driven by different hardware.
        public void Refresh()
        {
            Apply();
        }

        private void Apply()
        {
            foreach (PaddleSeat seat in seats)
            {
                SeatAssignment assignment = Roster.Get(seat.Seat);
                bool shared = Roster.OccupiedCount(seat.Seat.Side) > 1;
                float lengthScale = shared ? pairedPaddleLength : 1f;
                seat.Configure(
                    assignment,
                    controls,
                    profiles.Find(assignment.ProfileId),
                    lengthScale,
                    RegionFor(seat, shared)
                );
            }
        }

        /// The band of court a seat answers to a finger in.
        ///
        /// Alone, a paddle owns its whole half: anywhere on your side drags your paddle. Sharing a
        /// side, the half splits between the two paddles at the point midway between them, so each
        /// player drags near their own and the bands never overlap. That is what lets four fingers
        /// drive four paddles with nobody arbitrating.
        private CourtRegion RegionFor(PaddleSeat seat, bool shared)
        {
            float edge = seat.Side == PlayerSide.Left ? -courtHalfWidth : courtHalfWidth;
            if (!shared)
            {
                return new CourtRegion(edge, 0f);
            }

            float split = SplitFor(seat.Side);
            return seat.Seat.Role == SeatRole.Goalkeeper
                ? new CourtRegion(edge, split)
                : new CourtRegion(split, 0f);
        }

        /// Midway between the two paddles on a side, read from where they actually stand rather
        /// than assumed, so moving a column moves the band with it.
        private float SplitFor(PlayerSide side)
        {
            float keeper = 0f;
            float attacker = 0f;
            foreach (PaddleSeat seat in seats)
            {
                if (seat.Side != side)
                {
                    continue;
                }

                if (seat.Seat.Role == SeatRole.Goalkeeper)
                {
                    keeper = seat.transform.position.x;
                }
                else
                {
                    attacker = seat.transform.position.x;
                }
            }

            return (keeper + attacker) * 0.5f;
        }
    }
}
