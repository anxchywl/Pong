# Pong

## Overview

Pong is a small Unity 6 project built as a readable reference for 2D gameplay, physics, input, UI, and testable game state. The scope stays intentionally narrow so each part can be understood in context.

## Gameplay

Control the left paddle against a computer opponent. The first side to score five points wins the match.

## Controls

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | `W` / `S` or arrow keys | Left stick or D-pad |
| Pause | `P` or `Esc` | Start |
| Restart | `R` | Select |

## Features

- responsive paddle movement using 2D physics
- deterministic serves and angle-based paddle rebounds
- computer opponent with simple, readable tracking behavior
- scoring, pause, win, and restart states
- Edit Mode score tests and a Play Mode scene smoke test

## Requirements

- Unity Hub
- Unity `6000.5.3f1`

## Getting started

1. Clone the repository.
2. Add the project folder in Unity Hub.
3. Open it with Unity `6000.5.3f1`.
4. Open `Assets/Scenes/Main.unity` and enter Play Mode.

## Project layout

- `Assets/Scenes/` contains the playable scene.
- `Assets/Scripts/` contains runtime gameplay and UI code.
- `Assets/Tests/` contains automated Edit Mode and Play Mode tests.
- `Assets/Art/` contains the small set of shared visual and physics assets.
- `Assets/Settings/`, `Packages/`, and `ProjectSettings/` contain Unity configuration.

## Contributing

Bug reports and focused pull requests are welcome. Use the required Unity version, test gameplay changes in Play Mode, include `.meta` files with new assets, and keep generated folders and credentials out of commits. Architecture and gameplay decisions are documented in [`docs/`](docs/).

## License

Released under the [MIT License](LICENSE).
