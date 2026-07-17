using UnityEngine;

namespace Pong
{
    /// Turns a point on the screen into a point on the court.
    ///
    /// This is the whole of what input is allowed to know about presentation. The camera already
    /// carries whatever the framing strategy decided — a roll, a size, anything a later one does —
    /// so asking it converts through that automatically. Nothing upstream reads the orientation,
    /// which is why touch needs no idea which way up the court is drawn.
    [RequireComponent(typeof(Camera))]
    public sealed class CourtProjection : MonoBehaviour
    {
        private Camera gameplayCamera;

        private Camera Lens => gameplayCamera != null ? gameplayCamera : gameplayCamera = GetComponent<Camera>();

        public Vector2 ToCourt(Vector2 screenPoint)
        {
            return Lens.ScreenToWorldPoint(screenPoint);
        }
    }

    /// A band of the court a seat answers to. Bands never overlap, so a finger belongs to exactly
    /// one seat and no arbiter is needed to say which.
    public readonly struct CourtRegion
    {
        public CourtRegion(float minX, float maxX)
        {
            MinX = Mathf.Min(minX, maxX);
            MaxX = Mathf.Max(minX, maxX);
        }

        public static CourtRegion Nowhere => new CourtRegion(0f, 0f);

        public float MinX { get; }
        public float MaxX { get; }

        public bool Contains(Vector2 courtPoint)
        {
            return MaxX > MinX && courtPoint.x >= MinX && courtPoint.x <= MaxX;
        }
    }
}
