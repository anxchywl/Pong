using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Pong.Tests
{
    /// Fingers on a court. These press a virtual touchscreen and watch the paddles, so the whole
    /// path is covered: touch to court to intent to physics.
    public sealed class TouchInputTests
    {
        private static readonly CourtSeat LeftKeeper = new CourtSeat(PlayerSide.Left, SeatRole.Goalkeeper);
        private static readonly CourtSeat RightKeeper = new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper);

        private const float Settle = 0.3f;

        private readonly HashSet<int> down = new HashSet<int>();

        private Touchscreen screen;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorRouting;
#endif

        [SetUp]
        public void AddTouchscreen()
        {
            down.Clear();
            background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorRouting = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            screen = InputSystem.AddDevice<Touchscreen>();
        }

        [TearDown]
        public void RemoveTouchscreen()
        {
            if (screen != null && screen.added)
            {
                InputSystem.RemoveDevice(screen);
            }

            InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorRouting;
#endif
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator DraggingYourSideOfTheCourtMovesYourPaddle()
        {
            yield return SeatTouchOnLeft();
            PaddleMovement paddle = PaddleFor(PlayerSide.Left);
            float start = paddle.Position.y;

            yield return Drag(1, CourtPoint(-7.5f, start + 3f));

            Assert.That(paddle.Position.y, Is.GreaterThan(start + 0.5f), "a drag did not move the paddle");
        }

        [UnityTest]
        public IEnumerator ThePaddleFollowsTheFingerBothWays()
        {
            yield return SeatTouchOnLeft();
            PaddleMovement paddle = PaddleFor(PlayerSide.Left);

            yield return Drag(1, CourtPoint(-7.5f, 3f));
            float high = paddle.Position.y;
            yield return Drag(1, CourtPoint(-7.5f, -3f));

            Assert.That(paddle.Position.y, Is.LessThan(high - 0.5f), "the paddle would not come back down");
        }

        [UnityTest]
        public IEnumerator LiftingTheFingerStopsThePaddle()
        {
            yield return SeatTouchOnLeft();
            PaddleMovement paddle = PaddleFor(PlayerSide.Left);

            yield return Drag(1, CourtPoint(-7.5f, 3f));
            yield return Lift(1);
            float resting = paddle.Position.y;
            yield return new WaitForSeconds(Settle);

            Assert.That(paddle.Position.y, Is.EqualTo(resting).Within(0.01f), "the paddle drifted after the lift");
        }

        /// A finger on the other side of the court is someone else's.
        [UnityTest]
        public IEnumerator AFingerOnTheOtherSideDoesNotMoveYourPaddle()
        {
            yield return SeatTouchOnLeft();
            PaddleMovement paddle = PaddleFor(PlayerSide.Left);
            float start = paddle.Position.y;

            yield return Drag(1, CourtPoint(7.5f, start + 3f));

            Assert.That(paddle.Position.y, Is.EqualTo(start).Within(0.01f),
                "a touch on the far side drove this paddle");
        }

        /// The point of regions: two players, two fingers, one screen, at the same time.
        [UnityTest]
        public IEnumerator TwoFingersDriveTwoPaddlesAtOnce()
        {
            yield return LoadScene();
            SeatDirector seats = Object.FindAnyObjectByType<SeatDirector>();
            seats.Roster.Assign(LeftKeeper, SeatAssignment.Human("touch", SeatAssignment.NoDevice, exclusive: false));
            seats.Roster.Assign(RightKeeper, SeatAssignment.Human("touch", SeatAssignment.NoDevice, exclusive: false));
            yield return StartMatch();

            PaddleMovement left = PaddleFor(PlayerSide.Left);
            PaddleMovement right = PaddleFor(PlayerSide.Right);
            float leftStart = left.Position.y;
            float rightStart = right.Position.y;

            Press(1, CourtPoint(-7.5f, leftStart + 3f));
            Press(2, CourtPoint(7.5f, rightStart - 3f));
            InputSystem.Update();
            yield return new WaitForSeconds(Settle);

            Assert.That(left.Position.y, Is.GreaterThan(leftStart + 0.5f), "the left finger moved nothing");
            Assert.That(right.Position.y, Is.LessThan(rightStart - 0.5f), "the right finger moved nothing");
        }

        /// Touch answers to the court, not the screen, so a rolled camera needs no separate path.
        [UnityTest]
        public IEnumerator ADragFollowsTheCourtEvenWhenItIsDrawnSideways()
        {
            yield return SeatTouchOnLeft();
            Camera camera = Camera.main;
            camera.transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            yield return null;

            PaddleMovement paddle = PaddleFor(PlayerSide.Left);
            float start = paddle.Position.y;

            // the same court point, projected through a camera that is now on its side
            yield return Drag(1, CourtPoint(-7.5f, start + 3f));

            Assert.That(paddle.Position.y, Is.GreaterThan(start + 0.5f),
                "the drag stopped working once the court was drawn sideways");

            camera.transform.rotation = Quaternion.identity;
        }

        private IEnumerator SeatTouchOnLeft()
        {
            yield return LoadScene();
            SeatDirector seats = Object.FindAnyObjectByType<SeatDirector>();
            seats.Roster.Assign(LeftKeeper, SeatAssignment.Human("touch", SeatAssignment.NoDevice, exclusive: false));
            yield return StartMatch();
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

        /// Where on the screen a point of the court currently is.
        private static Vector2 CourtPoint(float x, float y)
        {
            return Camera.main.WorldToScreenPoint(new Vector3(x, y, 0f));
        }

        private static PaddleMovement PaddleFor(PlayerSide side)
        {
            PaddleSeat seat = Object.FindObjectsByType<PaddleSeat>(FindObjectsSortMode.None)
                .First(candidate => candidate.Seat.Side == side && candidate.Seat.Role == SeatRole.Goalkeeper);
            return seat.GetComponent<PaddleMovement>();
        }

        /// A touchscreen only knows a finger once it has begun, so a drag has to start before it
        /// can move. Sending Moved on its own registers nothing at all.
        private void Press(int id, Vector2 position)
        {
            if (down.Add(id))
            {
                InputSystem.QueueStateEvent(screen, new TouchState
                {
                    touchId = id,
                    phase = UnityEngine.InputSystem.TouchPhase.Began,
                    position = position
                });
                InputSystem.Update();
            }

            InputSystem.QueueStateEvent(screen, new TouchState
            {
                touchId = id,
                phase = UnityEngine.InputSystem.TouchPhase.Moved,
                position = position
            });
        }

        private IEnumerator Drag(int id, Vector2 position)
        {
            Press(id, position);
            InputSystem.Update();
            yield return new WaitForSeconds(Settle);
        }

        private IEnumerator Lift(int id)
        {
            down.Remove(id);
            InputSystem.QueueStateEvent(screen, new TouchState
            {
                touchId = id,
                phase = UnityEngine.InputSystem.TouchPhase.Ended,
                position = Vector2.zero
            });
            InputSystem.Update();
            yield return null;
        }
    }
}
