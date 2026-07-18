using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Pong
{
    /// Drags one paddle with a finger.
    ///
    /// The paddle is not placed where the finger is: it is asked to head there, at exactly the speed
    /// it has always moved. So a drag produces the same intent a key or a stick does, gameplay reads
    /// one number either way, and no rule changes to make a phone work.
    ///
    /// There is no filtering between the finger and the intent. Smoothing would only add the lag it
    /// pretends to remove.
    [RequireComponent(typeof(PaddleMovement))]
    public sealed class TouchPaddleInput : MonoBehaviour
    {
        private const int NoFinger = -1;

        [SerializeField] private CourtProjection projection;

        [Tooltip("How far the finger must lead the paddle for it to ask for full speed. Below this " +
            "the intent eases off, so a paddle that has arrived sits still instead of buzzing " +
            "around the finger.")]
        [SerializeField, Min(0.01f)] private float fullSpeedLead = 0.4f;

        private PaddleMovement paddle;
        private CourtRegion region;
        private int finger = NoFinger;

        private void Awake()
        {
            paddle = GetComponent<PaddleMovement>();
        }

        /// A seat answers to its own band of the court. Bands never overlap, so four fingers can
        /// drive four paddles without anyone deciding who owns which.
        public void Bind(CourtRegion value)
        {
            region = value;
            finger = NoFinger;
        }

        private void OnDisable()
        {
            finger = NoFinger;
            paddle.SetDirection(0f);
        }

        private void Update()
        {
            Touchscreen screen = Touchscreen.current;
            if (screen == null)
            {
                paddle.SetDirection(0f);
                return;
            }

            if (!TryReadFinger(screen, out Vector2 screenPoint))
            {
                paddle.SetDirection(0f);
                return;
            }

            float target = projection.ToCourt(screenPoint).y;
            float lead = target - paddle.Position.y;
            paddle.SetDirection(Mathf.Clamp(lead / fullSpeedLead, -1f, 1f));
        }

        /// Claims a finger that begins inside this seat's band and keeps it until it lifts. It is
        /// kept even if it wanders out: a player who has grabbed their paddle should not lose it by
        /// dragging past the halfway line.
        private bool TryReadFinger(Touchscreen screen, out Vector2 screenPoint)
        {
            screenPoint = Vector2.zero;

            foreach (TouchControl touch in screen.touches)
            {
                if (!touch.press.isPressed)
                {
                    continue;
                }

                int id = touch.touchId.ReadValue();
                if (finger == id)
                {
                    screenPoint = touch.position.ReadValue();
                    return true;
                }

                if (finger == NoFinger && region.Contains(projection.ToCourt(touch.position.ReadValue())))
                {
                    finger = id;
                    screenPoint = touch.position.ReadValue();
                    return true;
                }
            }

            finger = NoFinger;
            return false;
        }
    }
}
