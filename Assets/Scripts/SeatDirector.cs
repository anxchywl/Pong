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
                float lengthScale = Roster.OccupiedCount(seat.Seat.Side) > 1 ? pairedPaddleLength : 1f;
                seat.Configure(assignment, controls, profiles.Find(assignment.ProfileId), lengthScale);
            }
        }
    }
}
