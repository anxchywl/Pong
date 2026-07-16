using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Pong
{
    public enum InputProfileKind
    {
        Keyboard,
        Gamepad
    }

    [CreateAssetMenu(menuName = "Pong/Input/Input Profile Catalog")]
    public sealed class InputProfileCatalog : ScriptableObject
    {
        public const string DefaultProfileId = "keyboard-wasd";

        [SerializeField] private List<InputProfileDefinition> profiles = new List<InputProfileDefinition>();

        public IReadOnlyList<InputProfileDefinition> Profiles => profiles;

        public InputProfileDefinition Find(string id)
        {
            return profiles.Find(profile => profile.Id == id);
        }

        public InputProfileDefinition Default => Find(DefaultProfileId) ?? (profiles.Count == 0 ? null : profiles[0]);
    }

    /// A way to drive one paddle. Keyboard profiles carry their own key pair so several players can
    /// share one keyboard; gamepad profiles are bound to a device at assignment time.
    [Serializable]
    public sealed class InputProfileDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string hint;
        [SerializeField] private InputProfileKind kind;
        [SerializeField] private Key moveUpKey = Key.W;
        [SerializeField] private Key moveDownKey = Key.S;

        public string Id => id;
        public string DisplayName => displayName;
        public string Hint => hint;
        public InputProfileKind Kind => kind;
        public Key MoveUpKey => moveUpKey;
        public Key MoveDownKey => moveDownKey;

        /// Gamepad profiles need a device to be meaningful; keyboard profiles never do.
        public bool RequiresDevice => kind == InputProfileKind.Gamepad;
    }
}
