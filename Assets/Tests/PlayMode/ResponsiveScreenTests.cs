using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UIElements;

namespace Pong.Tests
{
    /// Every screen, at every width one can have. A layout that overflows is not responsive however
    /// many breakpoints it declares, so these measure the screens rather than trust the classes.
    ///
    /// The panel is sized by leaning on physical scaling: points are pixels over dpi against the
    /// reference, so moving the reference resizes the panel exactly as a different display would.
    public sealed class ResponsiveScreenTests
    {
        private static readonly string[] Screens =
        {
            "main-menu-screen", "players-screen", "game-mode-screen",
            "customization-screen", "settings-screen", "credits-screen"
        };

        /// A phone upright, a phone on its side, a small tablet, a large tablet, a desktop window.
        private static readonly float[] Widths = { 274f, 530f, 600f, 820f, 1920f };

        private float originalDpi;
        private UIDocument document;

        [TearDown]
        public void RestoreDpi()
        {
            if (document != null && document.panelSettings != null)
            {
                document.panelSettings.referenceDpi = originalDpi;
            }
        }

        [UnityTest]
        public IEnumerator NoScreenOverflowsTheWidthItIsGiven()
        {
            yield return Load();

            foreach (float width in Widths)
            {
                yield return Resize(width);
                float panelWidth = Root().resolvedStyle.width;

                foreach (string screen in Screens)
                {
                    yield return Reveal(screen);
                    float rightmost = Rightmost(Root().Q<VisualElement>(screen));

                    Assert.That(rightmost, Is.LessThanOrEqualTo(panelWidth + 1f),
                        $"{screen} runs {rightmost - panelWidth:0} points off a {panelWidth:0} point panel");

                    yield return Hide(screen);
                }
            }
        }

        /// Content taller than the screen is fine. Content taller than the screen with nowhere to go
        /// is a button a player cannot reach.
        [UnityTest]
        public IEnumerator AnythingTallerThanItsScreenCanBeScrolledTo()
        {
            yield return Load();
            yield return Resize(274f);
            float panelHeight = Root().resolvedStyle.height;

            foreach (string screen in Screens)
            {
                yield return Reveal(screen);
                VisualElement root = Root().Q<VisualElement>(screen);

                if (Lowest(root, out VisualElement deepest) > panelHeight + 1f)
                {
                    Assert.That(IsInsideAScrollView(deepest), Is.True,
                        $"{screen} puts content below the fold with no way to scroll to it");
                }

                yield return Hide(screen);
            }
        }

        /// The desktop layout is the one that already existed, and this phase must not have moved it.
        [UnityTest]
        public IEnumerator TheDesktopKeepsTheLayoutItAlreadyHad()
        {
            yield return Load();
            yield return Resize(1920f);
            VisualElement root = Root();

            Assert.That(root.ClassListContains("layout--expanded"), Is.True);
            Assert.That(root.Q<VisualElement>(className: "main-content").resolvedStyle.flexDirection,
                Is.EqualTo(FlexDirection.Row), "the desktop menu stopped sitting side by side");
            Assert.That(root.Q<VisualElement>(className: "main-content__navigation").resolvedStyle.width,
                Is.EqualTo(471f).Within(1f), "the desktop navigation column changed width");
            Assert.That(root.Q<VisualElement>(className: "nav-stack").resolvedStyle.width,
                Is.EqualTo(429f).Within(1f), "the desktop navigation stack changed width");
        }

        private IEnumerator Load()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;
            document = Object.FindAnyObjectByType<UIDocument>();
            originalDpi = document.panelSettings.referenceDpi;
        }

        private IEnumerator Resize(float wantedPanelWidth)
        {
            document.panelSettings.referenceDpi = wantedPanelWidth * Screen.dpi / Screen.width;
            // one frame to rescale, one to lay out, one for the layout classes to land
            yield return null;
            yield return null;
            yield return null;
        }

        private VisualElement Root()
        {
            return document.rootVisualElement;
        }

        private IEnumerator Reveal(string screen)
        {
            Root().Q<VisualElement>(screen).RemoveFromClassList("is-hidden");
            yield return null;
            yield return null;
        }

        private IEnumerator Hide(string screen)
        {
            Root().Q<VisualElement>(screen).AddToClassList("is-hidden");
            yield return null;
        }

        private static float Rightmost(VisualElement root)
        {
            float rightmost = 0f;
            root.Query<VisualElement>().ForEach(element =>
            {
                if (element.worldBound.xMax > rightmost)
                {
                    rightmost = element.worldBound.xMax;
                }
            });

            return rightmost;
        }

        private static float Lowest(VisualElement root, out VisualElement deepest)
        {
            float lowest = 0f;
            VisualElement found = root;
            root.Query<VisualElement>().ForEach(element =>
            {
                if (element.worldBound.yMax > lowest)
                {
                    lowest = element.worldBound.yMax;
                    found = element;
                }
            });

            deepest = found;
            return lowest;
        }

        private static bool IsInsideAScrollView(VisualElement element)
        {
            for (VisualElement parent = element; parent != null; parent = parent.parent)
            {
                if (parent is ScrollView)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
