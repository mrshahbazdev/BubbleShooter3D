using System;
using System.Collections.Generic;
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

        // --- ProgressManager: persistence round-trip ---
        UnityEngine.PlayerPrefs.DeleteAll();
        var pmGo = new GameObject("Progress");
        var pm = pmGo.AddComponent<ProgressManager>();
        T.Check(pm.UnlockedLevel(GameMode.Classic) == 1, "fresh unlock level = 1");
        pm.UnlockLevel(GameMode.Classic, 3);
        T.Check(pm.UnlockedLevel(GameMode.Classic) == 3, "unlock advances to 3");
        pm.UnlockLevel(GameMode.Classic, 2);
        T.Check(pm.UnlockedLevel(GameMode.Classic) == 3, "unlock never downgrades");
        pm.SetStars(GameMode.Classic, 3, 2);
        T.Check(pm.GetStars(GameMode.Classic, 3) == 2, "stars saved");
        pm.SetStars(GameMode.Classic, 3, 1);
        T.Check(pm.GetStars(GameMode.Classic, 3) == 2, "stars never downgrade");
        T.Check(pm.SetBest(GameMode.TimeAttack, 1500), "first best is a record");
        T.Check(!pm.SetBest(GameMode.TimeAttack, 1000), "lower score is not a record");
        T.Check(pm.GetBest(GameMode.TimeAttack) == 1500, "best score kept");
        pm.SetStars(GameMode.Classic, 1, 3);
        pm.SetStars(GameMode.Classic, 2, 1);
        T.Check(pm.TotalStars(GameMode.Classic) == 6, $"total stars = 6 ({pm.TotalStars(GameMode.Classic)})");
        T.Check(pm.UnlockedLevel(GameMode.TimeAttack) == 1, "time attack always level 1");

        // --- PuzzleLayouts: geometry + content validity ---
        T.Check(PuzzleLayouts.Count == 12, $"12 puzzle layouts ({PuzzleLayouts.Count})");
        bool layoutsOk = true; string layoutErr = "";
        for (int li = 0; li < PuzzleLayouts.Count && layoutsOk; li++)
        {
            var layout = PuzzleLayouts.Layouts[li];
            if (layout.Rows.Length < 6 || layout.Rows.Length > 8) { layoutsOk = false; layoutErr = $"layout {li} rows"; break; }
            if (layout.Par < 6 || layout.Par > 22) { layoutsOk = false; layoutErr = $"layout {li} par"; break; }
            int filled = 0, total = 0;
            for (int r = 0; r < layout.Rows.Length && layoutsOk; r++)
            {
                int expect = (r % 2 == 0) ? 11 : 10;
                if (layout.Rows[r].Length != expect) { layoutsOk = false; layoutErr = $"layout {li} row {r} len {layout.Rows[r].Length}"; break; }
                bool rowEmpty = true;
                foreach (char ch in layout.Rows[r])
                {
                    total++;
                    if (ch == '.') continue;
                    filled++; rowEmpty = false;
                    if (!PuzzleLayouts.CharToColor(ch).HasValue) { layoutsOk = false; layoutErr = $"layout {li} bad char '{ch}'"; break; }
                }
                if (rowEmpty) { layoutsOk = false; layoutErr = $"layout {li} empty row {r}"; break; }
            }
            if ((double)filled / total < 0.4) { layoutsOk = false; layoutErr = $"layout {li} fill"; }
        }
        T.Check(layoutsOk, "all layouts: 6-8 rows, 11/10 cols, valid chars, no empty rows, >=40% fill" + (layoutsOk ? "" : " [" + layoutErr + "]"));
        T.Check(PuzzleLayouts.Get(13).Name == PuzzleLayouts.Layouts[0].Name, "layout Get() wraps around");
        T.Check(!PuzzleLayouts.CharToColor('.').HasValue, "dot maps to empty");
        T.Check(PuzzleLayouts.CharToColor('S') == BubbleColor.Stone, "S maps to stone");
        T.Check(PuzzleLayouts.CountBubbles(PuzzleLayouts.Layouts[0]) > 0, "CountBubbles counts layout bubbles");

        // --- BossLevel: core setup, adjacent damage, direct damage, defeat ---
        var bossGo = new GameObject("Boss");
        var boss = bossGo.AddComponent<BossLevel>();
        var bboard = new GameObject("Board2").AddComponent<BubbleBoard>();
        bboard.Init();
        bboard.SeedClear();
        boss.Setup(bboard, 10);
        T.Check(boss.IsActive, "boss active after setup");
        T.Check(boss.CoreMaxHP == 27 && boss.CoreHP == 27, $"core HP 27 ({boss.CoreHP})");
        T.Check(boss.IsCoreCell(3, 5), "center cell is core");
        T.Check(!boss.IsCoreCell(0, 0), "corner cell is not core");
        boss.DamageAdjacent(new List<Vector2Int> { new Vector2Int(1, 3) });
        T.Check(boss.CoreHP == 26, $"adjacent pop damages core (26, got {boss.CoreHP})");
        boss.DamageCell(2, 4, 3);
        T.Check(!boss.IsCoreCell(2, 4), "destroyed cell leaves core set");
        T.Check(bboard.Get(2, 4) == null, "destroyed cell removed from board");
        T.Check(boss.IsActive, "boss still active with core remaining");
        for (int r = 2; r <= 4; r++)
            for (int c = 4; c <= 6; c++)
                boss.DamageCell(r, c, 3);
        T.Check(!boss.IsActive, "boss deactivates when core destroyed");
        T.Check(boss.CoreHP == 0, "core HP hits 0 on defeat");
        boss.Teardown();

        // --- Boss armor never drops when isolated (no soft-lock) ---
        bboard.SeedClear();
        boss.Setup(bboard, 20);
        T.Check(!boss.IsDroppable(2, 4), "core cell is not droppable");
        T.Check(!boss.IsDroppable(4, 6), "corner core cell is not droppable");
        T.Check(boss.IsDroppable(0, 0), "normal cell is droppable");
        boss.Teardown();
        T.Check(boss.IsDroppable(2, 4), "cells droppable again after teardown");

        Console.WriteLine();
        Console.WriteLine(T.failures == 0 ? "ALL TESTS PASSED" : $"{T.failures} FAILURES");
        return T.failures;
    }
}
