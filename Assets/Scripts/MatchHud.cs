using UnityEngine;
using UnityEngine.UI;

namespace Pong
{
    public sealed class MatchHud : MonoBehaviour
    {
        [SerializeField] private Text scoreText;
        [SerializeField] private Text statusText;

        public void ShowScore(MatchScore score)
        {
            scoreText.text = $"{score.Left}   {score.Right}";
        }

        public void ShowReady()
        {
            statusText.text = "GET READY";
        }

        public void ShowPaused()
        {
            statusText.text = "PAUSED";
        }

        public void ShowWinner(PlayerSide winner)
        {
            statusText.text = $"{winner.ToString().ToUpperInvariant()} WINS\nR / SELECT TO RESTART";
        }

        public void ClearStatus()
        {
            statusText.text = string.Empty;
        }
    }
}
