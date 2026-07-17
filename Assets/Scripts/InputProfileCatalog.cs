using System;
using System.Collections.Generic;
using UnityEngine;

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

    /// A way to drive one paddle. The profile names a control scheme in PongControls rather than
    /// keys, so two players can share one keyboard without either knowing which keys the other has.
    [Serializable]
    public sealed class InputProfileDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField] private string hint;
        [SerializeField] private InputProfileKind kind;
        [SerializeField] private string controlScheme;

        public string Id => id;
        public string DisplayName => displayName;
        public string Hint => hint;
        public InputProfileKind Kind => kind;

        /// Names a control scheme in PongControls. The bindings live there, not here.
        public string ControlScheme => controlScheme;

        /// Gamepad profiles need a device to be meaningful; keyboard profiles never do.
        public bool RequiresDevice => kind == InputProfileKind.Gamepad;
    }
}
