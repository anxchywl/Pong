# Architecture

## Design goals

The runtime separates game rules, movement, input, presentation, and UI without introducing a global framework. Gameplay components communicate through inspector references and narrow public methods. The UI observes immutable match snapshots and sends lifecycle commands, so it cannot decide scores or manipulate physics.

## Runtime responsibilities

| Type | Responsibility |
| --- | --- |
| `MatchController` | Owns match lifecycle, front-end suspension, pause, restart, speed, and score transitions. Offers them as commands and reads no input |
| `MatchScore` | Tracks points and determines the winner without Unity dependencies |
| `BallController` | Serves the ball and controls collision response and rally speed |
| `PaddleMovement` | Applies bounded kinematic movement from normalized intent, and derives its travel from the arena and its own measured height |
| `PlayerPaddleInput` | Turns one seat's Move action into movement intent, under the profile's control scheme |
| `MatchShortcuts` | Drives pause and restart from the Gameplay map, so the match reads no input |
| `ComputerPaddleController` | Produces simple movement intent from ball state |
| `CourtSeat` / `SeatAssignment` | Identify a seat and who occupies it, without Unity dependencies |
| `MatchRoster` | Owns the lineup rules: goalkeeper before attacker, one profile per paddle |
| `SeatDirector` | Pushes the roster onto the court's paddles and owns the paddle length rule |
| `SeatDeviceWatcher` | Pauses when a seated pad leaves and rebinds when it returns |
| `PaddleSeat` | One physical paddle: its seat, its renderers, and which intent drives it |
| `InputProfileCatalog` | Keyboard layouts and gamepad profiles as data, each naming a control scheme |
| `Goal` | Reports which side scored when the ball enters a trigger |
| `GameUiController` | Composes UI views, routes user intent, and observes match state |
| `ScreenNavigator` / `ScreenHost` | Maintain deterministic front-end navigation and visible screens |
| `MatchHud` / screen views | Render one focused part of the UI document |
| `ArenaFraming` | Frames the court for the space available; the only thing that knows the screen's shape |
| `SafeAreaLayout` | Insets content that must clear cutouts, leaving backgrounds full bleed |
| `GamePresentation` | Applies the active theme and scoped cosmetics to renderers, glow layers, particles, audio, and the camera |
| `GameTheme` / `ThemeCatalog` | Define independent visual identities and make them runtime-switchable |
| `GameModeCatalog` / `CosmeticCatalog` | Provide data-driven mode and theme-scoped cosmetic content |
| `GameSettings` / `PlayerPreferences` | Validate and persist user choices |
| `CosmeticSelection` | A copyable set of choices, so the workshop can preview a draft before it is saved |
| `CategoryRail` | One panel at a time, shared by settings and the workshop |

## State flow

The app opens in the front end with gameplay suspended. Starting a match gives the selected settings to `MatchController`, which publishes a `MatchState` snapshot. A goal trigger reports the scoring side, and the controller either prepares another serve or stops the ball after a win. Restart resets the same state without reloading the scene.

`GameUiController` subscribes to match state changes and delegates rendering to small views. `ScreenNavigator` owns front-end history; pause settings are presented over the paused match without changing game state. UI Toolkit provides one document for menus, HUD, pause, confirmation, and results, avoiding mixed UI systems. That document is laid out for desktop window sizes and scales uniformly from a 1920×1080 reference; it does not yet adapt its layout to phone or tablet dimensions.

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

## Assemblies

The runtime is split into two assemblies so the input boundary is enforced by the compiler rather
than by review.

| Assembly | Location | References |
| --- | --- | --- |
| `Pong.Gameplay` | `Assets/Scripts/Gameplay/` | nothing but the engine |
| `Pong.Runtime` | `Assets/Scripts/` | `Pong.Gameplay`, `Unity.InputSystem`, `Unity.ugui` |

`Pong.Gameplay` holds the match, the rules, the score, the lineup, and the physics bodies:
`MatchController`, `MatchScore`, `MatchState`, `Goal`, `PlayerSide`, `CourtSeat`, `SeatAssignment`,
`MatchRoster`, `InputProfileCatalog`, `PaddleMovement`, `BallController`, and
`ComputerPaddleController`. It cannot reference the Input System, so a hardware read inside it is a
compile error rather than a review comment. Gameplay consumes intent — `PaddleMovement.SetDirection`
is the whole surface — and never asks which device produced it.

