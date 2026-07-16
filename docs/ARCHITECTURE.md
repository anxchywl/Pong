# Architecture

## Design goals

The runtime separates game rules, movement, input, presentation, and UI without introducing a global framework. Gameplay components communicate through inspector references and narrow public methods. The UI observes immutable match snapshots and sends lifecycle commands, so it cannot decide scores or manipulate physics.

## Runtime responsibilities

| Type | Responsibility |
| --- | --- |
| `MatchController` | Owns match lifecycle, front-end suspension, pause, restart, speed, and score transitions |
| `MatchScore` | Tracks points and determines the winner without Unity dependencies |
| `BallController` | Serves the ball and controls collision response and rally speed |
| `PaddleMovement` | Applies bounded kinematic movement from normalized intent |
| `PlayerPaddleInput` | Converts keyboard or gamepad state into movement intent |
| `ComputerPaddleController` | Produces simple movement intent from ball state |
| `Goal` | Reports which side scored when the ball enters a trigger |
| `GameUiController` | Composes UI views, routes user intent, and observes match state |
| `ScreenNavigator` / `ScreenHost` | Maintain deterministic front-end navigation and visible screens |
| `MatchHud` / screen views | Render one focused part of the UI document |
| `GamePresentation` | Applies the active theme and scoped cosmetics to renderers, glow layers, particles, audio, and the camera |
| `GameTheme` / `ThemeCatalog` | Define independent visual identities and make them runtime-switchable |
| `GameModeCatalog` / `CosmeticCatalog` | Provide data-driven mode and theme-scoped cosmetic content |
| `GameSettings` / `PlayerPreferences` | Validate and persist user choices |

## State flow

The app opens in the front end with gameplay suspended. Starting a match gives the selected settings to `MatchController`, which publishes a `MatchState` snapshot. A goal trigger reports the scoring side, and the controller either prepares another serve or stops the ball after a win. Restart resets the same state without reloading the scene.

`GameUiController` subscribes to match state changes and delegates rendering to small views. `ScreenNavigator` owns front-end history; pause settings are presented over the paused match without changing game state. UI Toolkit provides one responsive document for menus, HUD, pause, confirmation, and results, avoiding mixed UI systems.

## Theming

Each `GameTheme` asset owns identity copy, its theme style sheet, font fallbacks, icon language, base palette, overlay pattern, glow, transition timing, particle behavior, and synthesized feedback sound. `UiThemePresenter` swaps the style sheet and rebuilds only the shared document's presentation layer. `GamePresentation` applies the same asset to the arena without changing colliders, transforms, rules, or physics.

Retro and Futuristic use the same UXML and view code but different design assets. Retro uses cabinet geometry, monospaced typography, square icon language, scanlines, hard transitions, and square-wave feedback. Futuristic uses layered rounded surfaces, proportional typography, line icons, ambient circuit structure, soft glow, smoother motion, and harmonic sine feedback.

Cosmetics include a `themeId`. Selection keys are namespaced by theme and category, so switching worlds restores that world's paddle, ball, arena, and background selections. The UI never displays cosmetics from another world.

To add a theme, create a new `GameTheme` asset, a theme USS file that overrides the shared component classes, and cosmetic catalog entries with the same theme ID. Add the theme asset to `ThemeCatalog`; no gameplay or screen code changes are required.

Input components do not move transforms directly. They pass a normalized direction to `PaddleMovement`, which moves its kinematic body during the physics step. The computer controller uses the same movement component as the player, so bounds and speed behavior stay consistent.

## Dependency boundaries

- `MatchScore` must remain independent of Unity APIs so its rules stay fast to test.
- Gameplay components may expose read-only state needed by another component, but should not expose their internal physics objects.
- Runtime dependencies should be serialized references or required components on the same object.
- UI may observe match state but must not decide gameplay outcomes.
- Presentation may change colors, materials, overlays, glow children, particles, and sound but must not change gameplay tuning or collider geometry.
- Mode and cosmetic menus render catalog entries rather than hardcoded buttons.

## Extension guidance

Add modes and cosmetics to their catalog assets. Add complete visual identities through theme assets and style sheets rather than conditional UI code. New screens should get a focused view class and a navigation entry rather than expanding `GameUiController` with rendering details. A second human player should provide another input component to `PaddleMovement`; it should not duplicate movement physics.
