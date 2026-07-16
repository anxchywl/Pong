using UnityEngine;

namespace Pong
{
    /// One physical paddle position on the court. Knows which seat it represents and switches
    /// between human and computer intent without either knowing the other exists.
    [RequireComponent(typeof(PaddleMovement))]
    public sealed class PaddleSeat : MonoBehaviour
    {
        [SerializeField] private PlayerSide side;
        [SerializeField] private SeatRole role;
        [SerializeField] private PlayerPaddleInput humanInput;
        [SerializeField] private ComputerPaddleController computerInput;

        private PaddleMovement movement;

        public CourtSeat Seat => new CourtSeat(side, role);

        // resolved lazily because a vacant seat is configured while its object is inactive
        private PaddleMovement Movement => movement != null ? movement : movement = GetComponent<PaddleMovement>();

        public void Configure(SeatAssignment assignment, InputProfileDefinition profile, float lengthScale)
        {
            gameObject.SetActive(assignment.IsOccupied);
            if (!assignment.IsOccupied)
            {
                return;
            }

            Movement.SetLengthScale(lengthScale);
            bool human = assignment.Occupant == SeatOccupant.Human;
            humanInput.enabled = human;
            computerInput.enabled = !human;

            if (human)
            {
                humanInput.Bind(profile, assignment.DeviceId);
            }
        }
    }
}
