using UnityEngine;

namespace Pong
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Goal : MonoBehaviour
    {
        [SerializeField] private MatchController match;
        [SerializeField] private PlayerSide scoringSide;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent<BallController>(out _))
            {
                match.AwardPoint(scoringSide);
            }
        }
    }
}
