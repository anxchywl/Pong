using UnityEngine;

namespace Pong
{
    [RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
    public sealed class PaddleMovement : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float speed = 8f;
        [SerializeField] private Vector2 verticalLimits = new Vector2(-3.8f, 3.8f);

        private Rigidbody2D body;
        private float direction;

        public Vector2 Position => body.position;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
        }

        private void FixedUpdate()
        {
            float nextY = Mathf.Clamp(
                body.position.y + direction * speed * Time.fixedDeltaTime,
                verticalLimits.x,
                verticalLimits.y
            );

            body.MovePosition(new Vector2(body.position.x, nextY));
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
