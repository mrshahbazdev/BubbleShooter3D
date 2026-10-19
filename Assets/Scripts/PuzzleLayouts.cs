using System;

namespace BubbleShooter
{
    /// <summary>
    /// Hand-designed puzzle layouts for Puzzle Mode.
    ///
    /// Char legend (one char = one hex cell):
    ///   '.'  empty cell
    ///   'R'  Red bubble        'G'  Green bubble      'B'  Blue bubble
    ///   'Y'  Yellow bubble     'P'  Purple bubble     'O'  Orange bubble
    ///   'S'  Stone blocker (cannot be matched; must be dropped or destroyed)
    ///
    /// Board geometry: even-index rows hold 11 cells, odd-index rows hold 10 cells
    /// (staggered hex grid). Every layout uses 6-8 rows, is at least 40% filled,
    /// has no fully-empty row, is never a single color, and always contains at
    /// least one immediate match-3 opportunity (three adjacent same-color cells).
    ///
    /// Difficulty ramps 1 -> 12: layouts 1-3 use 3 colors with obvious clusters,
    /// 4-8 use 4 colors with gaps (stone blockers from layout 6), 9-12 use
    /// 5 colors with stone blockers and tricky shapes. Par ~= bubbles / 4.
    /// </summary>
    public struct PuzzleLayout
    {
        public string Name;
        public string[] Rows;
        public int Par;
    }

