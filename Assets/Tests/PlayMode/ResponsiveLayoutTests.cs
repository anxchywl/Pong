using System.Collections;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Pong.Tests
{
    /// The breakpoints are checked as maths elsewhere. These check the panel is actually wearing
    /// the answer, and that it measures itself rather than the machine.
    public sealed class ResponsiveLayoutTests
    {
        private static readonly string[] SizeClasses =
        {
            "layout--compact", "layout--medium", "layout--expanded"
        };

        [UnityTest]
        public IEnumerator ThePanelWearsExactlyOneSizeClass()
        {
            yield return Load();
            VisualElement root = Root();

            string[] worn = SizeClasses.Where(root.ClassListContains).ToArray();

            Assert.That(worn.Length, Is.EqualTo(1),
                $"expected one size class, wearing: {string.Join(", ", worn)}");
        }

        [UnityTest]
        public IEnumerator TheSizeClassMatchesTheRoomThePanelHas()
        {
            yield return Load();
            VisualElement root = Root();

            float width = root.resolvedStyle.width;
            Assert.That(width, Is.GreaterThan(0f), "the panel never resolved a width");

            string expected = LayoutBreakpoints.ClassFor(LayoutBreakpoints.For(width));

            Assert.That(root.ClassListContains(expected), Is.True,
                $"a panel {width} points wide is not wearing {expected}");
        }

        [UnityTest]
        public IEnumerator ThePanelIsEitherTallOrWideAndNeverBoth()
        {
            yield return Load();
            VisualElement root = Root();

            bool tall = root.ClassListContains("layout--tall");
            bool wide = root.ClassListContains("layout--wide");

            Assert.That(tall && wide, Is.False, "the panel claims to be tall and wide at once");
            Assert.That(tall || wide, Is.True, "the panel claims to be neither tall nor wide");
            Assert.That(tall, Is.EqualTo(root.resolvedStyle.height > root.resolvedStyle.width));
        }

        private static IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            // one frame to run the layout, one for the controller to read it back
            yield return null;
            yield return null;
        }

        private static VisualElement Root()
        {
            return Object.FindAnyObjectByType<UIDocument>().rootVisualElement;
        }
    }
}
