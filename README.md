# Pong

## Overview

Pong is a polished, extensible Unity 6 take on the arcade classic. It pairs deterministic 2D gameplay with a desktop-first UI Toolkit front end and two complete, runtime-switchable visual identities.

## Gameplay

Take a seat and play. The court has four seats — a goalkeeper and an attacker on each side — and
every one can hold a human, a computer, or nobody, so one to four players share a screen without
switching modes. Open **Players** to seat them: each picks a keyboard layout or a gamepad of their
own. The default match ends at five points; the target score and game speed can be changed in
Settings.

## Controls

Each seat is driven by the input profile it was given, so two players can share one keyboard.

| Action | Keyboard | Gamepad |
| --- | --- | --- |
| Move (Keyboard W/S) | `W` / `S` | Left stick or D-pad |
| Move (Keyboard Arrows) | `Up` / `Down` | Left stick or D-pad |
| Pause | `P` or `Esc` | Start |
| Restart | `R` | Select |
| Back | `Esc` | B / East button |

## Features

- one to four local players across four court seats, each on its own keyboard layout or gamepad
- staggered attacker and goalkeeper columns, so a pair defends in depth rather than sharing a lane
- responsive paddle movement using 2D physics, with travel derived from the arena rather than tuned
- deterministic serves and angle-based paddle rebounds
- computer opponent with simple, readable tracking behavior
- scoring, pause, win, and restart states, with remaining match progress always on the HUD
- a UI Toolkit main menu, players screen, game mode library, categorised settings, credits, and pause flow, laid out for desktop window sizes
- a customization workshop that previews every change live and only saves when you apply it
- complete Retro and Futuristic design systems, each a Theme Style Sheet with its own typography, geometry, motion, audio, overlays, HUD and arena treatment
- theme-scoped cosmetics across arena, paddle, ball, HUD, effects, and audio that cannot create visually inconsistent combinations
- runtime theme switching with persistent, independent cosmetic selections for each world
- persistent gameplay, audio, graphics, and accessibility preferences
- mouse, keyboard, and controller-ready menu navigation with explicit focus states
- Edit Mode tests for scoring, the lineup rules, the selection model, and theme parity, plus a Play Mode scene smoke test

## Platform support

The project targets desktop today. Nothing below is claimed without a check that was actually run.

| Platform | State |
| --- | --- |
| macOS | compiles and runs in the editor; Edit Mode and Play Mode tests pass. No player build has been produced |
| Windows, Linux | source compatible. No build has been attempted |
| Android, iOS, iPadOS | source compatible only. Gameplay reads keyboard and gamepad devices directly, so a touch device has no way to move a paddle. Layouts carry fixed desktop dimensions and nothing reads `Screen.safeArea` |

The UI is laid out for desktop window sizes and scales uniformly from a 1920×1080 reference. It is
not adaptive to phone or tablet dimensions, and the arena's camera needs an aspect ratio of at least
1.32:1 to keep both goals on screen, so portrait orientations are not currently playable.

Cross-platform support is an incremental migration in progress. Do not describe a platform as
supported here without the build, runtime, and device checks behind it.

## Requirements

- Unity Hub
- Unity `6000.5.3f1`

## Getting started

1. Clone the repository.
2. Add the project folder in Unity Hub.
3. Open it with Unity `6000.5.3f1`.
4. Open `Assets/Scenes/Main.unity` and enter Play Mode.

## Regenerating the scene and content

`Pong > Setup Game UI` authors the parts of the project that are generated rather than hand-edited:
the court's four paddle seats, the seat director, the UI object, and the panel, theme, input
profile, game mode and cosmetic assets. It is deterministic — running it twice gives the same
result — and it must run in Edit Mode, not Play Mode.

Run it after changing anything the editor script owns: a serialized field on `GameTheme`, a copy
table entry, a cosmetic, a paddle constant. Without it those changes exist in code but not in the
assets, and `GameUiController` will tell you plainly if the scene predates the court seats rather
than failing as a null.

## Project layout

- `Assets/Scenes/` contains the playable scene.
- `Assets/Scripts/` contains presentation, input, and modular UI code as `Pong.Runtime`.
- `Assets/Scripts/Gameplay/` contains rules, physics, and scoring as `Pong.Gameplay`, an assembly that references no input, UI, or platform package.
- `Assets/UI/` contains the shared UI Toolkit document, independent theme style sheets and assets, panel settings, and content catalogs.
- `Assets/Tests/` contains automated Edit Mode and Play Mode tests.
- `Assets/Art/` contains the small set of shared visual and physics assets.
- `Assets/Settings/`, `Packages/`, and `ProjectSettings/` contain Unity configuration.

## Contributing

Bug reports and focused pull requests are welcome. Use the required Unity version, test gameplay changes in Play Mode, include `.meta` files with new assets, and keep generated folders and credentials out of commits. Architecture and gameplay decisions are documented in [`docs/`](docs/).

## License

Released under the [MIT License](LICENSE).
