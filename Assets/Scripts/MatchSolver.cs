using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>Cluster detection and floating-bubble analysis.</summary>
    public static class MatchSolver
    {
        /// <summary>All same-color bubbles connected to (r,c). Rainbow matches any.</summary>
        public static List<Vector2Int> GetMatchCluster(BubbleBoard board, int r, int c)
        {
            var start = board.Get(r, c);
            if (start == null) return new List<Vector2Int>();
            BubbleColor target = start.Color;
            if (target == BubbleColor.Rainbow)
            {
                // A rainbow shot adopts the neighbor color giving the biggest cluster.
                var best = new List<Vector2Int>();
                var candidates = new HashSet<BubbleColor>();
                foreach (var n in board.Neighbors(r, c))
                {
                    var nb = board.Get(n.x, n.y);
                    if (nb != null && BubblePalette.IsPlayable(nb.Color)) candidates.Add(nb.Color);
                }
                foreach (var col in candidates)
                {
                    var cl = Cluster(board, r, c, col);
                    if (cl.Count > best.Count) best = cl;
                }
                return best;
            }
            return Cluster(board, r, c, target);
        }

        static List<Vector2Int> Cluster(BubbleBoard board, int r, int c, BubbleColor target)
        {
            bool Matches(Bubble b) =>
                b != null && (b.Color == target || b.Color == BubbleColor.Rainbow);
            var seen = new HashSet<Vector2Int>();
            var cluster = new List<Vector2Int>();
            var stack = new Stack<Vector2Int>();
            stack.Push(new Vector2Int(r, c));
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (!seen.Add(cur)) continue;
                var b = board.Get(cur.x, cur.y);
                if (!Matches(b)) continue;
                cluster.Add(cur);
                foreach (var n in board.Neighbors(cur.x, cur.y)) stack.Push(n);
            }
            return cluster;
        }

        /// <summary>Bubbles in a radius (bomb power-up).</summary>
        public static List<Vector2Int> GetRadius(BubbleBoard board, int r, int c, int radius)
        {
            var seen = new HashSet<Vector2Int>();
            var list = new List<Vector2Int>();
            var stack = new Stack<(Vector2Int cell, int d)>();
            stack.Push((new Vector2Int(r, c), 0));
            while (stack.Count > 0)
            {
                var (cur, d) = stack.Pop();
                if (!seen.Add(cur) || d > radius) continue;
                if (board.Get(cur.x, cur.y) != null) list.Add(cur);
                foreach (var n in board.Neighbors(cur.x, cur.y)) stack.Push((n, d + 1));
            }
            return list;
        }

        /// <summary>All bubbles across an entire row (lightning power-up).</summary>
        public static List<Vector2Int> GetRow(BubbleBoard board, int r)
        {
            var list = new List<Vector2Int>();
            if (r < 0 || r >= board.RowCount) return list;
            for (int c = 0; c < board.ColsInRow(r); c++)
            {
                if (board.Get(r, c) != null) list.Add(new Vector2Int(r, c));
            }
            return list;
        }

        /// <summary>Bubbles not connected to the top row — these drop.</summary>
        public static List<Vector2Int> GetFloating(BubbleBoard board)
        {
            var connected = new HashSet<Vector2Int>();
            var stack = new Stack<Vector2Int>();
            if (board.RowCount > 0)
                for (int c = 0; c < board.ColsInRow(0); c++)
                    if (board.Get(0, c) != null) stack.Push(new Vector2Int(0, c));
            while (stack.Count > 0)
            {
                var cur = stack.Pop();
                if (!connected.Add(cur)) continue;
                foreach (var n in board.Neighbors(cur.x, cur.y))
                    if (board.Get(n.x, n.y) != null) stack.Push(n);
            }
            var floating = new List<Vector2Int>();
            foreach (var b in board.AllBubbles())
                if (!connected.Contains(new Vector2Int(b.Row, b.Col)))
                    floating.Add(new Vector2Int(b.Row, b.Col));
            return floating;
        }
    }
}
