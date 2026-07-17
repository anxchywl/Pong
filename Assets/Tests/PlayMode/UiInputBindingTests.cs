using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Pong.Tests
{
    /// The UI module resolves its actions by name and leaves a null behind when one does not match,
    /// so a renamed or missing action costs a kind of menu input without failing anything. These
    /// assert the scene binds every one of them to the project's own asset.
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

        /// Gameplay has its own map from here on. Nothing consumes it yet, so this only asserts it
        /// exists and is addressed by the names the paddles and match will bind to.
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