`InputProfileCatalog` belongs here despite its name. A profile is an abstract input binding: an id, a
name, and the control scheme it answers to. The bindings themselves live in `PongControls`, so the
catalog needs no Input System reference, and `MatchRoster` can name a seat's driver without either of
them knowing what a keyboard is.

`Pong.Runtime` holds input, presentation, and UI, and depends on `Pong.Gameplay` in that direction
only. The dependency must never be inverted.

## Input

`Assets/UI/PongControls.inputactions` is the project's own Input Actions asset. It holds three maps:

- **UI** drives menu navigation and is bound by the scene's `InputSystemUIInputModule`.
- **Gameplay** describes `Move`, `Pause`, and `Restart` as intent, under the `KeyboardLeft`,
  `KeyboardRight`, and `Gamepad` schemes.
- **Debug** carries the overlay toggle, so even a debug shortcut is an action rather than a key.

Adding an `InputSystemUIInputModule` binds it to the Input System package's own default actions,
which live inside an immutable package: nothing in this project could add a binding or a scheme to
them. `GameUiSceneSetup` therefore repoints the module at this asset. The module resolves each of its
actions by map and action name and leaves a null behind when a name does not match, rather than
failing, so the UI map deliberately mirrors the package default's action names exactly. Renaming a UI
action there costs a kind of menu input silently; `UiInputBindingTests` exists to catch that.

The UI map still carries the default's Joystick and XR bindings. They are inherited rather than
intended, and can be pruned once there is a reason to touch them.

Nothing reads a device to play. A seat's `InputProfileDefinition` names a control scheme rather than
a key pair, and `PlayerPaddleInput` binds `Gameplay/Move` under that scheme. Each paddle gets its own
copy of the asset, which is what lets two seats read one keyboard without sharing enabled state: the
scheme tells them apart. Only a gamepad seat pins itself to a device, because one pad must not drive
two paddles.

`MatchShortcuts` drives pause and restart from the same map, deliberately unmasked and unpaired, so
either answers to whoever reaches them on a shared screen. It sits beside `MatchController` rather
than inside it, so the match offers commands and never learns a keyboard exists.

Back is the UI map's own `Cancel`, the same action the menus navigate with, so escape and the
gamepad's east button reach `GameUiController` without it naming either. `Pause` is deliberately not
bound to escape: escape already arrives as `Cancel`, and binding both would toggle pause twice and
cancel itself out.

The remaining `Gamepad.all` reads, in `PlayersView` and `SeatDescription`, enumerate devices so a
player can pick one and so a pad can be numbered by plug order. That is device discovery, not input.

## Adaptive presentation

The court is 19.2 by 9.4 world units and never changes. Neither do the rules, the input, the themes
or the UI hierarchy. What adapts is presentation, and it is the only layer that knows the shape of
the screen it is drawing onto.

This is a boundary, not a detail. Nothing outside presentation may ask whether the screen is portrait
or what the camera is doing, so the strategy below can be replaced without touching a game system.

`ArenaFraming` reads the available space each time it changes and asks `ArenaFrame` for a framing.
It reads space, not the device: a narrow desktop window is a narrow window, not a phone.

The current strategy is one implementation, not the architecture:

- In landscape it keeps the framing the game has always had — a 16:9 or ultrawide window needs less
  room than that, and the band it leaves above the wall is where the HUD lives — and pulls back only
  when a narrow window such as 4:3 would otherwise crop a goal. It used to clear the goals by a tenth
  of a unit at 4:3, which was luck rather than design.
- In portrait it rolls the camera a quarter turn, so the court's long axis runs down the screen's
  long axis and the paddles sit at the top and bottom. Nothing in the world moves and no rule
  changes: it is the same match seen sideways, and `PlayerSide.Left` is simply drawn at the bottom.
  A court laid along a phone is a court that suits a phone, rather than one zoomed out until a
  landscape layout happens to fit.

