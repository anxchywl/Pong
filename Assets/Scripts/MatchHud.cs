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
        private readonly VisualElement leftProgress;
        private readonly VisualElement rightProgress;

        public MatchHud(VisualElement documentRoot)
        {
            root = documentRoot.Q<VisualElement>("hud");
            leftScore = documentRoot.Q<Label>("left-score");
            rightScore = documentRoot.Q<Label>("right-score");
            modeLabel = documentRoot.Q<Label>("hud-mode");
            statusLabel = documentRoot.Q<Label>("hud-status");
            leftProgress = documentRoot.Q<VisualElement>("left-progress");
            rightProgress = documentRoot.Q<VisualElement>("right-progress");
        }

        public void SetMode(string modeName)
        {
            modeLabel.text = modeName.ToUpperInvariant();
        }

        public void Render(MatchState state)
        {
            leftScore.text = state.LeftScore.ToString();
            rightScore.text = state.RightScore.ToString();
            RenderProgress(leftProgress, state.LeftScore, state.PointsToWin);
            RenderProgress(rightProgress, state.RightScore, state.PointsToWin);
            root.EnableInClassList("is-hidden", state.Phase is MatchPhase.FrontEnd or MatchPhase.Paused);

            statusLabel.text = state.Phase switch
            {
                MatchPhase.Serving => "GET READY",
                MatchPhase.Won => state.Winner == PlayerSide.Left ? "YOU WIN" : "RIVAL WINS",
                _ => string.Empty
            };
        }

        /// One pip per point needed to win, filled as they are taken: how much match is left,
        /// without arithmetic. Rebuilt only when the target changes, so scoring costs nothing.
        private static void RenderProgress(VisualElement host, int score, int pointsToWin)
        {
            if (host.childCount != pointsToWin)
            {
                host.Clear();
                for (int index = 0; index < pointsToWin; index++)
                {
                    VisualElement pip = new VisualElement();
                    pip.AddToClassList("hud__pip");
                    host.Add(pip);
                }
            }

            for (int index = 0; index < pointsToWin; index++)
            {
                host[index].EnableInClassList("hud__pip--won", index < score);
            }
        }
    }
}
