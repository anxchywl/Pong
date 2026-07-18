using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private TouchPaddleInput touchInput;
        [SerializeField] private SpriteRenderer paddleRenderer;
        [SerializeField] private SpriteRenderer glowRenderer;

        private PaddleMovement movement;

        public CourtSeat Seat => new CourtSeat(side, role);
        public PlayerSide Side => side;
        public SpriteRenderer Renderer => paddleRenderer;
        public SpriteRenderer Glow => glowRenderer;

        // resolved lazily because a vacant seat is configured while its object is inactive
        private PaddleMovement Movement => movement != null ? movement : movement = GetComponent<PaddleMovement>();

        public void Configure(
            SeatAssignment assignment,
            InputActionAsset controls,
            InputProfileDefinition profile,
            float lengthScale,
            CourtRegion region
        )
        {
            gameObject.SetActive(assignment.IsOccupied);
            if (!assignment.IsOccupied)
            {
                return;
            }

            Movement.SetLengthScale(lengthScale);
            bool human = assignment.Occupant == SeatOccupant.Human;
            bool dragged = human && profile != null && profile.Kind == InputProfileKind.Touch;

            humanInput.enabled = human && !dragged;
            touchInput.enabled = dragged;
            computerInput.enabled = !human;

            if (dragged)
            {
                touchInput.Bind(region);
            }
            else if (human)
            {
                humanInput.Bind(controls, profile, assignment.DeviceId);
            }
        }
    }
}
