# Gameplay

## Match rules

- Each side of the court has two seats: a goalkeeper guarding the goal and an attacker standing
  ahead of it. Any seat may hold a human, a computer, or nobody. One human against one computer
  goalkeeper is the default, and plays exactly as it always has.
- A side scores when the ball crosses the opponent's goal line.
- The first side to five points wins.
- After a point, the ball returns to center and serves toward the side that conceded.
- Restart clears the score and begins a new match without reloading the scene.
- Opening the front end or returning to the main menu suspends simulation.
- Pause preserves the current serve or rally and restores the configured game speed on resume.

## Ball behavior

Serves use a fixed angle and alternate vertically, which keeps openings varied but reproducible. Paddle contact maps the vertical hit position to the outgoing angle: center hits stay flatter and edge hits become steeper. Rally speed increases slightly on each paddle hit up to a fixed maximum. Wall bounces preserve the current rally speed.

## The court

Goalkeepers sit at the goal line's column; attackers stand ahead of them with the centre left open.
The columns are staggered rather than shared, so a pair defends in depth: the attacker meets the ball
early, and the goalkeeper is the last line. Nothing about the ball changes — its bounce direction has
always been derived from which side of a paddle it struck, so paddles at different columns need no
special handling.

A side defended by two paddles gets shorter ones, because two paddles covering a goal at full length
would make a duo close to impassable. The ratio is serialized on `SeatDirector` rather than fixed in
code, and wants playtesting.

A paddle is about a sixth of the arena's height. Its travel is not a tuned constant: it is measured
from the wall's inner face and the paddle's own height, so a paddle of any length stops exactly at
the wall and can never leave the arena.

## Computer opponent

The computer follows the ball only while it travels toward the right side. While the ball travels away, the paddle returns toward center. A small dead zone prevents jitter near the target position.

## Tuning

Gameplay values are serialized on scene components:

- serve delay, launch speed, angles, and rally acceleration on `BallController`
- speed, arena half height, and full paddle length on `PaddleMovement`
- paired paddle length on `SeatDirector`
- tracking dead zone on `ComputerPaddleController`
- default winning score and opening serve on `MatchController`

The player-facing points target and game speed are stored in Settings and applied when a match starts. Settings are clamped to supported ranges before use.

## Interface flow

The main menu is the entry point, and its match card shows the lineup, including empty seats, so a
third and fourth player are discoverable without opening a screen to find out. Classic is playable
and takes whoever is seated; Practice and AI League remain visible as intentionally disabled future
modes. There is no Local Multiplayer mode: the seats decide who plays, so a mode meaning "two humans"
would contradict the players screen rather than add anything.

The customization workshop previews a draft live and only saves on Apply; leaving discards. The HUD
shows mode, score, player identities, serve status, pause, and remaining match progress as one pip
per point needed. Pause and results use theme-specific overlays so the active match is never reloaded
accidentally.

Theme switching changes presentation only. Paddle and ball colliders, movement speeds, ball velocity, scoring, serve direction, pause state, and match progress remain identical. Framing the arena is a camera change: no world geometry moves.

Change tuning in small increments and verify paddle bounds at both walls, shallow and steep rebounds, several consecutive rallies, scoring on both sides, pause, restart, and a 2v2 lineup where the attacker and goalkeeper must not fight for the same ball.