    public static class PuzzleLayouts
    {
        public static readonly PuzzleLayout[] Layouts = new PuzzleLayout[]
        {
            // ---- Tier 1: 3 colors, obvious clusters ---------------------------------
            new PuzzleLayout
            {
                Name = "BABY STEPS",
                Par = 7,
                Rows = new string[]
                {
                    "RRR......BB",  // 11
                    "GGG.....BB",   // 10
                    "BBB......RR",  // 11
                    "RRR.....GG",   // 10
                    "GGG......BB",  // 11
                    "BBB.....RR",   // 10
                }
            },
            new PuzzleLayout
            {
                Name = "TRIPLE PLAY",
                Par = 9,
                Rows = new string[]
                {
                    "RRR..GGG...",  // 11
                    ".BBB..RRR.",   // 10
                    "GGG...BBB..",  // 11
                    "..RRR.GGG.",   // 10
                    "BBB....RRR.",  // 11
                    ".GGG...BBB",   // 10
                }
            },
            new PuzzleLayout
            {
                Name = "COLOR CASCADE",
                Par = 9,
                Rows = new string[]
                {
                    "RRGGBBRRGGB",  // 11
                    "RGGBBRRGGB",   // 10
                    "GGBBRRGGB..",  // 11
                    "....RRR...",   // 10
                    ".....GG....",  // 11
                    "....BB....",   // 10
                }
            },
            // ---- Tier 2: 4 colors, gaps and holes -----------------------------------
            new PuzzleLayout
            {
                Name = "FOUR ALARM",
                Par = 13,
                Rows = new string[]
                {
                    "RRRYYYGGBBB",  // 11
                    "RRYYGGBBYY",   // 10
                    "YYGGBBRRRYY",  // 11
                    "..GGBBYY..",   // 10
                    "...RRRYYY..",  // 11
                    "..BBGGYY..",   // 10
                    "....YY.....",  // 11
                }
            },
            new PuzzleLayout
            {
                Name = "SWISS CHEESE",
                Par = 12,
                Rows = new string[]
                {
                    "RRGYYBBRRGG",  // 11
                    "GGRRBBYYGG",   // 10
                    "BBYYRRGGBBR",  // 11
                    ".R.G.B.Y.R",   // 10
                    "..Y..B..G..",  // 11
                    ".R...Y...B",   // 10
                    "...GGGYYY..",  // 11
                }
            },
            new PuzzleLayout
            {
                Name = "STONE COLD",
                Par = 16,
                Rows = new string[]
                {
                    "RRRGGGBBBYY",  // 11
                    "RRGGBBYYRR",   // 10
                    "GGSSBBYYRRG",  // 11
                    "SSYYRRGGBB",   // 10
                    "YYRRGGBB.SS",  // 11
                    "..GGBBYY..",   // 10
                    "...RRRYYY..",  // 11
                }
            },
            new PuzzleLayout
            {
                Name = "THE GAUNTLET",
                Par = 16,
                Rows = new string[]
                {
                    "YYRRGGBBYYR",  // 11
                    "RRGGBBYYRR",   // 10
                    "GGBBYYRRSSG",  // 11
                    "BBYYRRSSGG",   // 10
                    "YYRRSSGGBB.",  // 11
                    "RRSSGGBB..",   // 10
                    "..YYYBBB...",  // 11
                }
            },
            new PuzzleLayout
            {
                Name = "CHECKMATE",
                Par = 15,
                Rows = new string[]
                {
                    "R.Y.G.B.R.Y",  // 11
                    ".G.B.Y.R.G",   // 10
                    "B.R.Y.G.B.R",  // 11
                    ".Y.R.B.Y.R",   // 10
                    "SSRRGGBBYY.",  // 11
                    "RRGGBBYYSS",   // 10
                    "GGBBYYRRSS.",  // 11
                    "BBYYYRRRGG",   // 10
                }
            },
            // ---- Tier 3: 5 colors, stone blockers, tricky shapes --------------------
            new PuzzleLayout
            {
                Name = "PURPLE REIGN",
                Par = 20,
                Rows = new string[]
                {
                    "PPPRRRGGGBB",  // 11
                    "PPRRGGBBYY",   // 10
                    "RRGGBBYYPPR",  // 11
                    "GGBBYYPPSS",   // 10
                    "BBYYPPSSRRG",  // 11
                    "YYPPSSRRGG",   // 10
                    "PPSSRRGGBB.",  // 11
                    "SSRRGGBB..",   // 10
                }
            },
            new PuzzleLayout
            {
                Name = "DIAMOND HEIST",
                Par = 19,
                Rows = new string[]
                {
                    "...PPPPP...",  // 11
                    "..PPPRRPPP",   // 10
                    ".PPRRGGRRPP",  // 11
                    "PPRRGGBBRR",   // 10
                    "RRGGBBYYPPR",  // 11
                    "SSRRYYPPSS",   // 10
                    ".SSYYYBBSS.",  // 11
                    "..SSBBBSS.",   // 10
                }
            },
            new PuzzleLayout
            {
                Name = "POCKET ACES",
                Par = 16,
                Rows = new string[]
                {
                    "RRRPPPYYYBB",  // 11
                    "RRRPPPYYBB",   // 10
                    "..SSSSSS...",  // 11
                    "GGGSSSSBBB",   // 10
                    "GGG..S..BBB",  // 11
                    "YY..SS..GG",   // 10
                    "YYY.....GGG",  // 11
                    "YYY..S..GG",   // 10
                }
            },
            new PuzzleLayout
            {
                Name = "FINAL BOSS",
                Par = 21,
                Rows = new string[]
                {
                    "PPPRRRGGGBB",  // 11
                    "PPRRGGBBYY",   // 10
                    "RRGGBBYYPPS",  // 11
                    "GGBBYYPPSS",   // 10
                    "BBYYPPSSRRG",  // 11
                    "YYPPSSRRGG",   // 10
                    "PPSSRRGGBBY",  // 11
                    "SSRRGGBBYY",   // 10
                }
            },
        };

        public static int Count => Layouts.Length;

        public static PuzzleLayout Get(int level) => Layouts[(Math.Max(1, level) - 1) % Layouts.Length];

        /// <summary>
        /// Maps a layout char to a bubble color. '.' (empty) and anything
        /// unrecognized map to null.
        /// </summary>
        public static BubbleColor? CharToColor(char ch)
        {
            return ch switch
            {
                'R' => BubbleColor.Red,
                'G' => BubbleColor.Green,
                'B' => BubbleColor.Blue,
                'Y' => BubbleColor.Yellow,
                'P' => BubbleColor.Purple,
                'O' => BubbleColor.Orange,
                'S' => BubbleColor.Stone,
                _ => null
            };
        }

        /// <summary>Counts non-'.' (non-empty) cells in a layout.</summary>
        public static int CountBubbles(PuzzleLayout l)
        {
            int n = 0;
            if (l.Rows == null) return 0;
            foreach (var row in l.Rows)
            {
                if (row == null) continue;
                foreach (char ch in row)
                    if (ch != '.') n++;
            }
            return n;
        }
    }
}
