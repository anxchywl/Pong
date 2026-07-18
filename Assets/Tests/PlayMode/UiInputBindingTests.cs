using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Pong.Tests
{
    /// A renamed or missing UI action resolves to null rather than failing, so menu input can stop
    /// working in silence. These pin the bindings down.
    public sealed class UiInputBindingTests
    {
        private static readonly string[] ExpectedActions =
        {
            "Point", "Navigate", "Click", "RightClick", "MiddleClick",
            "ScrollWheel", "Submit", "Cancel", "TrackedDevicePosition", "TrackedDeviceOrientation"
        };

        [UnityTest]
        public IEnumerator EventSystem_DrivesTheUiFromTheProjectsOwnActions()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            InputSystemUIInputModule module = Object.FindAnyObjectByType<InputSystemUIInputModule>();

            Assert.That(module, Is.Not.Null, "the scene has no UI input module");
            Assert.That(module.actionsAsset, Is.Not.Null, "the UI input module has no actions asset");
            Assert.That(
                module.actionsAsset.name,
                Is.EqualTo("PongControls"),
                "the UI module must use the project's asset, not the Input System package's default"
            );
        }

        [UnityTest]
        public IEnumerator EventSystem_BindsEveryUiAction()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            InputSystemUIInputModule module = Object.FindAnyObjectByType<InputSystemUIInputModule>();
            InputActionReference[] references =
            {
                module.point, module.move, module.leftClick, module.rightClick, module.middleClick,
                module.scrollWheel, module.submit, module.cancel,
                module.trackedDevicePosition, module.trackedDeviceOrientation
            };

            foreach (InputActionReference reference in references)
            {
                Assert.That(reference, Is.Not.Null, "a UI action reference is unassigned");
                Assert.That(reference.action, Is.Not.Null, $"{reference.name} resolves to no action");
                Assert.That(reference.action.actionMap.name, Is.EqualTo("UI"));
            }
        }

        [UnityTest]
        public IEnumerator Controls_KeepTheActionNamesTheUiModuleBindsBy()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            InputSystemUIInputModule module = Object.FindAnyObjectByType<InputSystemUIInputModule>();
            InputActionMap ui = module.actionsAsset.FindActionMap("UI");

            Assert.That(ui, Is.Not.Null, "the asset has no UI map");
            foreach (string action in ExpectedActions)
            {
                Assert.That(ui.FindAction(action), Is.Not.Null, $"the UI map has no {action} action");
            }
        }

        /// Nothing consumes the Gameplay map yet, so this only pins the names the paddles and the
        /// match will bind to.
        [UnityTest]
        public IEnumerator Controls_DescribeGameplayAsActions()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            yield return null;

            InputSystemUIInputModule module = Object.FindAnyObjectByType<InputSystemUIInputModule>();
            InputActionMap gameplay = module.actionsAsset.FindActionMap("Gameplay");

            Assert.That(gameplay, Is.Not.Null, "the asset has no Gameplay map");
            Assert.That(gameplay.FindAction("Move"), Is.Not.Null);
            Assert.That(gameplay.FindAction("Pause"), Is.Not.Null);
            Assert.That(gameplay.FindAction("Restart"), Is.Not.Null);
        }
    }
}
