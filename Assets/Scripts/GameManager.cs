using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public enum GameState { Playing, Won, Lost }

    /// <summary>
    /// Score, level progression, win/lose, power-up effects, and the pop/drop resolve loop.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }
        public int HighScore { get; private set; }
        public int Level { get; private set; } = 1;
        public int Moves { get; private set; }
        public int Combo { get; private set; }
        public int BombsLeft = 2;
        public int FireballsLeft = 1;
        public int LightningsLeft = 1;
        public bool IsPaused { get; private set; }

        public BubbleBoard Board { get; private set; }
        public Shooter Shooter { get; private set; }

        int shotsSinceDescend;
        public int ShotsPerDescend => Mathf.Max(3, 7 - Level / 2); // pressure ramps with level
        public int ShotsUntilDescend => Mathf.Max(0, ShotsPerDescend - shotsSinceDescend);

        public void Init(BubbleBoard board, Shooter shooter)
        {
            I = this;
            Board = board; Shooter = shooter;
            HighScore = PlayerPrefs.GetInt("BS3D_HighScore", 0);
            StartLevel(1);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            if (State != GameState.Playing) return;
            IsPaused = !IsPaused;
            Time.timeScale = IsPaused ? 0f : 1f;
            if (UIManager.I != null) UIManager.I.ShowPause(IsPaused);
        }

        static readonly string[] themeNames = {
            "SAPPHIRE REEF",
            "NEON HORIZON",
            "EMERALD CITADEL",
            "AMETHYST VOID",
            "SOLARIS FLARE",
            "CYBER MATRIX",
            "CRYSTAL CHASM",
            "ASTRAL NEXUS"
        };

        public string CurrentThemeName => themeNames[(Level - 1) % themeNames.Length];

        public void StartLevel(int level)
        {
            Level = level;
            State = GameState.Playing;
            shotsSinceDescend = 0; Combo = 0; Moves = 0;
            BombsLeft = 1 + level / 2;
            FireballsLeft = 1 + level / 3;
            LightningsLeft = 1 + level / 3;

            int rows = Mathf.Min(5 + level, 9);
            int colors = Mathf.Min(3 + level, 6);
            Board.TopY = 6.4f;
            Board.SeedRows(rows, colors, level);
            ApplyLevelAtmosphere(level);

            if (UIManager.I != null)
            {
                UIManager.I.SetHud();
                UIManager.I.ShowLevelBanner($"LEVEL {level}\n{CurrentThemeName}");
            }
        }

        void ApplyLevelAtmosphere(int level)
        {
            int t = (level - 1) % 4;
            Color ambient = t switch
            {
                0 => new Color(0.14f, 0.22f, 0.38f),
                1 => new Color(0.38f, 0.14f, 0.28f),
                2 => new Color(0.12f, 0.34f, 0.24f),
                _ => new Color(0.26f, 0.16f, 0.40f)
            };
            RenderSettings.ambientLight = ambient;
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
            float t = 0; Vector3 from = b.transform.position;
            while (t < 0.12f) { t += Time.deltaTime; b.transform.position = Vector3.Lerp(from, target, t / 0.12f); yield return null; }
            b.transform.position = target;
            Vector3 s = b.transform.localScale;
            float k = 0;
            while (k < 0.15f)
            {
                k += Time.deltaTime;
                b.transform.localScale = s * (1f + 0.25f * Mathf.Sin(k / 0.15f * Mathf.PI));
                yield return null;
            }
            b.transform.localScale = s;
        }

        public void OnFireballHitBubble(Bubble b)
        {
            if (b == null) return;
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

            if (hadMatches)
            {
                Combo++;
                int i = 0;
                foreach (var cell in popped)
                {
                    var bub = Board.Get(cell.x, cell.y);
                    if (bub == null) continue;
                    Board.Remove(cell.x, cell.y);
                    EffectsManager.I.Burst(bub.transform.position, bub.Color);
                    StartCoroutine(bub.PopAnim(i * 0.03f));
                    AudioManager.I.PlayPop(i);
                    i++;
                }

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
                foreach (var cell in floating)
                {
                    var bub = Board.Get(cell.x, cell.y);
                    if (bub == null) continue;
                    Board.Remove(cell.x, cell.y);
                    StartCoroutine(bub.DropAnim());
                    dropped++;
                }
                if (dropped > 0)
                {
                    int dropPoints = dropped * 25;
                    Score += dropPoints;
                    UpdateHighScore();
                    AudioManager.I.Play("drop");
                    EffectsManager.I.Popup(new Vector3(0f, 0f, 0f), "+" + dropPoints + " DROPPED!", new Color(0.4f, 0.9f, 1f));
                }
                shotsSinceDescend = 0;
            }
            else
            {
                Combo = 0;
                shotsSinceDescend++;
                if (shotsSinceDescend >= ShotsPerDescend)
                {
                    shotsSinceDescend = 0;
                    Board.AddTopRow(Mathf.Min(3 + Level, 6));
                    AudioManager.I.Play("descend");
                    EffectsManager.I.Shake(0.18f);
                }
            }

            if (UIManager.I != null) UIManager.I.SetHud(popped.Count, dropped);

            if (Board.Count == 0)
            {
                State = GameState.Won;
                Score += 500 + Level * 250;
                UpdateHighScore();
                AudioManager.I.Play("win");
                int stars = CalculateStars(Moves, Level);
                if (UIManager.I != null) UIManager.I.ShowWin(Score, Level, stars);
            }
            else if (Board.AnyBubbleBelowDeathLine())
            {
                State = GameState.Lost;
                AudioManager.I.Play("lose");
                if (UIManager.I != null) UIManager.I.ShowLose(Score, Level);
            }
            else
            {
                Shooter.OnProjectileDone();
            }
        }

        public int CalculateStars(int moves, int level)
        {
            int par = 12 + level * 2;
            if (moves <= par) return 3;
            if (moves <= par + 6) return 2;
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
            foreach (var b in Board.AllBubbles()) Destroy(b.gameObject);
            ClearBoard();
            StartLevel(Level + 1);
            Shooter.OnProjectileDone();
        }

        public void Restart()
        {
            foreach (var b in Board.AllBubbles()) Destroy(b.gameObject);
            ClearBoard();
            Score = 0;
            StartLevel(1);
            Shooter.OnProjectileDone();
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
