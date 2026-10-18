# Bubble Shooter 3D

A complete 3D bubble shooter built with **Unity 2022.3 LTS + C#** — no external assets needed, everything (models, materials, UI, sounds, particles) is generated procedurally at runtime.

## Features

- **Full 3D gameplay** — glossy emissive bubbles, soft shadows, neon rails, perspective camera
- **Hex grid board** — staggered-row layout hanging from the ceiling
- **Trajectory preview** — dotted aim line with wall-bounce prediction (up to 3 bounces)
- **Match-3 popping** — flood-fill cluster detection with combo multipliers
- **Floating drops** — detached clusters fall with real physics (Rigidbody + torque)
- **Power-ups** — Bomb bubble [B] destroys a radius; Rainbow bubble matches any color
- **Pressure mechanic** — new row pushes down after N missed shots (ramps with level)
- **Infinite levels** — more rows and colors each level, win bonus, score/moves/combo HUD
- **Synthesized SFX** — pop, shoot, bomb, drop, win/lose jingles generated in code
- **Swap** — press X to swap current/next bubble

## Controls

| Action | Input |
|---|---|
| Aim | Mouse |
| Fire | Left click |
| Swap bubble | X |
| Bomb shot | B |

## How to run

1. Install **Unity Hub** and a supported editor — **Unity 6** (6000.x) or **Unity 2022.3 LTS up to 2022.3.62f1**.
   - Note: 2022.3.63f1+ builds are *Extended LTS* and require an Enterprise/Industry license — they will not activate on a Personal license. Verified end-to-end on **Unity 6000.6.6f1** (compile + editor smoke tests).
2. Unity Hub → **Open** → select this folder.
3. Open `Assets/Scenes/Main.unity` and press **Play**.

The scene contains a single `Bootstrap` object — `GameBootstrap.cs` builds the
camera, lights, board, cannon, UI and audio at runtime, so no asset setup is
needed.

## Structure

```
Assets/Scripts/
  GameBootstrap.cs   — scene builder / entry point
  BubbleBoard.cs     — hex grid model, snap logic, row pressure
  Bubble.cs          — bubble visuals, pop/drop animations
  MatchSolver.cs     — match-3 clusters, radius, floating detection
  Projectile.cs      — fired bubble flight + analytic collision
  Shooter.cs         — cannon, aim, trajectory line, reload/power-ups
  GameManager.cs     — score, combos, levels, win/lose
  UIManager.cs       — code-built uGUI HUD + win/lose panels
  EffectsManager.cs  — particle bursts + bomb shockwave
  AudioManager.cs    — procedurally generated sound effects
  BubblePalette.cs   — colors + glossy materials
```
