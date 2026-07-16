using UnityEngine;

namespace Pong
{
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public sealed class PaddleMovement : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 8f;
        [SerializeField] private Vector2 verticalLimits = new Vector2(-3.8f, 3.8f);
        [SerializeField, Min(0.1f)] private float fullLength = 2.1f;

        private Rigidbody2D body;
        private float direction;
        private Vector2 activeLimits;

        public Vector2 Position => body.position;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            activeLimits = verticalLimits;
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

        /// Shortens the paddle while keeping the reach of its outer edges, so a shorter paddle still
        /// covers the wall it defends and a full-length paddle behaves exactly as it always has.
        public void SetLengthScale(float scale)
        {
            float length = fullLength * Mathf.Clamp(scale, 0.3f, 1f);
            Vector3 localScale = transform.localScale;
            transform.localScale = new Vector3(localScale.x, length, localScale.z);

            float reclaimed = (fullLength - length) * 0.5f;
            activeLimits = new Vector2(verticalLimits.x - reclaimed, verticalLimits.y + reclaimed);
        }

        public void SetDirection(float value)
        {
            direction = Mathf.Clamp(value, -1f, 1f);
        }

        private void OnValidate()
        {
            if (verticalLimits.x > verticalLimits.y)
            {
                verticalLimits = new Vector2(verticalLimits.y, verticalLimits.x);
            }
        }
    }
}
