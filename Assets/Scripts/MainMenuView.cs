using System;
using UnityEngine.UIElements;

namespace Pong
{
    public sealed class MainMenuView
    {
        private readonly MatchRoster roster;
        private readonly InputProfileCatalog profiles;
        private readonly Label modeValue;
        private readonly Label themeValue;
        private readonly Label playerCount;
        private readonly VisualElement lineup;

        public MainMenuView(
            VisualElement root,
            MatchRoster roster,
            InputProfileCatalog profiles,
            Action play,
            Action<AppScreen> open,
            Action quit,
            Action clickFeedback
        )
        {
            this.roster = roster;
            this.profiles = profiles;
            modeValue = root.Q<Label>("selected-mode-value");
            themeValue = root.Q<Label>("selected-theme-value");
            playerCount = root.Q<Label>("match-card-count");
            lineup = root.Q<VisualElement>("menu-lineup");

            Bind(root.Q<Button>("play-button"), play, clickFeedback);
            Bind(root.Q<Button>("players-button"), () => open(AppScreen.Players), clickFeedback);
            Bind(root.Q<Button>("game-mode-button"), () => open(AppScreen.GameMode), clickFeedback);
            Bind(root.Q<Button>("customization-button"), () => open(AppScreen.Customization), clickFeedback);
            Bind(root.Q<Button>("settings-button"), () => open(AppScreen.Settings), clickFeedback);
            Bind(root.Q<Button>("credits-button"), () => open(AppScreen.Credits), clickFeedback);
            Bind(root.Q<Button>("quit-button"), quit, clickFeedback);

            RenderLineup();
        }

        public void SetSelectedMode(string modeName)
        {
            modeValue.text = modeName;
        }

        public void SetSelectedTheme(string themeName)
        {
            themeValue.text = themeName;
        }

        /// Shows every seat on the menu itself, vacant ones included: who is playing is visible
        /// without opening the players screen, and the empty seats are how a player discovers
        /// that a third and fourth can join at all.
        public void RenderLineup()
        {
            lineup.Clear();

            foreach (CourtSeat seat in CourtSeat.All)
            {
                SeatAssignment assignment = roster.Get(seat);

                VisualElement row = new VisualElement();
                row.AddToClassList("lineup-row");
                row.EnableInClassList("lineup-row--vacant", !assignment.IsOccupied);

                Label seatLabel = new Label(SeatLabel(seat));
                seatLabel.AddToClassList("lineup-row__seat");
                Label occupant = new Label(SeatDescription.Occupant(assignment, profiles));
                occupant.AddToClassList("lineup-row__occupant");

                row.Add(seatLabel);
                row.Add(occupant);
                lineup.Add(row);
            }

            int humans = roster.HumanCount;
            playerCount.text = humans == 1 ? "1 PLAYER" : $"{humans} PLAYERS";
        }

        private static string SeatLabel(CourtSeat seat)
        {
            string side = seat.Side == PlayerSide.Left ? "LEFT" : "RIGHT";
            return $"{side}  ·  {SeatDescription.Role(seat.Role)}";
        }

        private static void Bind(Button button, Action action, Action clickFeedback)
        {
            button.clicked += () =>
            {
                clickFeedback();
                action();
            };
        }
    }
}
