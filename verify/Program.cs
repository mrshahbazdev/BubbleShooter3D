using System;
using BubbleShooter;
using UnityEngine;

static class T
{
    public static int failures;
    public static void Check(bool cond, string name)
    {
        Console.WriteLine((cond ? "PASS" : "FAIL") + "  " + name);
        if (!cond) failures++;
    }
}

class Program
{
    static int Main()
    {
        var boardGo = new GameObject("Board");
        var board = boardGo.AddComponent<BubbleBoard>();
        board.Init();

        // --- seeding ---
        board.SeedRows(6, 4);
        int n = board.Count;
        T.Check(n > 50, $"SeedRows fills board ({n} bubbles)");
        T.Check(board.RowCount == 6, "row count = 6");

        // --- cell positions inside walls ---
        bool inside = true;
        foreach (var b in board.AllBubbles())
            if (b.transform.position.x < board.LeftWallX - 0.01f ||
                b.transform.position.x > board.RightWallX + 0.01f) inside = false;
        T.Check(inside, "all bubbles inside side walls");

        // --- neighbors stay in bounds ---
        bool nbOk = true;
        for (int r = 0; r < board.RowCount; r++)
            for (int c = 0; c < board.ColsInRow(r); c++)
                foreach (var nb in board.Neighbors(r, c))
                    if (nb.x < 0 || nb.x >= board.RowCount || nb.y < 0 || nb.y >= board.ColsInRow(nb.x))
                        nbOk = false;
        T.Check(nbOk, "all neighbors in bounds");

        // --- controlled board: clear and rebuild by hand ---
        board.SeedClear();
        board.PlaceBubble(BubbleColor.Red, 0, 0);
        board.PlaceBubble(BubbleColor.Red, 0, 1);
        board.PlaceBubble(BubbleColor.Blue, 0, 2);
        board.PlaceBubble(BubbleColor.Blue, 2, 5); // hanging lower, connected via? isolated

        // match cluster: red at (1,0) touches (0,0) and (0,1)
        var cluster = MatchSolver.GetMatchCluster(board, 1, 0) ; // empty cell
        T.Check(cluster.Count == 0, "cluster of empty cell = 0");

        board.PlaceBubble(BubbleColor.Red, 1, 0);
        cluster = MatchSolver.GetMatchCluster(board, 1, 0);
        T.Check(cluster.Count == 3, $"match-3 cluster found ({cluster.Count})");

        // --- floating detection: (2,5) blue is not connected to top row ---
        var fl = MatchSolver.GetFloating(board);
        T.Check(fl.Exists(v => v.x == 2 && v.y == 5), "isolated lower bubble is floating");
        T.Check(!fl.Exists(v => v.x == 0 && v.y == 0), "top-row bubble not floating");

        // --- snap cell ---
        var snap = board.FindSnapCell(board.CellToWorld(0, 0));
        T.Check(snap != null, $"snap cell found: {snap}");

        // --- ceiling cell ---
        var ceil = board.FindCeilingCell(new Vector3(0f, board.TopY, 0f));
        T.Check(ceil != null, $"ceiling cell found: {ceil}");

        // --- AddTopRow keeps integrity ---
        board.AddTopRow(4);
        T.Check(board.RowCount == 4, $"AddTopRow -> {board.RowCount} rows");
        bool coordsOk = true;
        foreach (var b in board.AllBubbles())
            if (b.Row < 0 || b.Row >= board.RowCount || b.Col < 0 || b.Col >= board.ColsInRow(b.Row))
                coordsOk = false;
        T.Check(coordsOk, "AddTopRow coords stay in bounds");
        bool noDup = true;
        for (int r = 0; r < board.RowCount; r++)
            for (int c = 0; c < board.ColsInRow(r); c++)
                if (board.Get(r, c) != null && (board.Get(r, c).Row != r || board.Get(r, c).Col != c))
                    noDup = false;
        T.Check(noDup, "grid entries match bubble coords");

        // --- bomb radius ---
        var rad = MatchSolver.GetRadius(board, 1, 1, 1);
        T.Check(rad.Count >= 1, $"bomb radius hits {rad.Count} cells");

        // --- lightning row solver ---
        T.Check(MatchSolver.GetRow(board, 0).Count >= 1, "lightning GetRow returns row cells");

        // --- powerups playable check ---
        T.Check(!BubblePalette.IsPlayable(BubbleColor.Fireball), "fireball not in random standard playable pool");
        T.Check(!BubblePalette.IsPlayable(BubbleColor.Lightning), "lightning not in random standard playable pool");

        // --- empty-cell list only returns empty cells ---
        bool allEmpty = true;
        foreach (var cell in board.EmptyCellsNear(new Vector3(0, board.TopY, 0)))
            if (board.Get(cell.x, cell.y) != null) allEmpty = false;
        T.Check(allEmpty, "EmptyCellsNear returns only empty cells");

        Console.WriteLine();
        Console.WriteLine(T.failures == 0 ? "ALL TESTS PASSED" : $"{T.failures} FAILURES");
        return T.failures;
    }
}
