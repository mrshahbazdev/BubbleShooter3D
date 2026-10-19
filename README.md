# Bubble Shooter 3D — Advanced Edition v2

An advanced, arcade-grade 3D bubble shooter built with **Unity 6 / Unity 2022.3 LTS + C#** — zero external assets required. Everything (models, materials, shaders, UI, audio, music, particle systems, environments) is procedurally generated at runtime.

This v2 upgrade adds a **cinematic 3D presentation layer** (camera rig, per-theme 3D environments, rim-light bubble shader) and **deep gameplay systems** (3 game modes, progression saves, 3 new power-ups, touch controls, tutorial, boss levels, 12 hand-designed puzzle layouts).

## What's New in v2

### Cinematic 3D Presentation
- **CameraRig** — the camera rides on a pivot: gentle idle sway, aim-following parallax, subtle pitch with aim height, and a **dolly-in zoom kick on every combo** (2x, 3x, …). Screen shake still works independently on the camera itself.
- **EnvironmentBuilder** — procedural 3D dressing per level theme: metallic crystal shard clusters flanking the arena, floating rock islands, ambient drifting dust particles, and exponential distance fog — all re-tinted live when the theme crossfades.
- **BubbleRim shader** (`Assets/Shaders/BubbleRim.shader`) — custom fresnel rim-light shader gives every bubble a glowing 3D edge; falls back to Standard automatically if the shader asset is ever missing.

### Gameplay Depth
- **3 Game Modes** (main-menu select):
  - 🕹 **Classic** — endless levels, move limits, ceiling pressure.
  - ⏱ **Time Attack** — 90-second score rush, unlimited shots, live timer.
  - 🧩 **Puzzle** — 12 hand-designed layouts with par ratings; 3 stars for beating par.
- **Progression saves** — unlocked levels, per-level star ratings, and best scores persist per mode (`ProgressManager`, PlayerPrefs).
- **3 New Power-ups** (bottom bar + hotkeys):
  - ❄ **Freeze [V]** — ceiling holds still for 5 shots.
  - ✸ **Multishot [M]** — fires a 3-bubble spread (±14°).
  - 🛡 **Shield [H]** — absorbs the next ceiling descent.
- **Boss levels** — every 10th Classic level spawns a 3×3 armored **Pressure Core** (27 HP, pulsing red armor, cracks as it takes damage). Damage it with adjacent pops or direct bomb/lightning/fireball hits; it spawns minion bubbles every 4 shots. Boss HP bar on the HUD.
- **Touch controls** — drag to aim, release to fire; full mouse parity. Mobile-ready UI scaling.
- **Tutorial** — contextual first-time hints on Classic level 1 (aim → fire → match → swap → power-up).
- **Main menu** — mode select with best scores, 12-level grid with star/lock states, mute toggle.

## Previous Feature Set (still in)
- Hologram aim-ghost landing prediction, bouncing trajectory dots, cannon recoil + muzzle flash + point light.
- Bomb / Fireball / Lightning power-ups, Rainbow wildcards, Stone blockers.
- Combo callouts, pentatonic pop cascade audio, procedural ambient BGM, avalanche/mega-drop events.
- 8 crossfading level themes (Cyberpunk, Magma, Emerald, Amethyst, Solaris, Arctic, Synthwave, Ocean).
- Collector pods with jackpot, star ratings, pause menu, high-score tracking.

## Controls

| Action | Input | On-Screen Button |
|---|---|---|
| Aim | Mouse move / touch drag | — |
| Fire | Left click / lift finger | — |
| Swap Bubble | **X** | **SWAP [X]** |
| Bomb Shot | **B** | **BOMB [B]** |
| Fireball Shot | **F** | **FIRE [F]** |
| Lightning Zap | **L** | **ZAP [L]** |
| Freeze | **V** | **FREEZE [V]** |
| Multishot | **M** | **MULTI [M]** |
| Shield | **H** | **SHIELD [H]** |
| Pause / Settings | **ESC** | **⏸** (top right) |

## How to Run

1. Open **Unity Hub**.
2. Click **Add** / **Open** → select this folder.
3. Select **Unity 6** (6000.x) or **Unity 2022.3 LTS**.
4. Open `Assets/Scenes/Main.unity` and click **Play** — the main menu appears; pick a mode and play.

## Automated Verification

**Headless logic tests (no Unity needed)** — compiles ALL 18 gameplay scripts against extended Unity API stubs and runs 47 checks (board math, match solver, progression saves, all 12 puzzle layouts, boss core setup/damage/defeat, no-drop rule):

```powershell
cd verify
dotnet build
dotnet run
```

Requires the .NET 8 SDK. Expected: `ALL TESTS PASSED`.

**In-Unity smoke tests** (batch mode):

```powershell
& "C:\Program Files\Unity\Hub\Editor\6000.6.6f1\Editor\Unity.exe" -batchmode -projectPath . -executeMethod BubbleShooter.EditorTools.EditorSmokeTests.RunAll -quit -logFile -
```

## Project Layout

```
Assets/
  Scripts/
    GameBootstrap.cs      # entry point: builds camera rig, lights, arena, managers
    GameManager.cs        # modes, scoring, power-ups, boss hooks, resolve loop
    BubbleBoard.cs        # staggered hex grid
    Shooter.cs            # cannon, mouse+touch aim, trajectory, multishot
    CameraRig.cs          # NEW: cinematic pivot camera
    EnvironmentBuilder.cs # NEW: per-theme 3D environment + fog
    ProgressManager.cs    # NEW: unlocks, stars, best scores
    PuzzleLayouts.cs      # NEW: 12 hand-designed puzzle layouts
    BossLevel.cs          # NEW: pressure-core boss levels
    TutorialManager.cs    # NEW: first-time hints
    Bubble.cs / BubblePalette.cs / Projectile.cs / MatchSolver.cs
    EffectsManager.cs / AudioManager.cs / ThemeManager.cs / UIManager.cs
  Shaders/
    BubbleRim.shader      # NEW: fresnel rim-light bubble shader
  Scenes/Main.unity
  Editor/EditorSmokeTests.cs
verify/                       # headless dotnet test harness (Unity not required)
```

## Notes
- Boss levels trigger on Classic levels 10, 20, 30, … — the win condition becomes "destroy the core", not "clear the board".
- `PlayerPrefs` keys are prefixed `BS3D_`; delete them (or use `ProgressManager.ResetAll`) to wipe progression.
- The game starts at the main menu (`GameState.Menu`); HUD power-up buttons are inert until a level starts.
