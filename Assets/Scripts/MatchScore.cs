using System;

namespace Pong
{
    public sealed class MatchScore
    {
        public MatchScore(int pointsToWin)
        {
            if (pointsToWin < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(pointsToWin));
            }

            PointsToWin = pointsToWin;
        }

        public int Left { get; private set; }
        public int Right { get; private set; }
        public int PointsToWin { get; }
        public PlayerSide? Winner { get; private set; }

        public void AddPoint(PlayerSide side)
        {
            if (Winner.HasValue)
            {
                return;
            }

            switch (side)
            {
                case PlayerSide.Left:
                    Left++;
                    break;
                case PlayerSide.Right:
                    Right++;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(side));
            }

            if (GetPoints(side) >= PointsToWin)
            {
                Winner = side;
            }
        }

        public int GetPoints(PlayerSide side)
        {
            return side switch
            {
                PlayerSide.Left => Left,
                PlayerSide.Right => Right,
                _ => throw new ArgumentOutOfRangeException(nameof(side))
            };
        }

        public void Reset()
        {
            Left = 0;
            Right = 0;
            Winner = null;
        }
    }
}
