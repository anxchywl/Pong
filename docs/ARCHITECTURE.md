# Architecture

## Design goals

The runtime separates game rules, movement, input, presentation, and UI without introducing a global framework. Gameplay components communicate through inspector references and narrow public methods. The UI observes immutable match snapshots and sends lifecycle commands, so it cannot decide scores or manipulate physics.

## Runtime responsibilities

| Type | Responsibility |
| --- | --- |
| `MatchController` | Owns match lifecycle, front-end suspension, pause, restart, speed, and score transitions |
| `MatchScore` | Tracks points and determines the winner without Unity dependencies |
| `BallController` | Serves the ball and controls collision response and rally speed |
| `PaddleMovement` | Applies bounded kinematic movement from normalized intent, and derives its travel from the arena and its own measured height |
| `PlayerPaddleInput` | Converts one input profile's keys or gamepad into movement intent |
| `ComputerPaddleController` | Produces simple movement intent from ball state |
| `CourtSeat` / `SeatAssignment` | Identify a seat and who occupies it, without Unity dependencies |
| `MatchRoster` | Owns the lineup rules: goalkeeper before attacker, one profile per paddle |
| `SeatDirector` | Pushes the roster onto the court's paddles and owns the paddle length rule |
| `PaddleSeat` | One physical paddle: its seat, its renderers, and which intent drives it |
| `InputProfileCatalog` | Keyboard layouts and gamepad profiles as data |
| `Goal` | Reports which side scored when the ball enters a trigger |
| `GameUiController` | Composes UI views, routes user intent, and observes match state |
| `ScreenNavigator` / `ScreenHost` | Maintain deterministic front-end navigation and visible screens |
| `MatchHud` / screen views | Render one focused part of the UI document |
| `GamePresentation` | Applies the active theme and scoped cosmetics to renderers, glow layers, particles, audio, and the camera |
| `GameTheme` / `ThemeCatalog` | Define independent visual identities and make them runtime-switchable |
| `GameModeCatalog` / `CosmeticCatalog` | Provide data-driven mode and theme-scoped cosmetic content |
| `GameSettings` / `PlayerPreferences` | Validate and persist user choices |
| `CosmeticSelection` | A copyable set of choices, so the workshop can preview a draft before it is saved |
| `CategoryRail` | One panel at a time, shared by settings and the workshop |

## State flow

The app opens in the front end with gameplay suspended. Starting a match gives the selected settings to `MatchController`, which publishes a `MatchState` snapshot. A goal trigger reports the scoring side, and the controller either prepares another serve or stops the ball after a win. Restart resets the same state without reloading the scene.

`GameUiController` subscribes to match state changes and delegates rendering to small views. `ScreenNavigator` owns front-end history; pause settings are presented over the paused match without changing game state. UI Toolkit provides one responsive document for menus, HUD, pause, confirmation, and results, avoiding mixed UI systems.

## Theming

A theme is a complete design system, not a palette. Each `GameTheme` asset owns its theme style
sheet, font fallbacks, copy table, base palette, overlay pattern, glow, transition timing, particle
behaviour, and synthesised feedback sound.

Every fixed string lives in the theme's copy table, addressed by the UXML name or class it belongs
to. No screen names an element in C#, so a new menu entry is markup plus a copy entry, and a theme
can rewrite the interface's entire voice without touching code. `ThemeCatalogTests` asserts both
themes address the same set of elements, so neither can leave a string wearing the other's voice.

Each theme owns a Theme Style Sheet in `Assets/UI/Themes`. It imports the default runtime controls,
the shared `GameUi.uss`, and then the theme's own sheet. Assigning it to `PanelSettings` swaps the
whole interface at once. The order is load-bearing: a document `<Style>` element beats a panel theme,
so `GameUi.uxml` deliberately has none — the shared sheet must arrive through the TSS, before the
theme that overrides it.

Retro and Futuristic share the UXML and the view code and nothing else. Retro is cabinet geometry,
monospaced type, bracketed icons, scanlines, hard transitions, square-wave feedback, and a bolted
score plate. Futuristic is layered rounded surfaces, proportional type, line icons, a faint circuit
structure, soft glow, smoother motion, harmonic sine feedback, and a score on glass.

Cosmetics carry a `themeId` and selection keys are namespaced by theme and category, so switching
worlds restores that world's parts. Randomize is confined to the current theme, so a shuffle can
never assemble a look that crosses two design systems.

To add a theme: create a `GameTheme` asset, a USS file that overrides the shared component classes,
a `.tss` that imports the default theme, `GameUi.uss` and that USS, and cosmetic entries with the
same theme ID. Add the asset to `ThemeCatalog`. No gameplay or screen code changes are required.

## Players

The court has four seats: left and right, each with a goalkeeper and an attacker. Every seat is
independently a human with an input profile and device, a computer, or empty. Player count is a
consequence of who is seated rather than a mode.

`MatchRoster` owns the rules and is engine-independent, so they are cheap to test: a side's
occupancy always settles into the goalkeeper seat first, because an attacker guarding an empty goal
is not a state the game should be able to reach; and one profile and device drives exactly one
paddle, so a later claim vacates the earlier seat rather than moving two paddles in unison.

Keyboard layouts are catalog data rather than hardcoded keys, so several players share one keyboard.
A future mode reads the same roster instead of inventing its own idea of who is playing.

## Dependency boundaries

- `MatchScore` must remain independent of Unity APIs so its rules stay fast to test.
- Gameplay components may expose read-only state needed by another component, but should not expose their internal physics objects.
- Runtime dependencies should be serialized references or required components on the same object.
- UI may observe match state but must not decide gameplay outcomes.
- Presentation may change colors, materials, overlays, glow children, particles, and sound but must not change gameplay tuning or collider geometry.
- Mode and cosmetic menus render catalog entries rather than hardcoded buttons.

## Extension guidance

Add modes and cosmetics to their catalog assets. Add complete visual identities through theme assets, a USS file and a `.tss` rather than conditional UI code. New screens should get a focused view class and a navigation entry rather than expanding `GameUiController` with rendering details. A new way to play should read `MatchRoster` rather than inventing its own idea of who is playing, and a new input device should become an `InputProfileCatalog` entry rather than another branch in `PlayerPaddleInput`.
