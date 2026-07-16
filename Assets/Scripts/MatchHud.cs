using UnityEngine.UIElements;

namespace Pong
{
    public sealed class MatchHud
    {
        private readonly VisualElement root;
        private readonly Label leftScore;
        private readonly Label rightScore;
        private readonly Label modeLabel;
        private readonly Label statusLabel;

        public MatchHud(VisualElement documentRoot)
        {
            root = documentRoot.Q<VisualElement>("hud");
            leftScore = documentRoot.Q<Label>("left-score");
            rightScore = documentRoot.Q<Label>("right-score");
            modeLabel = documentRoot.Q<Label>("hud-mode");
            statusLabel = documentRoot.Q<Label>("hud-status");
        }

        public void SetMode(string modeName)
        {
            modeLabel.text = modeName.ToUpperInvariant();
        }

        public void Render(MatchState state)
        {
            leftScore.text = state.LeftScore.ToString();
            rightScore.text = state.RightScore.ToString();
            root.EnableInClassList("is-hidden", state.Phase is MatchPhase.FrontEnd or MatchPhase.Paused);

            statusLabel.text = state.Phase switch
            {
                MatchPhase.Serving => "GET READY",
                MatchPhase.Won => state.Winner == PlayerSide.Left ? "YOU WIN" : "RIVAL WINS",
                _ => string.Empty
            };
        }
    }
}
