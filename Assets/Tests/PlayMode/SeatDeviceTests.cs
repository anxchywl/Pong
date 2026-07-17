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
    /// Pads get unplugged mid-rally. These add and remove a virtual one and watch what the court
    /// does about it.
    public sealed class SeatDeviceTests
    {
        private static readonly CourtSeat RightKeeper = new CourtSeat(PlayerSide.Right, SeatRole.Goalkeeper);

        private Gamepad pad;
        private InputSettings.BackgroundBehavior background;
#if UNITY_EDITOR
        private InputSettings.EditorInputBehaviorInPlayMode editorRouting;
#endif

        [SetUp]
        public void AddPad()
        {
            background = InputSystem.settings.backgroundBehavior;
            InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
#if UNITY_EDITOR
            editorRouting = InputSystem.settings.editorInputBehaviorInPlayMode;
            InputSystem.settings.editorInputBehaviorInPlayMode =
                InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
#endif
            pad = InputSystem.AddDevice<Gamepad>();
        }

        [TearDown]
        public void RemovePad()
        {
            if (pad != null && pad.added)
            {
                InputSystem.RemoveDevice(pad);
            }

            InputSystem.settings.backgroundBehavior = background;
#if UNITY_EDITOR
            InputSystem.settings.editorInputBehaviorInPlayMode = editorRouting;
#endif
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator AGamepadSeatIsDrivenByItsOwnPad()
        {
            yield return SeatPadOnRight();

            PaddleMovement paddle = PaddleFor(PlayerSide.Right);
            float start = paddle.Position.y;

            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(0f, 1f) });
            InputSystem.Update();
            yield return new WaitForSeconds(0.3f);

            Assert.That(paddle.Position.y, Is.GreaterThan(start + 0.5f), "the pad did not move its paddle");
        }

        /// Losing a pad must not cost the point being played.
        [UnityTest]
        public IEnumerator LosingAPadPausesTheMatchAndKeepsTheSeat()
        {
            yield return SeatPadOnRight();
            SeatDirector seats = Object.FindAnyObjectByType<SeatDirector>();
            MatchController match = Object.FindAnyObjectByType<MatchController>();

            InputSystem.RemoveDevice(pad);
            yield return null;

            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Paused), "an unplugged pad did not pause the match");
            Assert.That(seats.Roster.Get(RightKeeper).Occupant, Is.EqualTo(SeatOccupant.Human),
                "the seat was vacated rather than held");
        }

        /// A real pad carries a description, so the Input System keeps it and gives it its own id
        /// back when it returns. A virtual one does not, so re-adding the instance is how a
        /// reconnect is modelled here; adding a fresh device would be a different pad entirely.
        [UnityTest]
        public IEnumerator APadComingBackPicksItsPaddleUpAgain()
        {
            yield return SeatPadOnRight();
            MatchController match = Object.FindAnyObjectByType<MatchController>();
            int claimed = pad.deviceId;

            InputSystem.RemoveDevice(pad);
            yield return null;
            InputSystem.AddDevice(pad);
            yield return null;

            Assert.That(pad.deviceId, Is.EqualTo(claimed), "the pad did not come back as itself");

            match.ResumeMatch();
            PaddleMovement paddle = PaddleFor(PlayerSide.Right);
            float start = paddle.Position.y;

            InputSystem.QueueStateEvent(pad, new GamepadState { leftStick = new Vector2(0f, 1f) });
            InputSystem.Update();
            yield return new WaitForSeconds(0.3f);

            Assert.That(paddle.Position.y, Is.GreaterThan(start + 0.5f),
                "the paddle did not answer the pad after it came back");
        }

        /// Someone else's pad must not inherit a seat by turning up. The seat keeps its claim on the
        /// pad that left, and the player reseats from the Players screen if they want the new one.
        [UnityTest]
        public IEnumerator ADifferentPadDoesNotInheritTheSeat()
        {
            yield return SeatPadOnRight();
            SeatDirector seats = Object.FindAnyObjectByType<SeatDirector>();
            MatchController match = Object.FindAnyObjectByType<MatchController>();
            int claimed = pad.deviceId;

            InputSystem.RemoveDevice(pad);
            yield return null;
            Gamepad stranger = InputSystem.AddDevice<Gamepad>();
            yield return null;

            try
            {
                Assert.That(stranger.deviceId, Is.Not.EqualTo(claimed), "the stranger reused the claimed id");
                Assert.That(seats.Roster.Get(RightKeeper).DeviceId, Is.EqualTo(claimed),
                    "the seat let a different pad take it over");

                match.ResumeMatch();
                PaddleMovement paddle = PaddleFor(PlayerSide.Right);
                float start = paddle.Position.y;
                InputSystem.QueueStateEvent(stranger, new GamepadState { leftStick = new Vector2(0f, 1f) });
                InputSystem.Update();
                yield return new WaitForSeconds(0.3f);

                Assert.That(paddle.Position.y, Is.EqualTo(start).Within(0.01f),
                    "a pad that was never seated drove someone's paddle");
            }
            finally
            {
                InputSystem.RemoveDevice(stranger);
            }
        }

        /// A keyboard seat has no device to lose, so a pad leaving is none of its business.
        [UnityTest]
        public IEnumerator LosingAnUnseatedPadLeavesTheMatchAlone()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            MatchController match = Object.FindAnyObjectByType<MatchController>();
            match.StartMatch(5, 1f);
            yield return null;

            InputSystem.RemoveDevice(pad);
            yield return null;

            Assert.That(match.State.Phase, Is.Not.EqualTo(MatchPhase.Paused),
                "a pad nobody was using paused the match");
        }

        private IEnumerator SeatPadOnRight()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            SeatDirector seats = Object.FindAnyObjectByType<SeatDirector>();
            seats.Roster.Assign(RightKeeper, SeatAssignment.Human("gamepad", pad.deviceId));
            Object.FindAnyObjectByType<MatchController>().StartMatch(5, 1f);
            yield return null;
        }

        private static PaddleMovement PaddleFor(PlayerSide side)
        {
            PaddleSeat seat = Object.FindObjectsByType<PaddleSeat>(FindObjectsSortMode.None)
                .First(candidate => candidate.Seat.Side == side && candidate.Seat.Role == SeatRole.Goalkeeper);
            return seat.GetComponent<PaddleMovement>();
        }
    }
}
