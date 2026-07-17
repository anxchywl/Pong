using System;
using UnityEngine;

namespace Pong
{
    public sealed class MatchController : MonoBehaviour
    {
        [SerializeField] private BallController ball;
        [SerializeField, Min(1)] private int pointsToWin = 5;
        [SerializeField] private PlayerSide openingServeReceiver = PlayerSide.Left;

        private MatchScore score;
        private MatchPhase phase = MatchPhase.FrontEnd;
        private MatchPhase phaseBeforePause = MatchPhase.Playing;
        private float gameSpeed = 1f;

        public event Action<MatchState> StateChanged;

        public MatchState State => new MatchState(score ??= new MatchScore(pointsToWin), phase);

        private void Awake()
        {
            score = new MatchScore(pointsToWin);
        }

        private void OnEnable()
        {
            ball.Served += HandleServed;
        }

        private void Start()
        {
            EnterFrontEnd();
        }

        private void OnDisable()
        {
            ball.Served -= HandleServed;
            Time.timeScale = 1f;
        }

        public void AwardPoint(PlayerSide scoringSide)
        {
            if (phase is MatchPhase.FrontEnd or MatchPhase.Won || score.Winner.HasValue)
            {
                return;
            }

            score.AddPoint(scoringSide);

            if (score.Winner.HasValue)
            {
                ball.Stop();
                SetPhase(MatchPhase.Won);
                return;
            }

            PlayerSide receivingSide = scoringSide == PlayerSide.Left ? PlayerSide.Right : PlayerSide.Left;
            SetPhase(MatchPhase.Serving);
            ball.PrepareServe(receivingSide);
        }

        public void StartMatch(int winningScore, float speed)
        {
            pointsToWin = Mathf.Max(1, winningScore);
            gameSpeed = Mathf.Clamp(speed, 0.5f, 2f);
            score = new MatchScore(pointsToWin);
            Time.timeScale = gameSpeed;
            SetPhase(MatchPhase.Serving);
            ball.PrepareServe(openingServeReceiver);
        }

        public void RestartMatch()
        {
            if (phase == MatchPhase.FrontEnd)
            {
                return;
            }

            score.Reset();
            Time.timeScale = gameSpeed;
            SetPhase(MatchPhase.Serving);
            ball.PrepareServe(openingServeReceiver);
        }

        public void TogglePause()
        {
            if (phase == MatchPhase.Paused)
            {
                ResumeMatch();
                return;
            }

            if (phase is MatchPhase.Serving or MatchPhase.Playing)
            {
                phaseBeforePause = phase;
                Time.timeScale = 0f;
                SetPhase(MatchPhase.Paused);
            }
        }

        public void ResumeMatch()
        {
            if (phase != MatchPhase.Paused)
            {
                return;
            }

            Time.timeScale = gameSpeed;
            SetPhase(phaseBeforePause);
        }

        public void EnterFrontEnd()
        {
            ball.Stop();
            score.Reset();
            Time.timeScale = 0f;
            SetPhase(MatchPhase.FrontEnd);
        }

        private void HandleServed()
        {
            if (phase == MatchPhase.Serving)
            {
                SetPhase(MatchPhase.Playing);
            }
        }

        private void SetPhase(MatchPhase value)
        {
            phase = value;
            StateChanged?.Invoke(State);
        }
    }
}
