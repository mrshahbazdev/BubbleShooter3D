using UnityEngine;
using UnityEngine.UI;

namespace BubbleShooter
{
    /// <summary>
    /// Rich procedural HUD, on-screen power-up controls, pause menu, and animated victory panels.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        Text scoreText, highScoreText, levelText, movesText, pressureText, comboText, bannerText;
        Text winScoreText, winStarsText, loseScoreText, muteButtonText;
        Button btnBomb, btnFire, btnZap, btnSwap, btnPause;
        Text btnBombText, btnFireText, btnZapText;
        Image btnBombImg, btnFireImg, btnZapImg;

        GameObject winPanel, losePanel, pausePanel;
        Font font;

        void Awake()
        {
            I = this;
            font = GetFont();
            BuildHud();
        }

        static Font GetFont()
        {
            try { return Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { return Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }

        Canvas MakeCanvas()
        {
            var go = new GameObject("Canvas");
            var c = go.AddComponent<Canvas>();
#if UNITY_2023_1_OR_NEWER || UNITY_6000_0_OR_NEWER
            if (FindAnyObjectByType<UnityEngine.EventSystems.EventSystem>() == null)
#else
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
#endif
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<UnityEngine.EventSystems.EventSystem>();
                es.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            }
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        Text MakeText(Transform parent, string name, string content, int size,
                      TextAnchor anchor, Vector2 pos, Vector2 anchorPoint)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var t = go.AddComponent<Text>();
            t.font = font; t.text = content; t.fontSize = size;
            t.alignment = anchor; t.color = Color.white;
            t.supportRichText = true;
            t.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorPoint;
            rt.anchorMax = anchorPoint;
            rt.pivot = anchorPoint;
            rt.sizeDelta = new Vector2(600, size + 20);
            rt.anchoredPosition = pos;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.65f);
            return t;
        }

