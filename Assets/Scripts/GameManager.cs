using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public enum GameState { Menu, Playing, Won, Lost }

    /// <summary>
    /// Score, level progression, game modes, win/lose, power-up effects,
    /// boss levels, freeze/shield mechanics, and the pop/drop resolve loop.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public GameState State { get; private set; } = GameState.Menu;
        public GameMode Mode { get; private set; } = GameMode.Classic;
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Level { get; private set; } = 1;
        public int Moves { get; private set; }
        public int MaxMoves { get; private set; } = 30;
        public int MovesRemaining => Mode == GameMode.TimeAttack ? int.MaxValue : Mathf.Max(0, MaxMoves - Moves);
        public int Combo { get; private set; }
        public int BombsLeft = 2;
        public int FireballsLeft = 1;
        public int LightningsLeft = 1;
        public int FreezesLeft = 1;
        public int MultishotsLeft = 1;
        public int ShieldsLeft = 1;
        public int FreezeShotsLeft { get; private set; }
        public bool ShieldActive { get; private set; }
        public bool IsPaused { get; private set; }

        public float TimeAttackDuration = 90f;
        public float TimeLeft { get; private set; }
        public int PuzzlePar { get; private set; }

        public BubbleBoard Board { get; private set; }
        public Shooter Shooter { get; private set; }

        int shotsSinceDescend;
        float lastAlarmTime = -10f;
        public int ShotsPerDescend => Mathf.Max(3, 7 - Level / 2); // pressure ramps with level
        public int ShotsUntilDescend => Mathf.Max(0, ShotsPerDescend - shotsSinceDescend);

        public void Init(BubbleBoard board, Shooter shooter)
        {
            I = this;
            Board = board; Shooter = shooter;
            HighScore = PlayerPrefs.GetInt("BS3D_HighScore", 0);
            ShowMenu();
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
            if (State == GameState.Playing && !IsPaused && Mode == GameMode.TimeAttack)
            {
                TimeLeft -= Time.deltaTime;
                if (UIManager.I != null) UIManager.I.SetTimer(TimeLeft);
                if (TimeLeft <= 0f)
                {
                    TimeLeft = 0f;
                    EndTimeAttack();
                }
            }
        }

        public void TogglePause()
        {
            if (State != GameState.Playing) return;
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            if (UIManager.I != null) UIManager.I.ShowPause(IsPaused);
        }

        public string CurrentThemeName => ThemeManager.I != null ? ThemeManager.I.CurrentTheme.Name : "LEVEL " + Level;
        public Color CurrentThemeAccent => ThemeManager.I != null ? ThemeManager.I.CurrentTheme.Accent : Color.white;

        public string ModeLabel => Mode switch
        {
            GameMode.TimeAttack => "TIME ATTACK",
            GameMode.Puzzle => "PUZZLE",
            _ => "CLASSIC"
        };

        public void ShowMenu()
        {
            State = GameState.Menu;
            IsPaused = false;
            Time.timeScale = 1f;
            if (BossLevel.I != null) BossLevel.I.Teardown();
            if (TutorialManager.I != null) TutorialManager.I.Cancel();
            if (UIManager.I != null) UIManager.I.ShowMainMenu();
        }

        public void StartLevel(int level, GameMode mode)
        {
            Mode = mode;
            Level = level;
            State = GameState.Playing;
            IsPaused = false;
            Time.timeScale = 1f;
            shotsSinceDescend = 0; Combo = 0; Moves = 0;
            FreezeShotsLeft = 0; ShieldActive = false;
            BombsLeft = 1 + level / 2;
            FireballsLeft = 1 + level / 3;
            LightningsLeft = 1 + level / 3;
            FreezesLeft = 1 + level / 3;
            MultishotsLeft = 1 + level / 4;
            ShieldsLeft = 1 + level / 4;

            if (BossLevel.I != null) BossLevel.I.Teardown();
            ClearAllBubbles();
            Board.TopY = 6.4f;

            bool isBoss = false;
            if (mode == GameMode.TimeAttack)
            {
                TimeLeft = TimeAttackDuration;
                MaxMoves = int.MaxValue;
                Board.SeedRows(6, 4, 1);
            }
            else if (mode == GameMode.Puzzle)
            {
                var layout = PuzzleLayouts.Get(level);
                PuzzlePar = layout.Par;
                MaxMoves = layout.Par + 8;
                SeedPuzzleLayout(layout);
            }
            else
            {
                // Progressive move constraints as level climbs
                MaxMoves = level switch
                {
                    1 => 30,
                    2 => 26,
                    3 => 23,
                    4 => 20,
                    _ => Mathf.Max(14, 20 - (level - 4))
                };

                int rows = Mathf.Min(5 + level, 9);
                int colors = Mathf.Min(3 + level, 6);
                Board.SeedRows(rows, colors, level);

                if (BossLevel.IsBossLevel(level) && BossLevel.I != null)
                {
                    BossLevel.I.Setup(Board, level);
                    isBoss = true;
                }
            }

            if (ThemeManager.I != null)
                ThemeManager.I.ApplyTheme(level, immediate: true);

            if (TutorialManager.I != null)
                TutorialManager.I.BeginIfNeeded(mode, level);

            if (UIManager.I != null)
            {
                UIManager.I.HideMainMenu();
                UIManager.I.SetHud();
                UIManager.I.ShowBossBar(isBoss);
                if (!isBoss)
                    UIManager.I.ShowLevelBanner($"LEVEL {level} · {ModeLabel}\n{CurrentThemeName}", CurrentThemeAccent);
            }
            if (Shooter != null) Shooter.OnProjectileDone();
        }

        void SeedPuzzleLayout(PuzzleLayout layout)
        {
            for (int r = 0; r < layout.Rows.Length; r++)
            {
                string row = layout.Rows[r];
                int cols = Mathf.Min(row.Length, Board.ColsInRow(r));
                for (int c = 0; c < cols; c++)
                {
                    var col = PuzzleLayouts.CharToColor(row[c]);
                    if (col.HasValue) Board.PlaceBubble(col.Value, r, c);
                }
            }
        }

        /// <summary>
        /// Called when a falling bubble bounces into one of the 5 bottom score collector pods.
        /// </summary>
        public void TriggerCollectorPod(Vector3 hitPos)
        {
            int pts = 50;
            string label = "+50";
            Color col = new Color(0.35f, 0.85f, 1f);

            if (Mathf.Abs(hitPos.x) < 0.9f)
            {
                pts = 500;
                label = "★ 500 JACKPOT! ★";
                col = new Color(1f, 0.85f, 0.15f);
                AudioManager.I.Play("combo");
                EffectsManager.I.Shake(0.18f);
            }
            else if (Mathf.Abs(hitPos.x) < 2.5f)
            {
                pts = 150;
                label = "+150 POD BONUS";
                col = new Color(0.2f, 1f, 0.6f);
                AudioManager.I.Play("podHit");
            }
            else
            {
                pts = 50;
                label = "+50";
                col = new Color(0.4f, 0.85f, 1f);
                AudioManager.I.Play("podHit");
            }

            Score += pts;
            UpdateHighScore();
            EffectsManager.I.Popup(new Vector3(hitPos.x, -3.2f, -0.6f), label, col);
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        public void OnProjectileSnapped(Projectile p, Vector2Int cell, Vector3 contact)
        {
            var b = Board.PlaceBubble(p.Color, cell.x, cell.y);
            b.transform.position = contact;
            StartCoroutine(SlideIn(b, Board.CellToWorld(cell.x, cell.y)));
            Destroy(p.gameObject, 0.05f);
            Moves++;
            StartCoroutine(Resolve(b, contact));
        }

        IEnumerator SlideIn(Bubble b, Vector3 target)
        {
            if (b == null) yield break;
            float t = 0; Vector3 from = b.transform.position;
            while (t < 0.12f)
            {
                if (b == null) yield break;
                t += Time.deltaTime;
                b.transform.position = Vector3.Lerp(from, target, t / 0.12f);
                yield return null;
            }
            if (b == null) yield break;
            b.transform.position = target;
            Vector3 s = b.transform.localScale;
            float k = 0;
            while (k < 0.15f)
            {
                if (b == null) yield break;
                k += Time.deltaTime;
                b.transform.localScale = s * (1f + 0.25f * Mathf.Sin(k / 0.15f * Mathf.PI));
                yield return null;
            }
            if (b != null) b.transform.localScale = s;
        }

        public void OnFireballHitBubble(Bubble b)
        {
            if (b == null) return;
            // Fireball cracks boss armor instead of vaporizing it.
            if (BossLevel.I != null && BossLevel.I.IsCoreCell(b.Row, b.Col))
            {
                BossLevel.I.DamageCell(b.Row, b.Col, 1);
                return;
            }
            Board.Remove(b.Row, b.Col);
            EffectsManager.I.FireBurst(b.transform.position);
            EffectsManager.I.Shake(0.12f);
            Score += 50;
            AudioManager.I.PlayPop(Random.Range(2, 6));
            StartCoroutine(b.PopAnim(0f));
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        public void OnFireballComplete(Vector3 ceilPos)
        {
            EffectsManager.I.FireBurst(ceilPos);
            EffectsManager.I.Shockwave(ceilPos);
            EffectsManager.I.Shake(0.3f);
            Moves++;
            StartCoroutine(ResolvePostShot(null));
        }

        /// <summary>Freeze: the ceiling holds still for the next 5 shots.</summary>
        public void ActivateFreeze()
        {
            if (FreezesLeft <= 0 || State != GameState.Playing) return;
            FreezesLeft--;
            FreezeShotsLeft = 5;
            AudioManager.I.Play("freeze");
            EffectsManager.I.FreezeBurst(new Vector3(0f, 2f, -0.5f));
            EffectsManager.I.Popup(new Vector3(0f, 1.5f, -1f), "❄ FROZEN! Ceiling holds 5 shots", new Color(0.55f, 0.9f, 1f));
            if (TutorialManager.I != null) TutorialManager.I.OnPowerupUsed();
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        /// <summary>Shield: absorbs the next ceiling descent.</summary>
        public void ActivateShield()
        {
            if (ShieldsLeft <= 0 || State != GameState.Playing || ShieldActive) return;
            ShieldsLeft--;
            ShieldActive = true;
            AudioManager.I.Play("shield");
            EffectsManager.I.ShieldRing(new Vector3(0f, 1.5f, 0f), 6f);
            EffectsManager.I.Popup(new Vector3(0f, 1.5f, -1f), "🛡 SHIELD UP! Next descent blocked", new Color(0.4f, 1f, 0.6f));
            if (TutorialManager.I != null) TutorialManager.I.OnPowerupUsed();
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        void ConsumeShield()
        {
            ShieldActive = false;
            AudioManager.I.Play("shield");
            EffectsManager.I.ShieldRing(new Vector3(0f, 1.5f, 0f), 6f);
            EffectsManager.I.Shake(0.22f);
            EffectsManager.I.Popup(new Vector3(0f, 0.5f, -1f), "🛡 SHIELD ABSORBED THE DESCENT!", new Color(0.4f, 1f, 0.6f));
        }

        public void OnBossDefeated()
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;
            EffectsManager.I.MegaConfetti(new Vector3(0f, 1.5f, -1f));
            EffectsManager.I.Shake(0.4f);
            AudioManager.I.Play("bossDie");
            StartCoroutine(CoVictorySequence());
        }

        void EndTimeAttack()
        {
            if (State != GameState.Playing) return;
            State = GameState.Won;
            AudioManager.I.Play("timeout");
            int stars = CalculateStars(Moves, Level);
            bool newRecord = SaveProgress(stars);
            if (UIManager.I != null) UIManager.I.ShowWin(Score, Level, stars, isTimeAttack: true, newRecord: newRecord);
        }

        IEnumerator Resolve(Bubble placed, Vector3 contact)
        {
            yield return new WaitForSeconds(0.12f);
            List<Vector2Int> popped;

            if (placed.Color == BubbleColor.Bomb)
            {
                popped = MatchSolver.GetRadius(Board, placed.Row, placed.Col, 1);
                AudioManager.I.Play("bomb");
                EffectsManager.I.Shockwave(placed.transform.position);
                EffectsManager.I.Shake(0.38f);
            }
            else if (placed.Color == BubbleColor.Lightning)
            {
                popped = MatchSolver.GetRow(Board, placed.Row);
                AudioManager.I.Play("zap");
                EffectsManager.I.ElectricBurst(placed.transform.position);
                EffectsManager.I.Shockwave(placed.transform.position);
                EffectsManager.I.Shake(0.35f);
            }
            else
            {
                popped = MatchSolver.GetMatchCluster(Board, placed.Row, placed.Col);
                if (popped.Count < 3) popped.Clear();
                Vector3 impactDir = placed.transform.position - contact;
                Board.JiggleNeighbors(placed.Row, placed.Col, impactDir);
            }

            yield return ResolveClustersAndDrops(popped, placed.transform.position, placed.Color);
        }

        IEnumerator ResolvePostShot(Bubble placed)
        {
            yield return new WaitForSeconds(0.15f);
            yield return ResolveClustersAndDrops(new List<Vector2Int>(), Vector3.zero, BubbleColor.Red);
        }

        IEnumerator ResolveClustersAndDrops(List<Vector2Int> popped, Vector3 centerPos, BubbleColor shotColor)
        {
            int dropped = 0;
            bool hadMatches = popped.Count >= 3 || shotColor == BubbleColor.Bomb || shotColor == BubbleColor.Lightning;
            var boss = BossLevel.I;

            if (hadMatches)
            {
                Combo++;
                if (CameraRig.I != null && Combo >= 2) CameraRig.I.KickZoom(Mathf.Min(0.35f * Combo, 1.6f));
                int i = 0;
                foreach (var cell in popped)
                {
                    // Boss armor takes damage instead of popping.
                    if (boss != null && boss.IsCoreCell(cell.x, cell.y))
                    {
                        int dmg = shotColor == BubbleColor.Bomb ? 3 : (shotColor == BubbleColor.Lightning ? 2 : 1);
                        boss.DamageCell(cell.x, cell.y, dmg);
                        continue;
                    }
                    var bub = Board.Get(cell.x, cell.y);
                    if (bub == null) continue;
                    Board.Remove(cell.x, cell.y);
                    EffectsManager.I.Burst(bub.transform.position, bub.Color);
                    StartCoroutine(bub.PopAnim(i * 0.03f));
                    AudioManager.I.PlayPop(i);
                    i++;
                }

                if (boss != null) boss.DamageAdjacent(popped);
                if (TutorialManager.I != null) TutorialManager.I.OnPopped(popped.Count);

                int points = popped.Count * 10 * Combo;
                Score += points;
                UpdateHighScore();

                if (centerPos != Vector3.zero)
                {
                    EffectsManager.I.Popup(centerPos, "+" + points, new Color(1f, 0.85f, 0.2f));
                }

                if (Combo > 1)
                {
                    AudioManager.I.Play("combo");
                    EffectsManager.I.ShowComboCallout(Combo);
                }

                yield return new WaitForSeconds(0.25f);
                var floating = MatchSolver.GetFloating(Board);
                int dropIdx = 0;
                foreach (var cell in floating)
                {
                    // Boss armor never drops — it must be destroyed by damage.
                    if (boss != null && !boss.IsDroppable(cell.x, cell.y)) continue;
                    var bub = Board.Get(cell.x, cell.y);
                    if (bub == null) continue;
                    Board.Remove(cell.x, cell.y);
                    float stagger = (bub.Row * 0.035f) + (dropIdx * 0.02f);
                    StartCoroutine(bub.DropAnim(stagger, dropIdx));
                    dropped++;
                    dropIdx++;
                }

                if (dropped > 0)
                {
                    int dropPoints = dropped * 35;
                    Score += dropPoints;
                    UpdateHighScore();

                    if (dropped >= 8)
                    {
                        // Catastrophic Avalanche
                        AudioManager.I.Play("avalanche");
                        EffectsManager.I.Shake(0.45f);
                        EffectsManager.I.MegaConfetti(new Vector3(0f, 1f, -1f));
                        EffectsManager.I.Popup(new Vector3(0f, 1.2f, -1f), $"💥 MASSIVE AVALANCHE!\n+{dropPoints} PTS ({dropped} BUBBLES!)", new Color(1f, 0.85f, 0.15f));
                    }
                    else if (dropped >= 4)
                    {
                        // Mega Drop
                        AudioManager.I.Play("avalanche");
                        EffectsManager.I.Shake(0.28f);
                        EffectsManager.I.MegaConfetti(new Vector3(0f, 1f, -1f));
                        EffectsManager.I.Popup(new Vector3(0f, 1f, -1f), $"⭐ MEGA DROP! +{dropPoints} PTS", new Color(0.35f, 0.95f, 1f));
                    }
                    else
                    {
                        AudioManager.I.Play("drop");
                        EffectsManager.I.Popup(new Vector3(0f, 0.5f, -1f), $"+{dropPoints} DROPPED!", new Color(0.4f, 0.9f, 1f));
                    }

                    AudioManager.I.PlayDropCascade(dropped);
                }
                shotsSinceDescend = 0;
            }
            else
            {
                Combo = 0;
                if (FreezeShotsLeft > 0)
                {
                    // ❄ Frozen: the ceiling holds — no pressure this shot.
                    FreezeShotsLeft--;
                    if (UIManager.I != null) UIManager.I.SetHud();
                }
                else
                {
                    shotsSinceDescend++;
                    if (shotsSinceDescend >= ShotsPerDescend)
                    {
                        shotsSinceDescend = 0;
                        if (ShieldActive)
                        {
                            ConsumeShield();
                        }
                        else
                        {
                            Board.AddTopRow(Mathf.Min(3 + Level, 6));
                            AudioManager.I.Play("descend");
                            EffectsManager.I.Shake(0.18f);
                        }
                    }
                }
            }

            if (boss != null) boss.OnShotResolved();
            if (TutorialManager.I != null) TutorialManager.I.OnAnyShot();

            if (UIManager.I != null) UIManager.I.SetHud(popped.Count, dropped);

            bool bossActive = boss != null && boss.IsActive;
            if (!bossActive && Board.Count == 0)
            {
                State = GameState.Won;
                StartCoroutine(CoVictorySequence());
            }
            else if (Board.AnyBubbleBelowDeathLine())
            {
                State = GameState.Lost;
                AudioManager.I.Play("lose");
                if (UIManager.I != null) UIManager.I.ShowLose(Score, Level);
            }
            else if (MovesRemaining <= 0)
            {
                State = GameState.Lost;
                AudioManager.I.Play("lose");
                EffectsManager.I.Popup(new Vector3(0f, 0f, -1f), "OUT OF SHOTS!", new Color(1f, 0.3f, 0.3f));
                if (UIManager.I != null) UIManager.I.ShowLose(Score, Level);
            }
            else
            {
                CheckDangerSiren();
                Shooter.OnProjectileDone();
            }
        }

        void CheckDangerSiren()
        {
            if (Time.time - lastAlarmTime < 4f) return;
            bool inDanger = false;
            foreach (var b in Board.AllBubbles())
            {
                if (b != null && b.transform.position.y <= Board.DeathLineY + 1.1f)
                {
                    inDanger = true;
                    break;
                }
            }
            if (inDanger)
            {
                lastAlarmTime = Time.time;
                AudioManager.I.Play("alarm");
                EffectsManager.I.Popup(new Vector3(0f, Board.DeathLineY + 0.6f, -1f), "⚠️ DANGER! BUBBLES NEAR LINE", new Color(1f, 0.35f, 0.2f));
            }
        }

        IEnumerator CoVictorySequence()
        {
            Time.timeScale = 0.35f;
            EffectsManager.I.MegaConfetti(new Vector3(0f, 1.5f, -1f));
            EffectsManager.I.Shake(0.35f);
            AudioManager.I.Play("combo");
            yield return new WaitForSecondsRealtime(0.85f);
            Time.timeScale = 1f;

            Score += 500 + Level * 250;
            UpdateHighScore();
            AudioManager.I.Play("win");
            int stars = CalculateStars(Moves, Level);
            bool newRecord = SaveProgress(stars);
            if (UIManager.I != null) UIManager.I.ShowWin(Score, Level, stars, newRecord: newRecord);
        }

        /// <summary>Persists stars, unlocks and best score. Returns true on a new best score.</summary>
        bool SaveProgress(int stars)
        {
            var pm = ProgressManager.I;
            if (pm == null) return false;
            pm.SetStars(Mode, Level, stars);
            pm.UnlockLevel(Mode, Level + 1);
            return pm.SetBest(Mode, Score);
        }

        public int CalculateStars(int moves, int level)
        {
            if (Mode == GameMode.TimeAttack)
            {
                if (Score >= 4000) return 3;
                if (Score >= 2200) return 2;
                return 1;
            }
            if (Mode == GameMode.Puzzle)
            {
                if (moves <= PuzzlePar) return 3;
                if (moves <= PuzzlePar + 4) return 2;
                return 1;
            }
            // Classic: award stars dynamically based on moves conserved
            if (MovesRemaining >= MaxMoves * 0.4f) return 3;
            if (MovesRemaining >= MaxMoves * 0.15f) return 2;
            return 1;
        }

        void UpdateHighScore()
        {
            if (Score > HighScore)
            {
                HighScore = Score;
                PlayerPrefs.SetInt("BS3D_HighScore", HighScore);
                PlayerPrefs.Save();
            }
        }

        public void OnProjectileLost(Projectile p)
        {
            Destroy(p.gameObject);
            Shooter.OnProjectileDone();
        }

        public void NextLevel()
        {
            ClearAllBubbles();
            if (Mode == GameMode.TimeAttack)
            {
                Score = 0;
                StartLevel(1, GameMode.TimeAttack);
            }
            else
            {
                StartLevel(Level + 1, Mode);
            }
            if (Shooter != null) Shooter.OnProjectileDone();
        }

        public void Restart()
        {
            ClearAllBubbles();
            Score = 0;
            StartLevel(Level, Mode);
            if (Shooter != null) Shooter.OnProjectileDone();
        }

        void ClearAllBubbles()
        {
            foreach (var b in Board.AllBubbles())
                if (b != null) Destroy(b.gameObject);
            ClearBoard();
        }

        void ClearBoard()
        {
            var root = Board.transform.Find("Bubbles");
            if (root != null)
                foreach (Transform child in root) Destroy(child.gameObject);
            Board.SeedClear();
        }
    }
}
