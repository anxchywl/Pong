using System;
using System.Collections.Generic;
using UnityEngine;

namespace Pong
{
    [CreateAssetMenu(menuName = "Pong/UI/Game Mode Catalog")]
    public sealed class GameModeCatalog : ScriptableObject
    {
        [SerializeField] private List<GameModeDefinition> modes = new List<GameModeDefinition>();

        public IReadOnlyList<GameModeDefinition> Modes => modes;

        public GameModeDefinition Find(string id)
        {
            return modes.Find(mode => mode.Id == id);
        }
    }

    [Serializable]
    public sealed class GameModeDefinition
    {
        [SerializeField] private string id;
        [SerializeField] private string displayName;
        [SerializeField, TextArea] private string description;
        [SerializeField] private string playerSummary;
        [SerializeField] private bool available;

        public string Id => id;
        public string DisplayName => displayName;
        public string Description => description;
        public string PlayerSummary => playerSummary;
        public bool Available => available;
    }
}
