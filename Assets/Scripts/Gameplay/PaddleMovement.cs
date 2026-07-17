using UnityEngine;

namespace Pong
{
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public sealed class PaddleMovement : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 8f;

        [Tooltip("Distance from the centre line to a wall's inner face. Travel is derived from " +
            "this and the paddle's own height, so a paddle can never leave the arena.")]
        [SerializeField, Min(0.1f)] private float arenaHalfHeight = 4.44f;

        [Tooltip("Local Y scale at full length. The sprite is 2.56 world units per unit of scale.")]
        [SerializeField, Min(0.01f)] private float fullLength = 0.55f;

        private Rigidbody2D body;
        private BoxCollider2D box;
        private float direction;
        private Vector2 activeLimits;

        public Vector2 Position => body.position;

        // resolved lazily because a vacant seat is configured while its object is inactive
        private BoxCollider2D Box => box != null ? box : box = GetComponent<BoxCollider2D>();

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            SetLengthScale(1f);
        }

        private void FixedUpdate()
        {
            float nextY = Mathf.Clamp(
                body.position.y + direction * speed * Time.fixedDeltaTime,
                activeLimits.x,
                activeLimits.y
            );

            body.MovePosition(new Vector2(body.position.x, nextY));
        }

        /// Sets the paddle's length and re-derives how far it may travel. The limit is measured from
        /// the collider rather than assumed from local scale: the sprite is not one unit tall, and
        /// assuming it was let paddles travel through the walls and off the screen.
        public void SetLengthScale(float scale)
        {
            float length = fullLength * Mathf.Clamp(scale, 0.3f, 1f);
            Vector3 localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x, length, localScale.z);

            float halfHeight = Box.size.y * length * 0.5f;
            float travel = Mathf.Max(0f, arenaHalfHeight - halfHeight);
            activeLimits = new Vector2(-travel, travel);
        }

        public void SetDirection(float value)
        {
            direction = Mathf.Clamp(value, -1f, 1f);
        }
    }
}
