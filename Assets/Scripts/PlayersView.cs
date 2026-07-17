using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Pong
{
    /// The lineup screen. Laid out like the court itself: left side on the left, right on the
    /// right, goalkeeper above attacker, so a seat's position is its paddle's position.
    public sealed class PlayersView : IDisposable
    {
        private readonly MatchRoster roster;
        private readonly InputProfileCatalog profiles;
        private readonly VisualElement leftSeats;
        private readonly VisualElement rightSeats;
        private readonly Label summary;
        private readonly Action clickFeedback;

        public PlayersView(
            VisualElement root,
            MatchRoster roster,
            InputProfileCatalog profiles,
            Action clickFeedback
        )
        {
            this.roster = roster;
            this.profiles = profiles;
            this.clickFeedback = clickFeedback;
            leftSeats = root.Q<VisualElement>("left-seats");
            rightSeats = root.Q<VisualElement>("right-seats");
            summary = root.Q<Label>("players-summary");

            // the cards name the pads on this machine, so one arriving or leaving dates them
            InputSystem.onDeviceChange += HandleDeviceChange;
            Render();
        }

        public void Dispose()
        {
            InputSystem.onDeviceChange -= HandleDeviceChange;
        }

        private void HandleDeviceChange(InputDevice device, InputDeviceChange change)
        {
            if (device is Gamepad)
            {
                Render();
            }
        }

        public void Render()
        {
            leftSeats.Clear();
            rightSeats.Clear();

            foreach (CourtSeat seat in CourtSeat.All)
            {
                VisualElement column = seat.Side == PlayerSide.Left ? leftSeats : rightSeats;
                column.Add(BuildCard(seat));
            }

            int humans = roster.HumanCount;
            summary.text = humans == 1 ? "1 PLAYER" : $"{humans} PLAYERS";
        }

        private Button BuildCard(CourtSeat seat)
        {
            SeatAssignment assignment = roster.Get(seat);
            Button card = new Button(() => Advance(seat));
            card.AddToClassList("seat-card");
            card.AddToClassList("panel");
            card.EnableInClassList("seat-card--vacant", !assignment.IsOccupied);

            Label role = new Label(SeatDescription.Role(seat.Role));
            role.AddToClassList("seat-card__role");
            Label occupant = new Label(SeatDescription.Occupant(assignment, profiles));
            occupant.AddToClassList("seat-card__occupant");
            Label hint = new Label(SeatDescription.Hint(assignment, profiles));
            hint.AddToClassList("seat-card__hint");

            card.Add(role);
            card.Add(occupant);
            card.Add(hint);
            return card;
        }

        private void Advance(CourtSeat seat)
        {
            clickFeedback();
            IReadOnlyList<SeatAssignment> options = BuildOptions(seat);
            SeatAssignment current = roster.Get(seat);

            int index = 0;
            for (int candidate = 0; candidate < options.Count; candidate++)
            {
                if (options[candidate].Equals(current))
                {
                    index = candidate + 1;
                    break;
                }
            }

            roster.Assign(seat, options[index % options.Count]);
        }

        /// Empty, then every input this machine can currently offer, then the computer.
        /// Profiles held by another seat are left out rather than silently stolen.
        private IReadOnlyList<SeatAssignment> BuildOptions(CourtSeat seat)
        {
            List<SeatAssignment> options = new List<SeatAssignment> { SeatAssignment.Empty };

            foreach (InputProfileDefinition profile in profiles.Profiles)
            {
                if (!profile.RequiresDevice)
                {
                    AddIfFree(options, SeatAssignment.Human(profile.Id, SeatAssignment.NoDevice), seat);
                    continue;
                }

                foreach (Gamepad gamepad in Gamepad.all)
                {
                    AddIfFree(options, SeatAssignment.Human(profile.Id, gamepad.deviceId), seat);
                }
            }

            options.Add(SeatAssignment.Computer);
            return options;
        }

        private void AddIfFree(List<SeatAssignment> options, SeatAssignment candidate, CourtSeat seat)
        {
            bool taken = roster.TryFindClaim(candidate.ProfileId, candidate.DeviceId, out CourtSeat holder) &&
                !holder.Equals(seat);
            if (!taken)
            {
                options.Add(candidate);
            }
        }
    }
}
