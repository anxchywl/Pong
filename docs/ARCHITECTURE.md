# Architecture

## Design goals

The runtime separates game rules, movement, input, and presentation without introducing a framework. Components communicate through inspector references and narrow public methods, making scene wiring visible and keeping each class readable in isolation.

## Runtime responsibilities

| Type | Responsibility |
| --- | --- |
| `MatchController` | Owns match lifecycle, pause, restart, and score transitions |
| `MatchScore` | Tracks points and determines the winner without Unity dependencies |
| `BallController` | Serves the ball and controls collision response and rally speed |
| `PaddleMovement` | Applies bounded kinematic movement from normalized intent |
| `PlayerPaddleInput` | Converts keyboard or gamepad state into movement intent |
| `ComputerPaddleController` | Produces simple movement intent from ball state |
| `Goal` | Reports which side scored when the ball enters a trigger |
| `MatchHud` | Displays score and match status |

## State flow

The match controller starts a serve. A goal trigger reports the scoring side to the match controller, which updates `MatchScore`, refreshes the HUD, and either prepares another serve or stops the ball after a win. Restart resets the same state without reloading the scene.

Input components do not move transforms directly. They pass a normalized direction to `PaddleMovement`, which moves its kinematic body during the physics step. The computer controller uses the same movement component as the player, so bounds and speed behavior stay consistent.

## Dependency boundaries

- `MatchScore` must remain independent of Unity APIs so its rules stay fast to test.
- Gameplay components may expose read-only state needed by another component, but should not expose their internal physics objects.
- Runtime dependencies should be serialized references or required components on the same object.
- UI may observe match state but must not decide gameplay outcomes.

## Extension guidance

Prefer extending an existing responsibility before adding a new subsystem. Audio can subscribe to explicit gameplay events when sounds are added. A second human player should provide another input component to `PaddleMovement`; it should not duplicate movement physics. Config assets are justified only when multiple scenes or modes need to share the same tuning.
