# Bubble Shooter 3D (Enhanced & Advanced Edition)

An advanced, arcade-grade 3D bubble shooter built with **Unity 6 / Unity 2022.3 LTS + C#** — zero external assets required. Everything (models, materials, shaders, UI, audio, music, particle systems) is procedurally generated at runtime.

## Advanced Features

- **Hologram Aim Ghost Prediction** — Displays an exact translucent ghost sphere at the grid cell where the projectile will snap before you fire.
- **Full 3D Arcade Visuals & Juice**:
  - Mechanical recoil on the cannon barrel with spring recovery.
  - Dynamic point-light muzzle flash on firing.
  - Hex grid neighbors jiggle and squash on bullet impact.
  - Floating combo callouts (*"NICE!"*, *"GREAT!"*, *"AWESOME!"*, *"UNSTOPPABLE!"*).
- **Expanded Special Power-Up Arsenal**:
  - **Bomb Bubble [B] / UI Button**: Destroys all neighboring bubbles within a radius with a high-intensity shockwave and screen shake.
  - **Fireball Bubble [F] / UI Button**: Blazes through obstacles, piercing and burning bubbles in its direct path all the way to the ceiling!
  - **Lightning Zap [L] / UI Button**: High-speed electric bolt that zaps and wipes out an entire horizontal row of bubbles!
  - **Rainbow Bubble**: Wildcard that adopts whatever neighboring color creates the largest match cluster.
- **Audio Experience**:
  - **Pentatonic Musical Pop Cascade**: Successive pops in a cluster pitch-shift upward along the musical scale (*C-D-E-G-A-C-D-E*), creating an immensely satisfying cascade.
  - **Procedural Ambient BGM**: Relaxing, dreamy synthesized chord arpeggios looping in the background.
  - Unique sound effects for Zap, Fireball, Bombs, and Combo chimes.
- **On-Screen Interactive Power-Up Bar** — Clickable HUD buttons for touch and mouse gameplay with live inventory count badges and active glowing states.
- **Performance & Progression**:
  - **Ceiling Descent Warning**: Real-time counter and flashing warning indicator when a pressure descent is imminent.
  - **High Score Tracking**: Automatically saved to `PlayerPrefs` and displayed on the HUD.
  - **Star Rating System**: 1, 2, or 3 stars awarded based on efficiency and moves taken per level.
  - **Pause & Settings Menu [ESC]**: In-game pause menu with Resume, Restart, and Audio Mute/Unmute toggle.

## Controls

| Action | Input | On-Screen Button |
|---|---|---|
| Aim | Mouse Movement | — |
| Fire | Left Click | — |
| Swap Bubble | **X** | **SWAP [X]** |
| Bomb Shot | **B** | **BOMB [B]** |
| Fireball Shot | **F** | **FIRE [F]** |
| Lightning Zap | **L** | **ZAP [L]** |
| Pause / Settings | **ESC** | **\|\|** (Top Right) |

## How to Run

1. Open **Unity Hub**.
2. Click **Add** / **Open** → select this cloned folder:
   `C:\Users\mrsha\.gemini\antigravity\scratch\BubbleShooter3D`
3. Select **Unity 6** (6000.6.6f1) or **Unity 2022.3 LTS**.
4. In Project window, open `Assets/Scenes/Main.unity` and click **Play**!

## Automated Verification

Run headless tests via:
```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod BubbleShooter.EditorTools.EditorSmokeTests.RunAll -quit -logFile -
```
