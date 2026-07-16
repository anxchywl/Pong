namespace Pong
{
    public enum MatchPhase
    {
        FrontEnd,
        Serving,
        Playing,
        Paused,
        Won
    }

    public readonly struct MatchState
    {
        public MatchState(MatchScore score, MatchPhase phase)
        {
            LeftScore = score.Left;
            RightScore = score.Right;
            PointsToWin = score.PointsToWin;
            Winner = score.Winner;
            Phase = phase;
        }

        public int LeftScore { get; }
        public int RightScore { get; }
        public int PointsToWin { get; }
        public PlayerSide? Winner { get; }
        public MatchPhase Phase { get; }
    }
}
