using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Staggered hex board hanging from a ceiling. Even rows have `Cols` cells,
    /// odd rows are shifted half a cell right and have Cols-1 cells.
    /// </summary>
    public class BubbleBoard : MonoBehaviour
    {
        public int Cols = 11;
        public int MaxRows = 16;
        public float BubbleRadius = 0.48f;
        public float TopY = 6.4f;
        public float DeathLineY = -3.4f;

        public float CellW { get; private set; }
        public float CellH { get; private set; }
        public float LeftWallX { get; private set; }
        public float RightWallX { get; private set; }

        readonly List<Bubble[]> rows = new List<Bubble[]>();
        GameObject bubbleProto;
        GameObject bubbleRoot;

        public int RowCount => rows.Count;

        public void Init()
        {
            CellW = BubbleRadius * 2f * 1.05f;
            CellH = CellW * 0.87f;
            float half = Cols * CellW / 2f;
            LeftWallX = -half;
            RightWallX = half;

            bubbleRoot = new GameObject("Bubbles");
            bubbleRoot.transform.SetParent(transform, false);

            bubbleProto = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(bubbleProto.GetComponent<Collider>());
            bubbleProto.SetActive(false);
            bubbleProto.name = "BubbleProto";
        }

        public int ColsInRow(int r) => (r % 2 == 0) ? Cols : Cols - 1;

        public Vector3 CellToWorld(int r, int c)
        {
            float x = LeftWallX + (c + 0.5f + (r % 2 == 1 ? 0.5f : 0f)) * CellW;
            float y = TopY - r * CellH;
            return new Vector3(x, y, 0f);
        }

        public Bubble Get(int r, int c)
        {
            if (r < 0 || r >= rows.Count || c < 0 || c >= ColsInRow(r)) return null;
            return rows[r][c];
        }

        public IEnumerable<Vector2Int> Neighbors(int r, int c)
        {
            bool odd = (r % 2) == 1;
            int[] dc = odd ? new[] { -1, 1, 0, 1, 0, 1 } : new[] { -1, 1, -1, 0, -1, 0 };
            int[] dr = { 0, 0, -1, -1, 1, 1 };
            for (int i = 0; i < 6; i++)
            {
                int nr = r + dr[i], nc = c + dc[i];
                if (nr >= 0 && nr < rows.Count && nc >= 0 && nc < ColsInRow(nr))
                    yield return new Vector2Int(nr, nc);
            }
        }

        /// <summary>All empty cells adjacent to any bubble on the board + empty ceiling cells.</summary>
        public List<Vector2Int> EmptyCellsNear(Vector3 point)
        {
            var set = new HashSet<Vector2Int>();
            foreach (var b in AllBubbles())
            {
                bool odd = (b.Row % 2) == 1;
                int[] dc = odd ? new[] { -1, 1, 0, 1, 0, 1 } : new[] { -1, 1, -1, 0, -1, 0 };
                int[] dr = { 0, 0, -1, -1, 1, 1 };
                for (int i = 0; i < 6; i++)
                {
                    int nr = b.Row + dr[i], nc = b.Col + dc[i];
                    if (nr >= 0 && nr < MaxRows && nc >= 0 && nc < ColsInRow(nr))
                    {
                        if (Get(nr, nc) == null)
                            set.Add(new Vector2Int(nr, nc));
                    }
                }
            }
            // also allow attaching to an empty top row
            for (int c = 0; c < ColsInRow(0); c++)
                if (Get(0, c) == null)
                    set.Add(new Vector2Int(0, c));

            var list = new List<Vector2Int>(set);
            list.Sort((a, b) =>
                Vector3.Distance(CellToWorld(a.x, a.y), point)
                    .CompareTo(Vector3.Distance(CellToWorld(b.x, b.y), point)));
            return list;
        }

        public Vector2Int? FindSnapCell(Vector3 contact)
        {
            var empty = EmptyCellsNear(contact);
            return empty.Count > 0 ? empty[0] : (Vector2Int?)null;
        }

        /// <summary>Nearest empty cell for a ceiling hit.</summary>
        public Vector2Int? FindCeilingCell(Vector3 hit)
        {
            int r = Mathf.Clamp(Mathf.RoundToInt((TopY - hit.y) / CellH), 0, MaxRows - 1);
            while (r >= rows.Count) EnsureRow();
            float best = float.MaxValue; Vector2Int? bestCell = null;
            for (int dr = 0; dr < 3 && r + dr < rows.Count; dr++)
                for (int c = 0; c < ColsInRow(r + dr); c++)
                {
                    if (rows[r + dr][c] != null) continue;
                    float d = Vector3.Distance(CellToWorld(r + dr, c), hit);
                    if (d < best) { best = d; bestCell = new Vector2Int(r + dr, c); }
                }
            return bestCell;
        }

        public Bubble PlaceBubble(BubbleColor color, int r, int c)
        {
            while (r >= rows.Count) EnsureRow();
            var go = Instantiate(bubbleProto, bubbleRoot.transform);
            go.SetActive(true);
            var b = go.AddComponent<Bubble>();
            b.Init(color, r, c, BubbleRadius);
            go.transform.position = CellToWorld(r, c);
            rows[r][c] = b;
            return b;
        }

        public void Remove(int r, int c)
        {
            if (r >= 0 && r < rows.Count && c >= 0 && c < ColsInRow(r)) rows[r][c] = null;
        }

        public IEnumerable<Bubble> AllBubbles()
        {
            foreach (var row in rows)
                foreach (var b in row)
                    if (b != null) yield return b;
        }

        public HashSet<BubbleColor> RemainingPlayableColors()
        {
            var s = new HashSet<BubbleColor>();
            foreach (var b in AllBubbles())
                if (BubblePalette.IsPlayable(b.Color)) s.Add(b.Color);
            return s;
        }

        public bool AnyBubbleBelowDeathLine()
        {
            foreach (var b in AllBubbles())
                if (b.transform.position.y - BubbleRadius < DeathLineY) return true;
            return false;
        }

        public int Count => System.Linq.Enumerable.Count(AllBubbles());

        /// <summary>Push a new row on top; everything shifts down one row.</summary>
        public void AddTopRow(int colorCount)
        {
            var newRow = new Bubble[ColsInRow(0)];
            rows.Insert(0, newRow);
            // fix coords + re-layout odd/even because parities flipped
            for (int r = 0; r < rows.Count; r++)
            {
                int w = ColsInRow(r);
                if (rows[r].Length != w)
                {
                    var old = rows[r];
                    var nw = new Bubble[w];
                    for (int c = 0; c < Mathf.Min(w, old.Length); c++) nw[c] = old[c];
                    for (int c = w; c < old.Length; c++)
                        if (old[c] != null) Destroy(old[c].gameObject); // overflow from parity flip
                    rows[r] = nw;
                }
                for (int c = 0; c < w; c++)
                    if (rows[r][c] != null)
                    {
                        rows[r][c].Row = r; rows[r][c].Col = c;
                        StartCoroutine(MoveTo(rows[r][c].transform, CellToWorld(r, c)));
                    }
            }
            // populate new top row
            int cols0 = ColsInRow(0);
            for (int c = 0; c < cols0; c++)
            {
                var col = (BubbleColor)Random.Range(0, colorCount);
                PlaceBubble(col, 0, c);
            }
        }

        /// <summary>Descend everything by one row visually (used as pressure mechanic).</summary>
        public void DescendStep()
        {
            TopY -= CellH;
            foreach (var b in AllBubbles())
                StartCoroutine(MoveTo(b.transform, CellToWorld(b.Row, b.Col)));
        }

        IEnumerator MoveTo(Transform t, Vector3 target)
        {
            float elapsed = 0f; Vector3 from = t.position;
            while (elapsed < 0.25f)
            {
                elapsed += Time.deltaTime;
                t.position = Vector3.Lerp(from, target, elapsed / 0.25f);
                yield return null;
            }
            t.position = target;
        }

        public void SeedRows(int rowCount, int colorCount, int level = 1)
        {
            int pattern = level % 4;

            for (int r = 0; r < rowCount; r++)
            {
                EnsureRow();
                int cols = ColsInRow(r);
                for (int c = 0; c < cols; c++)
                {
                    // Level architectures for variation across unlimited levels
                    if (pattern == 1 && r >= 3)
                    {
                        // Inverted wedge / diamond gaps
                        int distFromCenter = Mathf.Abs(c - cols / 2);
                        if (distFromCenter > (rowCount - r) + 1) continue;
                    }
                    else if (pattern == 2)
                    {
                        // Staggered columns with strategic gaps
                        if (r > 2 && (c % 3 == 0) && r % 2 == 1) continue;
                    }
                    else if (r > 3 && Random.value < 0.08f)
                    {
                        continue;
                    }

                    // Cluster generation: 50% chance to inherit color from adjacent neighbor
                    // This generates natural match-3 setups that make real commercial bubble shooters so fun!
                    BubbleColor col;
                    Bubble topNb = r > 0 ? Get(r - 1, Mathf.Clamp(c, 0, ColsInRow(r - 1) - 1)) : null;
                    Bubble leftNb = c > 0 ? Get(r, c - 1) : null;

                    if (leftNb != null && Random.value < 0.48f)
                        col = leftNb.Color;
                    else if (topNb != null && Random.value < 0.48f)
                        col = topNb.Color;
                    else
                        col = (BubbleColor)Random.Range(0, colorCount);

                    PlaceBubble(col, r, c);
                }
            }
        }

        public void JiggleNeighbors(int r, int c, Vector3 dir)
        {
            foreach (var n in Neighbors(r, c))
            {
                var b = Get(n.x, n.y);
                if (b != null && !b.IsPopping)
                {
                    StartCoroutine(JiggleBubble(b.transform, CellToWorld(n.x, n.y), dir.normalized * 0.12f));
                }
            }
        }

        IEnumerator JiggleBubble(Transform t, Vector3 origin, Vector3 offset)
        {
            if (t == null) yield break;
            float dur = 0.18f;
            float el = 0f;
            while (el < dur && t != null)
            {
                el += Time.deltaTime;
                float k = Mathf.Sin(el / dur * Mathf.PI * 2f) * (1f - el / dur);
                t.position = origin + offset * k;
                yield return null;
            }
            if (t != null) t.position = origin;
        }

        public void SeedClear() { rows.Clear(); }

        void EnsureRow() { rows.Add(new Bubble[ColsInRow(rows.Count)]); }
    }
}
