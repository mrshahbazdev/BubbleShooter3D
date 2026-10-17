using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public enum GameState { Playing, Won, Lost }

    /// <summary>Score, level progression, win/lose, and the pop/drop resolve loop.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public GameState State { get; private set; } = GameState.Playing;
        public int Score { get; private set; }
        public int Level { get; private set; } = 1;
        public int Moves { get; private set; }
        public int Combo { get; private set; }
        public int BombsLeft = 2;

        public BubbleBoard Board { get; private set; }
        public Shooter Shooter { get; private set; }

        int shotsSinceDescend;
        int ShotsPerDescend => Mathf.Max(3, 7 - Level / 2); // pressure ramps with level

        public void Init(BubbleBoard board, Shooter shooter)
        {
            I = this;
            Board = board; Shooter = shooter;
            StartLevel(1);
        }

        public void StartLevel(int level)
        {
            Level = level;
            State = GameState.Playing;
            shotsSinceDescend = 0; Combo = 0; Moves = 0;
            BombsLeft = 1 + level / 2;
            int rows = Mathf.Min(5 + level, 9);
            int colors = Mathf.Min(3 + level, 6);
            Board.TopY = 6.4f;
            Board.SeedRows(rows, colors);
            UIManager.I.SetHud();
            UIManager.I.ShowLevelBanner(level);
        }

        public void OnProjectileSnapped(Projectile p, Vector2Int cell, Vector3 contact)
        {
            var b = Board.PlaceBubble(p.Color, cell.x, cell.y);
            b.transform.position = contact; // then slide into place
            StartCoroutine(SlideIn(b, Board.CellToWorld(cell.x, cell.y)));
            Destroy(p.gameObject, 0.05f);
            Moves++;
            StartCoroutine(Resolve(b));
        }

        IEnumerator SlideIn(Bubble b, Vector3 target)
        {
            float t = 0; Vector3 from = b.transform.position;
            while (t < 0.12f) { t += Time.deltaTime; b.transform.position = Vector3.Lerp(from, target, t / 0.12f); yield return null; }
            b.transform.position = target;
        }

        IEnumerator Resolve(Bubble placed)
        {
            yield return new WaitForSeconds(0.12f);
            List<Vector2Int> popped;
            if (placed.Color == BubbleColor.Bomb)
            {
                popped = MatchSolver.GetRadius(Board, placed.Row, placed.Col, 1);
                AudioManager.I.Play("bomb");
                EffectsManager.I.Shockwave(placed.transform.position);
            }
            else
            {
                popped = MatchSolver.GetMatchCluster(Board, placed.Row, placed.Col);
                if (popped.Count < 3) popped.Clear();
            }

            int dropped = 0;
            if (popped.Count >= 3 || placed.Color == BubbleColor.Bomb)
            {
                Combo++;
                int i = 0;
                foreach (var cell in popped)
                {
                    var bub = Board.Get(cell.x, cell.y);
                    if (bub == null) continue;
                    Board.Remove(cell.x, cell.y);
                    EffectsManager.I.Burst(bub.transform.position, bub.Color);
                    StartCoroutine(bub.PopAnim(i++ * 0.03f));
                }
                AudioManager.I.Play("pop");
                Score += popped.Count * 10 * Combo;

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
                if (dropped > 0) { Score += dropped * 25; AudioManager.I.Play("drop"); }
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
                }
            }

            UIManager.I.SetHud(popped.Count, dropped);

            if (Board.Count == 0)
            {
                State = GameState.Won;
                Score += 500 + Level * 250;
                AudioManager.I.Play("win");
                UIManager.I.ShowWin(Score, Level);
            }
            else if (Board.AnyBubbleBelowDeathLine())
            {
                State = GameState.Lost;
                AudioManager.I.Play("lose");
                UIManager.I.ShowLose(Score, Level);
            }
            else
            {
                Shooter.OnProjectileDone();
            }
        }

        void NextProjectileIfPlaying()
        {
            if (State == GameState.Playing) Shooter.OnProjectileDone();
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
            // reset internal rows by re-initting the grid transform children
            var root = Board.transform.Find("Bubbles");
            if (root != null)
                foreach (Transform child in root) Destroy(child.gameObject);
            Board.SeedClear();
        }
    }
}
