using System;
using UnityEngine;

namespace Pong
{
    [RequireComponent(typeof(CircleCollider2D), typeof(Rigidbody2D))]
    public sealed class BallController : MonoBehaviour
    {
        [Header("Serve")]
        [SerializeField, Min(0f)] private float serveDelay = 0.8f;
        [SerializeField, Min(0.1f)] private float startingSpeed = 7f;
        [SerializeField, Range(0f, 80f)] private float serveAngle = 20f;

        [Header("Rally")]
        [SerializeField, Min(0f)] private float speedIncreasePerHit = 0.35f;
        [SerializeField, Min(0.1f)] private float maximumSpeed = 11f;
        [SerializeField, Range(10f, 80f)] private float maximumBounceAngle = 65f;

        private Rigidbody2D body;
        private Vector2 startingPosition;
        private int serveDirection = 1;
        private int verticalServeDirection = 1;
        private float currentSpeed;

        public event Action Served;
        public event Action<Vector2> Bounced;

        public Vector2 Position => body.position;
        public Vector2 Velocity => body.linearVelocity;

        private void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            startingPosition = body.position;
            currentSpeed = startingSpeed;
        }

        private void OnDisable()
        {
            CancelInvoke();
        }

        private void OnValidate()
        {
            maximumSpeed = Mathf.Max(startingSpeed, maximumSpeed);
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.collider.TryGetComponent(out PaddleMovement paddle))
            {
                BounceFrom(paddle, collision.collider.bounds.extents.y);
                Bounced?.Invoke(body.position);
                return;
            }

            MaintainSpeed();
            Bounced?.Invoke(body.position);
        }

        public void PrepareServe(PlayerSide receivingSide)
        {
            CancelInvoke();
            body.position = startingPosition;
            body.linearVelocity = Vector2.zero;
            currentSpeed = startingSpeed;
            serveDirection = receivingSide == PlayerSide.Left ? -1 : 1;
            verticalServeDirection *= -1;
            Invoke(nameof(Launch), serveDelay);
        }

        public void Stop()
        {
            CancelInvoke();
            body.linearVelocity = Vector2.zero;
            body.position = startingPosition;
        }

        private void Launch()
        {
            float angle = serveAngle * verticalServeDirection * Mathf.Deg2Rad;
            Vector2 direction = new Vector2(serveDirection * Mathf.Cos(angle), Mathf.Sin(angle));
            body.linearVelocity = direction * currentSpeed;
            Served?.Invoke();
        }

        private void BounceFrom(PaddleMovement paddle, float paddleHalfHeight)
        {
            float verticalOffset = (body.position.y - paddle.Position.y) / paddleHalfHeight;
            float angle = Mathf.Clamp(verticalOffset, -1f, 1f) * maximumBounceAngle * Mathf.Deg2Rad;
            float horizontalDirection = body.position.x < paddle.Position.x ? -1f : 1f;

            currentSpeed = Mathf.Min(currentSpeed + speedIncreasePerHit, maximumSpeed);
            body.linearVelocity = new Vector2(
                horizontalDirection * Mathf.Cos(angle),
                Mathf.Sin(angle)
            ) * currentSpeed;
        }

        private void MaintainSpeed()
        {
            if (body.linearVelocity.sqrMagnitude > 0f)
            {
                body.linearVelocity = body.linearVelocity.normalized * currentSpeed;
            }
        }
    }
}
