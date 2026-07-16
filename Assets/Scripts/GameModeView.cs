using System;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class GameModeView
    {
        private readonly GameModeCatalog catalog;
        private readonly PlayerPreferences preferences;
        private readonly VisualElement cardContainer;
        private readonly Label selectionSummary;
        private readonly Action selectionChanged;
        private readonly Action clickFeedback;

        public GameModeView(
            VisualElement root,
            GameModeCatalog catalog,
            PlayerPreferences preferences,
            Action selectionChanged,
            Action clickFeedback
        )
        {
            this.catalog = catalog;
            this.preferences = preferences;
            this.selectionChanged = selectionChanged;
            this.clickFeedback = clickFeedback;
            cardContainer = root.Q<VisualElement>("mode-card-list");
            selectionSummary = root.Q<Label>("mode-selection-summary");
            Render();
        }

        public void Render()
        {
            cardContainer.Clear();

            foreach (GameModeDefinition mode in catalog.Modes)
            {
                Button card = new Button(() => Select(mode));
                card.AddToClassList("selection-card");
                card.EnableInClassList("is-selected", mode.Id == preferences.SelectedModeId);
                card.SetEnabled(mode.Available);

                Label availability = new Label(mode.Available ? mode.PlayerSummary : "COMING SOON");
                availability.AddToClassList("selection-card__eyebrow");
                Label title = new Label(mode.DisplayName);
                title.AddToClassList("selection-card__title");
                Label description = new Label(mode.Description);
                description.AddToClassList("selection-card__description");

                card.Add(availability);
                card.Add(title);
                card.Add(description);
                cardContainer.Add(card);
            }

            GameModeDefinition selected = catalog.Find(preferences.SelectedModeId);
            selectionSummary.text = selected == null ? "Choose a mode" : $"Selected: {selected.DisplayName}";
        }

        private void Select(GameModeDefinition mode)
        {
            if (!mode.Available)
            {
                return;
            }

            clickFeedback();
            preferences.SelectMode(mode.Id);
            Render();
            selectionChanged();
        }
    }
}
