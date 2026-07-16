# Pong

## Overview

Pong is a polished, extensible Unity 6 take on the arcade classic. It pairs deterministic 2D gameplay with a responsive UI Toolkit front end and two complete, runtime-switchable visual identities.

## Gameplay

Control the left paddle against a computer opponent. The default match ends at five points, and the target score and game speed can be changed in Settings.

## Controls

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move | `W` / `S` or arrow keys | Left stick or D-pad |
| Pause | `P` or `Esc` | Start |
| Restart | `R` | Select |
| Back | `Esc` | B / East button |

## Features

- responsive paddle movement using 2D physics
- deterministic serves and angle-based paddle rebounds
- computer opponent with simple, readable tracking behavior
- scoring, pause, win, and restart states
- a responsive UI Toolkit main menu, game mode library, settings, credits, and pause flow
- complete Retro and Futuristic design systems with distinct typography, geometry, motion, audio, overlays, HUDs, and arena treatment
- theme-scoped paddle, ball, arena, and background cosmetics that cannot create visually inconsistent combinations
- runtime theme switching with persistent, independent cosmetic selections for each world
- persistent gameplay, audio, graphics, and accessibility preferences
- mouse, keyboard, and controller-ready menu navigation with explicit focus states
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
- `Assets/Scripts/` contains runtime gameplay, presentation, and modular UI code.
- `Assets/UI/` contains the shared UI Toolkit document, independent theme style sheets and assets, panel settings, and content catalogs.
- `Assets/Tests/` contains automated Edit Mode and Play Mode tests.
- `Assets/Art/` contains the small set of shared visual and physics assets.
- `Assets/Settings/`, `Packages/`, and `ProjectSettings/` contain Unity configuration.

## Contributing

Bug reports and focused pull requests are welcome. Use the required Unity version, test gameplay changes in Play Mode, include `.meta` files with new assets, and keep generated folders and credentials out of commits. Architecture and gameplay decisions are documented in [`docs/`](docs/).

## License

Released under the [MIT License](LICENSE).
