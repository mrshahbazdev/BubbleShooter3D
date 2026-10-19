using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// First-time tutorial for Classic level 1: a short sequence of hint popups
    /// that advance as the player performs each action. Skipped on later levels.
    /// </summary>
    public class TutorialManager : MonoBehaviour
    {
        public static TutorialManager I { get; private set; }

        int step = -1; // -1 = inactive
        bool swapSeen, powerupSeen;

        void Awake() { I = this; }

        public bool IsActive => step >= 0;

        public void BeginIfNeeded(GameMode mode, int level)
        {
            if (mode == GameMode.Classic && level == 1 && PlayerPrefs.GetInt("BS3D_TutorialDone", 0) == 0)
            {
                step = 0;
                ShowStep();
            }
            else step = -1;
        }

        void ShowStep()
        {
            if (UIManager.I == null) return;
            switch (step)
            {
                case 0: UIManager.I.ShowTutorialHint("🎯 Move your mouse (or drag) to AIM"); break;
                case 1: UIManager.I.ShowTutorialHint("👆 Click (or lift finger) to FIRE"); break;
                case 2: UIManager.I.ShowTutorialHint("🫧 Match 3+ bubbles of the same color to POP!"); break;
                case 3: UIManager.I.ShowTutorialHint("🔄 Press X to SWAP with the next bubble"); break;
                case 4: UIManager.I.ShowTutorialHint("💣 Try a power-up: B / F / L keys or the buttons below"); break;
            }
        }

        public void OnFired()
        {
            if (step == 0 || step == 1) { step = 2; ShowStep(); }
        }

        public void OnPopped(int count)
        {
            if (step == 2 && count >= 3) { step = 3; ShowStep(); }
        }

        public void OnSwapped()
        {
            if (step == 3) { swapSeen = true; step = 4; ShowStep(); }
        }

        public void OnPowerupUsed()
        {
            if (step == 4) { powerupSeen = true; Complete(); }
        }

        // Gentle auto-advance so nobody gets stuck: firing also clears swap/powerup steps.
        public void OnAnyShot()
        {
            if (step == 3 && swapSeen) { step = 4; ShowStep(); }
            else if (step == 4 && powerupSeen) Complete();
        }

        void Complete()
        {
            step = -1;
            PlayerPrefs.SetInt("BS3D_TutorialDone", 1);
            PlayerPrefs.Save();
            if (UIManager.I != null) UIManager.I.HideTutorialHint();
        }

        public void Cancel()
        {
            step = -1;
            if (UIManager.I != null) UIManager.I.HideTutorialHint();
        }
    }
}
