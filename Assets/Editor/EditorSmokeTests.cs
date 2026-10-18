using System;
using UnityEditor;
using UnityEngine;

namespace BubbleShooter.EditorTools
{
    /// <summary>
    /// Headless smoke tests — run via:
    ///   Unity.exe -batchmode -projectPath . -executeMethod BubbleShooter.EditorTools.EditorSmokeTests.RunAll -quit
    /// Results go to the editor log.
    /// </summary>
    public static class EditorSmokeTests
    {
        static int failures;

        static void Check(bool cond, string name)
        {
            Debug.Log((cond ? "PASS  " : "FAIL  ") + name);
            if (!cond) failures++;
        }

        public static void RunAll()
        {
            try
            {
                var boardGo = new GameObject("Board");
                var board = boardGo.AddComponent<BubbleBoard>();
                board.Init();

                board.SeedRows(6, 4);
                int n = board.Count;
                Check(n > 50, $"SeedRows fills board ({n} bubbles)");
                Check(board.RowCount == 6, "row count = 6");

                bool inside = true;
                foreach (var b in board.AllBubbles())
                    if (b.transform.position.x < board.LeftWallX - 0.01f ||
                        b.transform.position.x > board.RightWallX + 0.01f) inside = false;
                Check(inside, "all bubbles inside side walls");

                bool nbOk = true;
                for (int r = 0; r < board.RowCount; r++)
                    for (int c = 0; c < board.ColsInRow(r); c++)
                        foreach (var nb in board.Neighbors(r, c))
                            if (nb.x < 0 || nb.x >= board.RowCount || nb.y < 0 || nb.y >= board.ColsInRow(nb.x))
                                nbOk = false;
                Check(nbOk, "all neighbors in bounds");

                board.SeedClear();
                board.PlaceBubble(BubbleColor.Red, 0, 0);
                board.PlaceBubble(BubbleColor.Red, 0, 1);
                board.PlaceBubble(BubbleColor.Blue, 0, 2);
                board.PlaceBubble(BubbleColor.Blue, 2, 5);

                board.PlaceBubble(BubbleColor.Red, 1, 0);
                var cluster = MatchSolver.GetMatchCluster(board, 1, 0);
                Check(cluster.Count == 3, $"match-3 cluster found ({cluster.Count})");

                var fl = MatchSolver.GetFloating(board);
                Check(fl.Exists(v => v.x == 2 && v.y == 5), "isolated lower bubble is floating");
                Check(!fl.Exists(v => v.x == 0 && v.y == 0), "top-row bubble not floating");

                Check(board.FindSnapCell(board.CellToWorld(0, 0)) != null, "snap cell found");
                Check(board.FindCeilingCell(new Vector3(0f, board.TopY, 0f)) != null, "ceiling cell found");

                board.AddTopRow(4);
                bool coordsOk = true;
                foreach (var b in board.AllBubbles())
                    if (b.Row < 0 || b.Row >= board.RowCount || b.Col < 0 || b.Col >= board.ColsInRow(b.Row))
                        coordsOk = false;
                Check(coordsOk, "AddTopRow coords stay in bounds");

                Check(MatchSolver.GetRadius(board, 1, 1, 1).Count >= 1, "bomb radius hits cells");

                // --- lightning row solver test ---
                Check(MatchSolver.GetRow(board, 0).Count >= 1, "lightning GetRow returns row cells");

                // --- powerup palettes and ghost material ---
                Check(BubblePalette.CreateMaterial(BubbleColor.Fireball) != null, "fireball material created");
                Check(BubblePalette.CreateMaterial(BubbleColor.Lightning) != null, "lightning material created");
                Check(BubblePalette.CreateGhostMaterial(BubbleColor.Blue) != null, "ghost aim material created");
                Check(!BubblePalette.IsPlayable(BubbleColor.Fireball), "fireball is non-playable special");
                Check(!BubblePalette.IsPlayable(BubbleColor.Lightning), "lightning is non-playable special");

                bool allEmpty = true;
                foreach (var cell in board.EmptyCellsNear(new Vector3(0, board.TopY, 0)))
                    if (board.Get(cell.x, cell.y) != null) allEmpty = false;
                Check(allEmpty, "EmptyCellsNear returns only empty cells");

                Debug.Log(failures == 0 ? "SMOKETEST ALL PASSED" : $"SMOKETEST {failures} FAILURES");
                EditorApplication.Exit(failures);
            }
            catch (Exception e)
            {
                Debug.LogError("SMOKETEST EXCEPTION: " + e);
                EditorApplication.Exit(99);
            }
        }
    }
}
