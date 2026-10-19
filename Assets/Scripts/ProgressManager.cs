using UnityEngine;

namespace BubbleShooter
{
    /// <summary>Game modes: Classic (endless levels), Time Attack (90s score rush), Puzzle (hand-designed layouts).</summary>
    public enum GameMode { Classic, TimeAttack, Puzzle }

    /// <summary>
    /// Persistent progression: unlocked levels per mode, star ratings, best scores.
    /// Stored in PlayerPrefs so it survives sessions with zero assets.
    /// </summary>
    public class ProgressManager : MonoBehaviour
    {
        public static ProgressManager I { get; private set; }

        const string K_UNLOCK = "BS3D_Unlock_";   // + mode -> highest unlocked level
        const string K_STARS = "BS3D_Stars_";     // + mode + "_" + level -> 0..3
        const string K_BEST = "BS3D_Best_";       // + mode -> best score

        void Awake() { I = this; }

        public int UnlockedLevel(GameMode mode)
        {
            // Time Attack has no levels — always "level 1".
            if (mode == GameMode.TimeAttack) return 1;
            return Mathf.Max(1, PlayerPrefs.GetInt(K_UNLOCK + mode, 1));
        }

        public bool IsLevelUnlocked(GameMode mode, int level)
        {
            return level <= UnlockedLevel(mode);
        }

        public void UnlockLevel(GameMode mode, int level)
        {
            if (mode == GameMode.TimeAttack) return;
            int cur = UnlockedLevel(mode);
            if (level > cur)
            {
                PlayerPrefs.SetInt(K_UNLOCK + mode, level);
                PlayerPrefs.Save();
            }
        }

        public int GetStars(GameMode mode, int level)
        {
            return Mathf.Clamp(PlayerPrefs.GetInt(K_STARS + mode + "_" + level, 0), 0, 3);
        }

        /// <summary>Keeps the best star rating earned (never downgrades).</summary>
        public void SetStars(GameMode mode, int level, int stars)
        {
            stars = Mathf.Clamp(stars, 0, 3);
            if (stars > GetStars(mode, level))
            {
                PlayerPrefs.SetInt(K_STARS + mode + "_" + level, stars);
                PlayerPrefs.Save();
            }
        }

        public int TotalStars(GameMode mode)
        {
            int total = 0;
            for (int l = 1; l <= UnlockedLevel(mode); l++) total += GetStars(mode, l);
            return total;
        }

        public int GetBest(GameMode mode)
        {
            return PlayerPrefs.GetInt(K_BEST + mode, 0);
        }

        /// <summary>Returns true when a new record was set.</summary>
        public bool SetBest(GameMode mode, int score)
        {
            if (score > GetBest(mode))
            {
                PlayerPrefs.SetInt(K_BEST + mode, score);
                PlayerPrefs.Save();
                return true;
            }
            return false;
        }

        public void ResetAll()
        {
            PlayerPrefs.DeleteKey(K_UNLOCK + GameMode.Classic);
            PlayerPrefs.DeleteKey(K_UNLOCK + GameMode.Puzzle);
            PlayerPrefs.DeleteKey(K_BEST + GameMode.Classic);
            PlayerPrefs.DeleteKey(K_BEST + GameMode.TimeAttack);
            PlayerPrefs.DeleteKey(K_BEST + GameMode.Puzzle);
            for (int l = 1; l <= 60; l++)
            {
                PlayerPrefs.DeleteKey(K_STARS + GameMode.Classic + "_" + l);
                PlayerPrefs.DeleteKey(K_STARS + GameMode.Puzzle + "_" + l);
            }
            PlayerPrefs.Save();
        }
    }
}
