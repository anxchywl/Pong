using UnityEngine;

namespace Pong
{
    [RequireComponent(typeof(PaddleMovement))]
    public sealed class ComputerPaddleController : MonoBehaviour
    {
        [SerializeField] private BallController ball;
        [SerializeField, Min(0f)] private float deadZone = 0.15f;

        private PaddleMovement paddle;

        private void Awake()
        {
            paddle = GetComponent<PaddleMovement>();
        }

        private void Update()
        {
            float targetY = ball.Velocity.x > 0f ? ball.Position.y : 0f;
            float distance = targetY - paddle.Position.y;
            paddle.SetDirection(Mathf.Abs(distance) <= deadZone ? 0f : Mathf.Sign(distance));
        }
    }
}