A future strategy could letterbox, shorten the court, or lay it out differently again. Gameplay,
input and the UI would not notice, because none of them read the roll: anything needing to relate the
screen to the court goes through the camera, which already carries whatever the strategy decided.

`ArenaFrame` is pure maths, so `ArenaFrameTests` checks every shape a screen comes in — ultrawide
through phone portrait — rather than every device.

## Safe areas

`SafeAreaLayout` keeps readable and interactive content clear of notches, rounded corners and gesture
areas. Elements opt in with the `safe-area` class, and they are inset with padding rather than moved,
so a background, a scrim or an overlay still reaches every edge. A screen should look like it runs
under the cutout; its buttons should not sit beneath one.

Inset: the six screens, the HUD, the pause, win and quit layers, and the debug overlay. Full bleed:
`game-ui`, `screen-layer`'s backdrop, and `theme-overlay`. `screen-layer` keeps its own design
padding and the safe inset lands on the screens inside it, so the two compose rather than fight.

`SafeArea` is pure maths in panel points, not pixels, because the panel is measured in points and a
cutout in pixels: a dense display would otherwise inset twice as far as it should. `SafeAreaTests`
covers a phone upright, the same phone turned sideways where the notch becomes a side inset, a
high-density display, and a screen that is not ready yet.

## Devices

A seat claims a profile and, for a gamepad, one device id. `SeatDeviceWatcher` watches pads arrive
and leave and keeps the court honest about it:

- **A claimed pad leaves.** The seat keeps its claim and the match pauses. The paddle would stop
  anyway once its device is gone, but a live ball would go on scoring against a player holding a
  dead pad, so a pulled cable costs a pause rather than the point or the seat.
- **It comes back.** The seat never let go, so the pad picks its paddle up again. Resuming is left
  to the player, who may not have both hands back yet.
- **A different pad arrives.** Nothing happens. It has a different device id, so it cannot inherit a
  seat by turning up; the player reseats it from the Players screen. This is also what a reconnect
  looks like when the platform does not give a pad its own id back.
- **An unclaimed pad leaves.** Nothing happens. Keyboard seats carry no device and never answer to
  one leaving.

A real pad carries a description, so the Input System keeps it in `disconnectedDevices` and restores
its id when it returns. A virtual device has none, which is why `SeatDeviceTests` models a reconnect
by re-adding the same instance: adding a fresh one would be a different pad entirely.

Reassignment needs no help from any of this. `MatchRoster` already gives one profile and device
exactly one paddle, so a later claim vacates the earlier seat.

The pause menu is what a player sees when a pad drops, because the match is simply paused. A prompt
naming the lost pad would need a string in both theme copy tables and is not written yet.

`GameplayInputTests` presses keys on a virtual keyboard and watches the court, so the path from
device to action to intent to physics is covered rather than assumed. It has to opt out of two
Input System defaults first: a test runner holds no focus and no game view, and by default device
state is wiped while unfocused and keys are routed away from an unfocused game view. Both defaults
are right for a real game and wrong for a runner, so the fixture overrides them and puts them back.

## Dependency boundaries

- `MatchScore` must remain independent of Unity APIs so its rules stay fast to test.
- `Pong.Gameplay` must not reference the Input System, UI Toolkit, uGUI, or any platform package.
- Gameplay components may expose read-only state needed by another component, but should not expose their internal physics objects.
- Runtime dependencies should be serialized references or required components on the same object.
- UI may observe match state but must not decide gameplay outcomes.
- Presentation may change colors, materials, overlays, glow children, particles, and sound but must not change gameplay tuning or collider geometry.
- Mode and cosmetic menus render catalog entries rather than hardcoded buttons.

## Extension guidance

Add modes and cosmetics to their catalog assets. Add complete visual identities through theme assets, a USS file and a `.tss` rather than conditional UI code. New screens should get a focused view class and a navigation entry rather than expanding `GameUiController` with rendering details. A new way to play should read `MatchRoster` rather than inventing its own idea of who is playing, and a new input device should become an `InputProfileCatalog` entry rather than another branch in `PlayerPaddleInput`.
