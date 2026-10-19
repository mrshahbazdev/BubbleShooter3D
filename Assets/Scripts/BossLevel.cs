using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Every 10th Classic level is a BOSS level: a 3x3 armored "Pressure Core"
    /// embedded in the board. It has shared HP — damage it by popping bubbles
    /// adjacent to it (or hitting it directly with bombs / lightning / fireballs).
    /// Every few shots it spawns a minion bubble. Destroy the core to win.
    /// </summary>
    public class BossLevel : MonoBehaviour
    {
        public static BossLevel I { get; private set; }

        public bool IsActive { get; private set; }
        public int CoreMaxHP { get; private set; }
        public int CoreHP { get; private set; }
        public float CoreHpFrac => CoreMaxHP <= 0 ? 0f : (float)CoreHP / CoreMaxHP;

        BubbleBoard board;
        readonly HashSet<Vector2Int> coreCells = new HashSet<Vector2Int>();
        int shotsSinceSpawn;
        const int SHOTS_PER_SPAWN = 4;
        const int CORE_HP_PER_CELL = 3;

        void Awake() { I = this; }

        public static bool IsBossLevel(int level) => level > 0 && level % 10 == 0;

        public void Setup(BubbleBoard board, int level)
        {
            Teardown();
            this.board = board;
            IsActive = true;
            shotsSinceSpawn = 0;

            int cMid = board.Cols / 2; // 5 for Cols=11
            for (int r = 2; r <= 4; r++)
                for (int c = cMid - 1; c <= cMid + 1; c++)
                {
                    if (c < 0 || c >= board.ColsInRow(r)) continue;
                    var existing = board.Get(r, c);
                    if (existing != null) Object.Destroy(existing.gameObject);
                    board.Remove(r, c);
                    var b = board.PlaceBubble(BubbleColor.Stone, r, c);
                    b.SetBossCore(CORE_HP_PER_CELL);
                    coreCells.Add(new Vector2Int(r, c));
                }

            CoreMaxHP = coreCells.Count * CORE_HP_PER_CELL;
            CoreHP = CoreMaxHP;

            if (UIManager.I != null)
                UIManager.I.ShowLevelBanner("⚠ BOSS LEVEL ⚠\nDESTROY THE PRESSURE CORE!", new Color(1f, 0.25f, 0.2f));
            if (AudioManager.I != null) AudioManager.I.Play("alarm");
        }

        public bool IsCoreCell(int r, int c) => IsActive && coreCells.Contains(new Vector2Int(r, c));

        /// <summary>
        /// Boss armor never drops when isolated — it must be destroyed by damage.
        /// GameManager's floating-drop loop skips cells where this is false.
        /// </summary>
        public bool IsDroppable(int r, int c) => !IsCoreCell(r, c);

        /// <summary>Called after a shot resolves: spawns a minion bubble every few shots.</summary>
        public void OnShotResolved()
        {
            if (!IsActive || board == null) return;
            shotsSinceSpawn++;
            if (shotsSinceSpawn < SHOTS_PER_SPAWN) return;
            shotsSinceSpawn = 0;
            SpawnMinion();
        }

        void SpawnMinion()
        {
            var empties = new List<Vector2Int>();
            for (int r = 0; r < Mathf.Min(3, board.RowCount); r++)
                for (int c = 0; c < board.ColsInRow(r); c++)
                    if (board.Get(r, c) == null && !IsCoreCell(r, c))
                        empties.Add(new Vector2Int(r, c));
            if (empties.Count == 0) return;

            var cell = empties[Random.Range(0, empties.Count)];
            var alive = board.RemainingPlayableColors();
            BubbleColor col = BubbleColor.Red;
            if (alive.Count > 0)
            {
                var arr = new List<BubbleColor>(alive);
                col = arr[Random.Range(0, arr.Count)];
            }
            var b = board.PlaceBubble(col, cell.x, cell.y);
            if (EffectsManager.I != null) EffectsManager.I.Burst(b.transform.position, col);
            if (AudioManager.I != null) AudioManager.I.Play("descend");
        }

        /// <summary>Adjacent pops crack the armor: 1 damage per neighboring popped bubble.</summary>
        public void DamageAdjacent(List<Vector2Int> poppedCells)
        {
            if (!IsActive) return;
            var hit = new HashSet<Vector2Int>();
            foreach (var cell in poppedCells)
                foreach (var n in board.Neighbors(cell.x, cell.y))
                    if (IsCoreCell(n.x, n.y)) hit.Add(n);
            foreach (var cell in hit) DamageCell(cell.x, cell.y, 1);
        }

        /// <summary>Direct hits (bomb / lightning / fireball) on a core cell.</summary>
        public void DamageCell(int r, int c, int amount)
        {
            if (!IsActive) return;
            var cell = new Vector2Int(r, c);
            if (!coreCells.Contains(cell)) return;
            var b = board.Get(r, c);
            if (b == null || !b.IsBossCore) return;

            b.HitPoints -= amount;
            CoreHP = Mathf.Max(0, CoreHP - amount);

            if (EffectsManager.I != null)
            {
                EffectsManager.I.Burst(b.transform.position, BubbleColor.Orange);
                EffectsManager.I.Shake(0.15f);
            }
            if (AudioManager.I != null) AudioManager.I.Play("click");
            if (UIManager.I != null) UIManager.I.SetHud();

            if (b.HitPoints <= 0)
            {
                coreCells.Remove(cell);
                board.Remove(r, c);
                if (EffectsManager.I != null)
                {
                    EffectsManager.I.Shockwave(b.transform.position);
                    EffectsManager.I.Burst(b.transform.position, BubbleColor.Red);
                }
                if (AudioManager.I != null) AudioManager.I.Play("bomb");
                StartCoroutine(b.PopAnim(0f));
            }
            else
            {
                b.RefreshBossVisual();
            }

            if (CoreHP <= 0 || coreCells.Count == 0)
            {
                IsActive = false;
                if (GameManager.I != null) GameManager.I.OnBossDefeated();
            }
        }

        public void Teardown()
        {
            IsActive = false;
            coreCells.Clear();
            CoreHP = 0;
            CoreMaxHP = 0;
            shotsSinceSpawn = 0;
        }
    }
}
