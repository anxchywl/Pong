# Gameplay

## Match rules

- The player controls the left paddle and the computer controls the right paddle.
- A side scores when the ball crosses the opponent's goal line.
- The first side to five points wins.
- After a point, the ball returns to center and serves toward the side that conceded.
- Restart clears the score and begins a new match without reloading the scene.
- Opening the front end or returning to the main menu suspends simulation.
- Pause preserves the current serve or rally and restores the configured game speed on resume.

## Ball behavior

Serves use a fixed angle and alternate vertically, which keeps openings varied but reproducible. Paddle contact maps the vertical hit position to the outgoing angle: center hits stay flatter and edge hits become steeper. Rally speed increases slightly on each paddle hit up to a fixed maximum. Wall bounces preserve the current rally speed.

## Computer opponent

The computer follows the ball only while it travels toward the right side. While the ball travels away, the paddle returns toward center. A small dead zone prevents jitter near the target position.

## Tuning

Gameplay values are serialized on scene components:

- serve delay, launch speed, angles, and rally acceleration on `BallController`
- speed and vertical limits on `PaddleMovement`
- tracking dead zone on `ComputerPaddleController`
- default winning score and opening serve on `MatchController`

The player-facing points target and game speed are stored in Settings and applied when a match starts. Settings are clamped to supported ranges before use.

## Interface flow

The main menu is the entry point. Classic mode is playable; Practice, AI League, and Local Multiplayer remain visible as intentionally disabled future modes. The customization library switches complete Retro and Futuristic worlds at runtime and persists separate cosmetics for each. The in-game HUD only shows mode, score, player identities, serve status, and pause access. Pause and results use theme-specific overlays so the active match is never reloaded accidentally.

Theme switching changes presentation only. Paddle and ball colliders, movement speeds, ball velocity, scoring, serve direction, pause state, and match progress remain identical.

Change tuning in small increments and verify paddle bounds, shallow and steep rebounds, several consecutive rallies, scoring on both sides, pause, and restart.