        GameObject MakePanel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.04f, 0.05f, 0.09f, 0.88f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, Color bgColor)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = bgColor;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var t = MakeText(go.transform, "Label", label, 20, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(0.5f, 0.5f));
            t.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => AudioManager.I.Play("click"));
            return btn;
        }

        GameObject MakeChip(Transform parent, Vector2 anchorPoint, Vector2 pos, Vector2 size, Color tint)
        {
            var go = new GameObject("Chip");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = tint;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorPoint; rt.anchorMax = anchorPoint; rt.pivot = anchorPoint;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            return go;
        }

        void BuildHud()
        {
            var canvas = MakeCanvas();
            var root = canvas.transform;

            var topLeft = new Vector2(0f, 1f);
            var topCenter = new Vector2(0.5f, 1f);
            var topRight = new Vector2(1f, 1f);
            var center = new Vector2(0.5f, 0.5f);
            var bottomLeft = new Vector2(0f, 0f);
            var bottomCenter = new Vector2(0.5f, 0f);
            var bottomRight = new Vector2(1f, 0f);

            // Top Status Chips
            MakeChip(root, topLeft, new Vector2(16, -12), new Vector2(240, 56), new Color(0f, 0f, 0f, 0.65f));
            MakeChip(root, topCenter, new Vector2(0, -12), new Vector2(260, 56), new Color(0f, 0f, 0f, 0.65f));
            MakeChip(root, topRight, new Vector2(-16, -12), new Vector2(240, 56), new Color(0f, 0f, 0f, 0.65f));

            scoreText = MakeText(root, "Score", "SCORE 0", 24, TextAnchor.UpperLeft, new Vector2(28, -16), topLeft);
            scoreText.color = new Color(1f, 0.85f, 0.2f);
            highScoreText = MakeText(root, "HighScore", "BEST 0", 16, TextAnchor.UpperLeft, new Vector2(28, -42), topLeft);
            highScoreText.color = new Color(0.8f, 0.8f, 0.8f, 0.7f);

            levelText = MakeText(root, "Level", "LEVEL 1", 24, TextAnchor.UpperCenter, new Vector2(0, -16), topCenter);
            levelText.color = new Color(0.4f, 0.9f, 1f);
            pressureText = MakeText(root, "Pressure", "CEILING IN 4", 16, TextAnchor.UpperCenter, new Vector2(0, -42), topCenter);
            pressureText.color = new Color(1f, 0.65f, 0.3f);

            movesText = MakeText(root, "Moves", "MOVES 0", 22, TextAnchor.UpperRight, new Vector2(-80, -26), topRight);

            // Pause Button (top right inside chip)
            btnPause = MakeButton(root, "||", new Vector2(-42, -40), new Vector2(40, 40), new Color(0.2f, 0.25f, 0.35f));
            btnPause.GetComponent<RectTransform>().anchorMin = topRight;
            btnPause.GetComponent<RectTransform>().anchorMax = topRight;
            btnPause.GetComponent<RectTransform>().pivot = topRight;
            btnPause.onClick.AddListener(() => GameManager.I.TogglePause());

            comboText = MakeText(root, "Combo", "", 36, TextAnchor.MiddleCenter, new Vector2(0, -110), center);
            comboText.color = new Color(1f, 0.85f, 0.2f);

            // Power-Up Action Bar (Bottom Right & Center)
            btnSwap = MakeButton(root, "SWAP [X]", new Vector2(16, 20), new Vector2(130, 48), new Color(0.18f, 0.45f, 0.75f));
            btnSwap.GetComponent<RectTransform>().anchorMin = bottomLeft;
            btnSwap.GetComponent<RectTransform>().anchorMax = bottomLeft;
            btnSwap.GetComponent<RectTransform>().pivot = bottomLeft;
            btnSwap.onClick.AddListener(() => GameManager.I.Shooter.Swap());

            btnBomb = MakeButton(root, "BOMB [B] x2", new Vector2(-360, 20), new Vector2(140, 48), new Color(0.35f, 0.18f, 0.22f));
            btnBomb.GetComponent<RectTransform>().anchorMin = bottomRight;
            btnBomb.GetComponent<RectTransform>().anchorMax = bottomRight;
            btnBomb.GetComponent<RectTransform>().pivot = bottomRight;
            btnBombImg = btnBomb.GetComponent<Image>();
            btnBombText = btnBomb.GetComponentInChildren<Text>();
            btnBomb.onClick.AddListener(() => GameManager.I.Shooter.ToggleBomb());

            btnFire = MakeButton(root, "FIRE [F] x1", new Vector2(-210, 20), new Vector2(140, 48), new Color(0.45f, 0.22f, 0.08f));
            btnFire.GetComponent<RectTransform>().anchorMin = bottomRight;
            btnFire.GetComponent<RectTransform>().anchorMax = bottomRight;
            btnFire.GetComponent<RectTransform>().pivot = bottomRight;
            btnFireImg = btnFire.GetComponent<Image>();
            btnFireText = btnFire.GetComponentInChildren<Text>();
            btnFire.onClick.AddListener(() => GameManager.I.Shooter.ToggleFireball());

            btnZap = MakeButton(root, "ZAP [L] x1", new Vector2(-60, 20), new Vector2(140, 48), new Color(0.12f, 0.42f, 0.52f));
            btnZap.GetComponent<RectTransform>().anchorMin = bottomRight;
            btnZap.GetComponent<RectTransform>().anchorMax = bottomRight;
            btnZap.GetComponent<RectTransform>().pivot = bottomRight;
            btnZapImg = btnZap.GetComponent<Image>();
            btnZapText = btnZap.GetComponentInChildren<Text>();
            btnZap.onClick.AddListener(() => GameManager.I.Shooter.ToggleLightning());

            MakeText(root, "Hint", "Aim: Mouse | Fire: Left Click | Shortcuts: [X] Swap, [B] Bomb, [F] Fireball, [L] Zap, [ESC] Pause",
                     15, TextAnchor.LowerCenter, new Vector2(-60, 20), bottomCenter)
                .color = new Color(1f, 1f, 1f, 0.45f);

            bannerText = MakeText(root, "Banner", "", 54, TextAnchor.MiddleCenter, new Vector2(0, 60), center);

            // Win Panel with Stars
            winPanel = MakePanel(root, "WinPanel");
            MakeText(winPanel.transform, "WinTitle", "LEVEL CLEARED!", 58,
                     TextAnchor.MiddleCenter, new Vector2(0, 160), center).color = new Color(0.3f, 1f, 0.5f);
            winStarsText = MakeText(winPanel.transform, "WinStars", "★★★", 52,
                                    TextAnchor.MiddleCenter, new Vector2(0, 85), center);
            winStarsText.color = new Color(1f, 0.85f, 0.15f);
            winScoreText = MakeText(winPanel.transform, "WinScore", "SCORE 0", 28,
                                    TextAnchor.MiddleCenter, new Vector2(0, 20), center);
            var next = MakeButton(winPanel.transform, "NEXT LEVEL", new Vector2(0, -60), new Vector2(240, 54), new Color(0.2f, 0.7f, 0.35f));
            next.onClick.AddListener(() => { winPanel.SetActive(false); GameManager.I.NextLevel(); });
            var winRestart = MakeButton(winPanel.transform, "REPLAY LEVEL", new Vector2(0, -130), new Vector2(240, 46), new Color(0.25f, 0.3f, 0.45f));
            winRestart.onClick.AddListener(() => { winPanel.SetActive(false); GameManager.I.Restart(); });
            winPanel.SetActive(false);

            // Lose Panel
            losePanel = MakePanel(root, "LosePanel");
            MakeText(losePanel.transform, "LoseTitle", "GAME OVER", 58,
                     TextAnchor.MiddleCenter, new Vector2(0, 140), center).color = new Color(1f, 0.35f, 0.35f);
            loseScoreText = MakeText(losePanel.transform, "LoseScore", "SCORE 0", 28,
                                     TextAnchor.MiddleCenter, new Vector2(0, 40), center);
            var retry = MakeButton(losePanel.transform, "TRY AGAIN", new Vector2(0, -50), new Vector2(240, 56), new Color(0.85f, 0.25f, 0.25f));
            retry.onClick.AddListener(() => { losePanel.SetActive(false); GameManager.I.Restart(); });
            losePanel.SetActive(false);

            // Pause Panel
            pausePanel = MakePanel(root, "PausePanel");
            MakeText(pausePanel.transform, "PauseTitle", "GAME PAUSED", 54,
                     TextAnchor.MiddleCenter, new Vector2(0, 140), center).color = new Color(0.9f, 0.9f, 1f);
            var resumeBtn = MakeButton(pausePanel.transform, "RESUME", new Vector2(0, 30), new Vector2(240, 52), new Color(0.2f, 0.6f, 0.9f));
            resumeBtn.onClick.AddListener(() => GameManager.I.TogglePause());
            var muteBtn = MakeButton(pausePanel.transform, "AUDIO: ON", new Vector2(0, -40), new Vector2(240, 52), new Color(0.35f, 0.4f, 0.5f));
            muteButtonText = muteBtn.GetComponentInChildren<Text>();
            muteBtn.onClick.AddListener(() => {
                AudioManager.I.ToggleMute();
                UpdateMuteButtonText();
            });
            var pauseRestart = MakeButton(pausePanel.transform, "RESTART", new Vector2(0, -110), new Vector2(240, 52), new Color(0.75f, 0.3f, 0.3f));
            pauseRestart.onClick.AddListener(() => {
                GameManager.I.TogglePause();
                GameManager.I.Restart();
            });
            pausePanel.SetActive(false);
            UpdateMuteButtonText();
        }

        void UpdateMuteButtonText()
        {
            if (muteButtonText != null)
                muteButtonText.text = AudioManager.I.IsMuted ? "AUDIO: MUTED" : "AUDIO: ACTIVE";
        }

        public void SetHud(int popped = 0, int dropped = 0)
        {
            var g = GameManager.I;
            if (g == null) return;

            scoreText.text = $"SCORE {g.Score:N0}";
            highScoreText.text = $"BEST {g.HighScore:N0}";
            levelText.text = $"LEVEL {g.Level}";
            movesText.text = $"MOVES {g.Moves}";

            int shotsLeft = g.ShotsUntilDescend;
            pressureText.text = shotsLeft <= 1 ? "⚠️ CEILING DROP NEXT!" : $"CEILING IN {shotsLeft}";
            pressureText.color = shotsLeft <= 1 ? new Color(1f, 0.3f, 0.3f) : new Color(1f, 0.65f, 0.3f);

            // Power-up button updates
            var shooter = g.Shooter;
            bool bombArmed = shooter != null && shooter.ActivePowerup == PowerupType.Bomb;
            bool fireArmed = shooter != null && shooter.ActivePowerup == PowerupType.Fireball;
            bool zapArmed = shooter != null && shooter.ActivePowerup == PowerupType.Lightning;

            btnBombText.text = $"BOMB [B] x{g.BombsLeft}";
            btnBombImg.color = bombArmed ? new Color(0.95f, 0.25f, 0.25f) : new Color(0.35f, 0.18f, 0.22f);

            btnFireText.text = $"FIRE [F] x{g.FireballsLeft}";
            btnFireImg.color = fireArmed ? new Color(1.0f, 0.5f, 0.05f) : new Color(0.45f, 0.22f, 0.08f);

            btnZapText.text = $"ZAP [L] x{g.LightningsLeft}";
            btnZapImg.color = zapArmed ? new Color(0.2f, 0.85f, 1f) : new Color(0.12f, 0.42f, 0.52f);

            comboText.text = g.Combo > 1 ? $"COMBO x{g.Combo}!" : "";
            if (dropped > 0) comboText.text += $"  +{dropped} DROPPED!";
        }

        public void ShowLevelBanner(string text, Color? accent = null)
        {
            bannerText.text = text;
            if (accent.HasValue) bannerText.color = accent.Value;
            else bannerText.color = new Color(0.4f, 0.9f, 1f);
            CancelInvoke();
            Invoke(nameof(ClearBanner), 2.2f);
        }

        public void ShowLevelBanner(int level) => ShowLevelBanner($"LEVEL {level}");

        void ClearBanner() => bannerText.text = "";

        public void ShowPause(bool paused)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(paused);
                UpdateMuteButtonText();
            }
        }

        public void ShowWin(int score, int level, int stars = 3)
        {
            winPanel.SetActive(true);
            winStarsText.text = stars switch
            {
                3 => "★ ★ ★",
                2 => "★ ★ ☆",
                _ => "★ ☆ ☆"
            };
            winScoreText.text = $"SCORE {score:N0}\nHIGH {GameManager.I.HighScore:N0}";
        }

        public void ShowLose(int score, int level)
        {
            losePanel.SetActive(true);
            loseScoreText.text = $"SCORE {score:N0}\nHIGH {GameManager.I.HighScore:N0}";
        }
    }
}
