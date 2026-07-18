using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Pong.Tests
{
    /// Binding an action proves nothing about playing the game. These press keys on a virtual
    /// keyboard and watch the court, so the whole path is covered: device to action to intent to
    /// physics.
    public sealed class GameplayInputTests
    {
        private const float Settle = 0.3f;

        private Keyboard keyboard;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorRouting;
#endif

        [SetUp]
        public void AddKeyboard()
        {
            // a test runner holds no focus and no game view, and by default the Input System wipes
            // device state while unfocused and routes keys away from an unfocused game view. Either
            // one drops a simulated key before anything can read it
            background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorRouting = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            keyboard = InputSystem.AddDevice<Keyboard>();
        }

        [TearDown]
        public void RemoveKeyboard()
        {
            if (keyboard != null && keyboard.added)
            {
                InputSystem.RemoveDevice(keyboard);
            }

            InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorRouting;
#endif
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator W_MovesTheKeyboardPlayersPaddleUp()
        {
            yield return LoadPlayingMatch();

            PaddleMovement paddle = PaddleFor(PlayerSide.Left);
            float start = paddle.Position.y;

            yield return Hold(Key.W);

            Assert.That(paddle.Position.y, Is.GreaterThan(start + 0.5f),
                "W did not drive the left paddle up");
        }

        [UnityTest]
        public IEnumerator S_MovesTheKeyboardPlayersPaddleDown()
        {
            yield return LoadPlayingMatch();

            PaddleMovement paddle = PaddleFor(PlayerSide.Left);
            float start = paddle.Position.y;

            yield return Hold(Key.S);

            Assert.That(paddle.Position.y, Is.LessThan(start - 0.5f),
                "S did not drive the left paddle down");
        }

        [UnityTest]
        public IEnumerator ReleasingTheKeyStopsThePaddle()
        {
            yield return LoadPlayingMatch();

            PaddleMovement paddle = PaddleFor(PlayerSide.Left);
            yield return Hold(Key.W);
            yield return Release();
            float resting = paddle.Position.y;
            yield return new WaitForSeconds(Settle);

            Assert.That(paddle.Position.y, Is.EqualTo(resting).Within(0.01f),
                "the paddle kept moving after the key came up");
        }

        /// The point of control schemes: one keyboard, two players, neither aware of the other.
        [UnityTest]
        public IEnumerator TwoPlayersShareOneKeyboardWithoutDrivingEachOthersPaddle()
        {
            yield return LoadScene();
            SeatDirector seats = Object.FindAnyObjectByType<SeatDirector>();
            seats.Roster.Assign(
                new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper),
                SeatAssignment.Human("keyboard-arrows", SeatAssignment.NoDevice)
            );
            yield return StartMatch();

            PaddleMovement left = PaddleFor(PlayerSide.Left);
            PaddleMovement right = PaddleFor(PlayerSide.Right);
            float leftStart = left.Position.y;
            float rightStart = right.Position.y;

            yield return Hold(Key.W);

            Assert.That(left.Position.y, Is.GreaterThan(leftStart + 0.5f), "W did not move its own paddle");
            Assert.That(right.Position.y, Is.EqualTo(rightStart).Within(0.01f),
                "W moved the arrow player's paddle as well");

            yield return Release();
            leftStart = left.Position.y;

            yield return Hold(Key.DownArrow);

            Assert.That(right.Position.y, Is.LessThan(rightStart - 0.5f), "down arrow did not move its own paddle");
            Assert.That(left.Position.y, Is.EqualTo(leftStart).Within(0.01f),
                "down arrow moved the WASD player's paddle as well");
        }

        [UnityTest]
        public IEnumerator P_PausesAndResumesTheMatch()
        {
            yield return LoadPlayingMatch();
            MatchController match = Object.FindAnyObjectByType<MatchController>();

            yield return Tap(Key.P);
            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Paused), "P did not pause");

            yield return Tap(Key.P);
            Assert.That(match.State.Phase, Is.Not.EqualTo(MatchPhase.Paused), "P did not resume");
        }

        [UnityTest]
        public IEnumerator R_RestartsTheMatch()
        {
            yield return LoadPlayingMatch();
            MatchController match = Object.FindAnyObjectByType<MatchController>();
            match.AwardPoint(PlayerSide.Left);
            Assert.That(match.State.LeftScore, Is.EqualTo(1), "the point under test did not land");

            yield return Tap(Key.R);

            Assert.That(match.State.LeftScore, Is.Zero, "R did not clear the score");
            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Serving));
        }

        /// Escape reaches the UI as Cancel, never as a key, and pausing is what Cancel means mid-rally.
        [UnityTest]
        public IEnumerator Escape_PausesThroughTheUiCancelAction()
        {
            yield return LoadPlayingMatch();
            MatchController match = Object.FindAnyObjectByType<MatchController>();

            yield return Tap(Key.Escape);

            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Paused), "escape did not reach HandleEscape");
        }

        private static IEnumerator LoadScene()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
        }

        private static IEnumerator StartMatch()
        {
            Object.FindAnyObjectByType<MatchController>().StartMatch(5, 1f);
            yield return null;
        }

        private static IEnumerator LoadPlayingMatch()
        {
            yield return LoadScene();
            yield return StartMatch();
        }

        private static PaddleMovement PaddleFor(PlayerSide side)
        {
            PaddleSeat seat = Object.FindObjectsByType<PaddleSeat>(FindObjectsSortMode.None)
                .First(candidate => candidate.Seat.Side == side && candidate.Seat.Role == SeatRole.Goalkeeper);
            return seat.GetComponent<PaddleMovement>();
        }

        private IEnumerator Hold(params Key[] keys)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
            InputSystem.Update();
            yield return new WaitForSeconds(Settle);
        }

        private IEnumerator Release()
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState());
            InputSystem.Update();
            yield return null;
        }

        private IEnumerator Tap(Key key)
        {
            InputSystem.QueueStateEvent(keyboard, new KeyboardState(key));
            InputSystem.Update();
            yield return null;
            yield return Release();
        }
    }
}
