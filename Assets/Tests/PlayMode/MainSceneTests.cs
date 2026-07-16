using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Pong.Tests
{
    public sealed class MainSceneTests
    {
        [UnityTest]
        public IEnumerator MainScene_StartsPlayableMatch()
        {
            yield return SceneManager.LoadSceneAsync("Main");

            MatchController match = Object.FindFirstObjectByType<MatchController>();
            BallController ball = Object.FindFirstObjectByType<BallController>();
            PaddleMovement[] paddles = Object.FindObjectsByType<PaddleMovement>(FindObjectsSortMode.None);

            Assert.That(match, Is.Not.Null);
            Assert.That(ball, Is.Not.Null);
            Assert.That(paddles, Has.Length.EqualTo(2));

            yield return new WaitForSeconds(1f);

            Assert.That(ball.Velocity.sqrMagnitude, Is.GreaterThan(0f));
        }
    }
}
