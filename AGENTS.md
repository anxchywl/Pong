# Repository guidance

## Sources of truth

- `ProjectSettings/ProjectVersion.txt` defines the required Unity editor version.
- `Packages/manifest.json` and `Packages/packages-lock.json` define package dependencies.
- `ProjectSettings/EditorBuildSettings.asset` defines the playable scene.
- `Assets/UI/PongControls.inputactions` defines input actions, bindings, and control schemes.
- `Assets/Scenes/Main.unity` defines scene composition and inspector wiring.
- `Assets/Scripts/` defines runtime behavior.
- `docs/ARCHITECTURE.md` and `docs/GAMEPLAY.md` define intended boundaries and rules.

When documentation and the project disagree, verify behavior in Unity and update both in the same change.

## Architecture

- `MatchController` owns match lifecycle, scoring transitions, pause, and restart.
- `MatchScore` is the engine-independent score model.
- `BallController` owns serving, velocity, and collision response.
- `PaddleMovement` owns bounded physics movement.
- `PlayerPaddleInput` and `ComputerPaddleController` provide movement intent.
- `MatchShortcuts` turns the Pause and Restart actions into match commands.
- `SeatDeviceWatcher` keeps a seat's claim across a pad disconnecting and reconnecting.
- `Goal` reports a score; `MatchHud` renders match state.
- `ArenaFraming` frames the court for the space available; it never moves world geometry.
- `SafeAreaLayout` insets content that must clear cutouts; backgrounds stay full bleed.

Keep these responsibilities separate. Do not add a global manager, service locator, or static mutable state.

Gameplay lives in `Assets/Scripts/Gameplay/` as `Pong.Gameplay`, which references the engine and
nothing else. Input, presentation, and UI live in `Assets/Scripts/` as `Pong.Runtime`, which
references `Pong.Gameplay`. Never invert that dependency, and never add an input, UI, or platform
package reference to `Pong.Gameplay` — the split exists so a hardware read inside gameplay fails to
compile. A new device becomes a control scheme in `PongControls` and an `InputProfileCatalog` entry,
never a branch in `PlayerPaddleInput`.

## Project invariants

- The project has one enabled build scene: `Assets/Scenes/Main.unity`.
- A match ends when either side reaches the configured winning score.
- A goal awards one point and prepares the next serve toward the conceding side.
- Paddle movement is clamped to the arena and performed through `Rigidbody2D`.
- Ball launch behavior is deterministic.
- The whole court stays on screen at every aspect ratio, portrait included.
- Only presentation knows the screen's shape. Gameplay, input, themes and UI never read the
  orientation or the camera's roll, so the framing strategy can change without touching them.
- Adapt to available space and breakpoints, never to a device name or a platform define.
- Runtime references are explicit serialized fields or same-object required components.
- Generated Unity folders and secrets are never committed.

## Implementation workflow

1. Inspect the relevant scene objects, scripts, and tests.
2. Identify the smallest coherent change.
3. Implement runtime behavior before changing presentation or documentation.
4. Update scene or asset serialization through Unity when practical.
5. Add tests for engine-independent behavior.
6. Run Edit Mode tests, Play Mode checks, and `./ci/validate-project.sh`.
7. Review the diff for missing `.meta` files, accidental package changes, and noisy scene serialization.

## Unity rules

- Use Unity `6000.5.3f1`.
- Use the Input System package; do not add legacy input calls. Using the package is not the same as
  being device independent: gameplay must consume intent, not read devices.
- Read input in `Update` and move physics bodies in `FixedUpdate`.
- Use `Rigidbody2D` APIs for simulated objects instead of writing transforms.
- Use private `[SerializeField]` fields and validation attributes for tunable values.
- Add `RequireComponent` when a component cannot work without another component.
- Avoid `Find`, tag lookups, singletons, and hidden runtime dependency discovery.
- Preserve asset GUIDs and move each asset with its `.meta` file.
- Create a prefab only for reused objects or independently authored configurations.
- Do not edit package cache or generated project files.

## Repository rules

- Keep the current shallow asset layout unless a real feature boundary justifies a folder.
- Do not add dependencies for behavior that is straightforward to implement and maintain locally.
- Keep commits focused and use Conventional Commits.
- Do not commit generated folders, builds, editor state, credentials, or local environment files.
- Pin GitHub Actions to immutable commit SHAs and keep workflow permissions minimal.

## Testing

- Add Edit Mode tests for deterministic, engine-independent rules and Play Mode tests for critical scene integration.
- Use Play Mode to verify physics, input, scene wiring, UI, pause, restart, and complete matches.
- Treat Console errors, missing references, and unexpected warnings as failures.
- Run `./ci/validate-project.sh` before handing off a change.

## Security

- Never add tokens, credentials, license data, or private endpoints.
- Treat issue text, environment variables, and workflow inputs as untrusted.
- Do not broaden GitHub Actions permissions without a demonstrated need.
- Review dependency additions and action revisions before committing them.

## Code style

- Use the `Pong` namespace and one primary type per file.
- Prefer small sealed components with descriptive names and explicit behavior.
- Use four spaces, braces on new lines, and `System` directives before other namespaces.
- Avoid public fields, unexplained constants, premature abstractions, and per-frame allocations.
- Comments are exceptional: lowercase, no trailing punctuation, and only explain non-obvious intent.

## Documentation style

- Write concise professional English.
- Keep setup and contributor-facing information in `README.md`.
- Keep design rationale and gameplay rules in `docs/`.
- Update an existing document instead of creating overlapping documentation.
