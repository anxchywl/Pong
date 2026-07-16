using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    public sealed class MatchController : MonoBehaviour
    {
        [SerializeField] private BallController ball;
        [SerializeField] private MatchHud hud;
        [SerializeField, Min(1)] private int pointsToWin = 5;
        [SerializeField] private PlayerSide openingServeReceiver = PlayerSide.Left;

        private MatchScore score;
        private bool isPaused;

        private void Awake()
        {
            score = new MatchScore(pointsToWin);
        }

        private void OnEnable()
        {
            ball.Served += hud.ClearStatus;
        }

        private void Start()
        {
            hud.ShowScore(score);
            hud.ShowReady();
            ball.PrepareServe(openingServeReceiver);
        }

        private void Update()
        {
            if (PauseWasPressed() && !score.Winner.HasValue)
            {
                SetPaused(!isPaused);
            }

            if (RestartWasPressed())
            {
                RestartMatch();
            }
        }

        private void OnDisable()
        {
            ball.Served -= hud.ClearStatus;
            Time.timeScale = 1f;
        }

        public void AwardPoint(PlayerSide scoringSide)
        {
            if (score.Winner.HasValue)
            {
                return;
            }

            score.AddPoint(scoringSide);
            hud.ShowScore(score);

            if (score.Winner.HasValue)
            {
                ball.Stop();
                hud.ShowWinner(score.Winner.Value);
                return;
            }

            PlayerSide receivingSide = scoringSide == PlayerSide.Left ? PlayerSide.Right : PlayerSide.Left;
            hud.ClearStatus();
            ball.PrepareServe(receivingSide);
        }

        public void RestartMatch()
        {
            SetPaused(false);
            score.Reset();
            hud.ShowScore(score);
            hud.ShowReady();
            ball.PrepareServe(openingServeReceiver);
        }

        private void SetPaused(bool paused)
        {
            isPaused = paused;
            Time.timeScale = paused ? 0f : 1f;

            if (paused)
            {
                hud.ShowPaused();
            }
            else
            {
                hud.ClearStatus();
            }
        }

        private static bool PauseWasPressed()
        {
            bool keyboardPressed = Keyboard.current != null &&
                (Keyboard.current.escapeKey.wasPressedThisFrame || Keyboard.current.pKey.wasPressedThisFrame);
            bool gamepadPressed = Gamepad.current != null && Gamepad.current.startButton.wasPressedThisFrame;
            return keyboardPressed || gamepadPressed;
        }

        private static bool RestartWasPressed()
        {
            bool keyboardPressed = Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame;
            bool gamepadPressed = Gamepad.current != null && Gamepad.current.selectButton.wasPressedThisFrame;
            return keyboardPressed || gamepadPressed;
        }
    }
}
