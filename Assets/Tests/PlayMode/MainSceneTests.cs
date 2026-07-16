using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Pong.Tests
{
    public sealed class MainSceneTests
    {
        [TearDown]
        public void ResetTimeScale()
        {
            Time.timeScale = 1f;
        }

        [UnityTest]
        public IEnumerator MainScene_StartsInFrontEnd()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            MatchController match = Object.FindAnyObjectByType<MatchController>();
            BallController ball = Object.FindAnyObjectByType<BallController>();
            PaddleMovement[] paddles = Object.FindObjectsByType<PaddleMovement>();
            GameUiController ui = Object.FindAnyObjectByType<GameUiController>();

            Assert.That(match, Is.Not.Null);
            Assert.That(ball, Is.Not.Null);
            Assert.That(paddles, Has.Length.EqualTo(2));
            Assert.That(ui, Is.Not.Null);
            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.FrontEnd));
            Assert.That(ball.Velocity, Is.EqualTo(Vector2.zero));
            Assert.That(Time.timeScale, Is.Zero);
        }

        [UnityTest]
        public IEnumerator MainScene_StartsPlayableMatch()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            MatchController match = Object.FindAnyObjectByType<MatchController>();
            BallController ball = Object.FindAnyObjectByType<BallController>();
            match.StartMatch(5, 1f);

            yield return new WaitForSeconds(1f);

            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Playing));
            Assert.That(ball.Velocity.sqrMagnitude, Is.GreaterThan(0f));
        }

        [UnityTest]
        public IEnumerator PauseAndResume_PreserveActiveMatch()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            MatchController match = Object.FindAnyObjectByType<MatchController>();
            match.StartMatch(7, 1.25f);
            yield return new WaitForSeconds(1f);

            match.TogglePause();

            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Paused));
            Assert.That(Time.timeScale, Is.Zero);

            match.ResumeMatch();

            Assert.That(match.State.Phase, Is.EqualTo(MatchPhase.Playing));
            Assert.That(Time.timeScale, Is.EqualTo(1.25f));
        }

        [UnityTest]
        public IEnumerator ThemeSelection_SwitchesAtRuntimeAndPersists()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            GameUiController ui = Object.FindAnyObjectByType<GameUiController>();
            MatchController match = Object.FindAnyObjectByType<MatchController>();
            BallController ball = Object.FindAnyObjectByType<BallController>();
            string originalTheme = ui.ActiveThemeId;
            string alternateTheme = originalTheme == "retro" ? "futuristic" : "retro";
            match.StartMatch(5, 1f);
            yield return new WaitForSeconds(1f);
            MatchState stateBeforeSwitch = match.State;
            Vector2 velocityBeforeSwitch = ball.Velocity;

            try
            {
                ui.SelectTheme(alternateTheme);

                Assert.That(ui.ActiveThemeId, Is.EqualTo(alternateTheme));
                Assert.That(PlayerPrefs.GetString("pong.selection.theme"), Is.EqualTo(alternateTheme));
                Assert.That(match.State.Phase, Is.EqualTo(stateBeforeSwitch.Phase));
                Assert.That(match.State.LeftScore, Is.EqualTo(stateBeforeSwitch.LeftScore));
                Assert.That(match.State.RightScore, Is.EqualTo(stateBeforeSwitch.RightScore));
                Assert.That(ball.Velocity, Is.EqualTo(velocityBeforeSwitch));
            }
            finally
            {
                ui.SelectTheme(originalTheme);
            }
        }
    }
}
