using UnityEngine;

namespace Pong
{
    /// Keeps the whole court on screen at any shape of window, and lays it along the screen in
    /// portrait. Framing only: no world geometry moves and nothing plays differently.
    [RequireComponent(typeof(Camera))]
    public sealed class ArenaFraming : MonoBehaviour
    {
        [Tooltip("Distance from the centre to the outside of a goal. The court must never be " +
            "framed tighter than this, or a goal would sit off screen.")]
        [SerializeField, Min(0.1f)] private float halfWidth = 9.6f;

        [Tooltip("Distance from the centre line to a wall's inner face.")]
        [SerializeField, Min(0.1f)] private float halfHeight = 4.7f;

        [Tooltip("The framing a landscape window keeps unless the court needs more room. This is " +
            "the size the game has always used, and the band it leaves above the wall is where " +
            "the HUD lives.")]
        [SerializeField, Min(0.1f)] private float landscapeMinimum = 6.9f;

        private Camera gameplayCamera;
        private int lastWidth;
        private int lastHeight;

        private void Awake()
        {
            gameplayCamera = GetComponent<Camera>();
        }

        private void OnEnable()
        {
            Reframe();
        }

        private void Update()
        {
            // rotating a phone is the common case and there is no event for it, but reframing costs
            // a trigonometry-free divide, so only a resize pays for it
            if (Screen.width == lastWidth && Screen.height == lastHeight)
            {
                return;
            }

            Reframe();
        }

        private void Reframe()
        {
            lastWidth = Screen.width;
            lastHeight = Screen.height;
            if (lastHeight <= 0)
            {
                return;
            }

            float aspect = (float)lastWidth / lastHeight;
            gameplayCamera.orthographicSize =
                ArenaFrame.OrthographicSize(aspect, halfWidth, halfHeight, landscapeMinimum);
            transform.rotation = Quaternion.Euler(0f, 0f, ArenaFrame.RollDegrees(aspect));
        }
    }
}
