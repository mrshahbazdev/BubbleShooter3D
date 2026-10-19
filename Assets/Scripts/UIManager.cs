using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace BubbleShooter
{
    /// <summary>
    /// Generates crisp procedural 9-sliced rounded rectangles at runtime with zero assets.
    /// </summary>
    public static class UISpriteFactory
    {
        static Sprite cardSprite;
        static Sprite pillSprite;
        static Sprite buttonSprite;
        static Sprite glowFrameSprite;
        static Sprite circleSprite;

        public static Sprite GetCardSprite() =>
            cardSprite ??= CreateRoundedRect(64, 64, 16, new Color(0.06f, 0.08f, 0.14f, 0.92f), new Color(0.25f, 0.55f, 0.85f, 0.55f), 2);

        public static Sprite GetPillSprite() =>
            pillSprite ??= CreateRoundedRect(64, 32, 16, new Color(0.04f, 0.06f, 0.10f, 0.88f), new Color(0.35f, 0.75f, 1f, 0.45f), 1);

        public static Sprite GetButtonSprite() =>
            buttonSprite ??= CreateRoundedRect(64, 64, 16, Color.white, new Color(1f, 1f, 1f, 0.35f), 2);

        public static Sprite GetGlowFrameSprite() =>
            glowFrameSprite ??= CreateRoundedRect(64, 64, 16, new Color(1f, 1f, 1f, 0.08f), Color.white, 3);

        public static Sprite GetCircleSprite() =>
            circleSprite ??= CreateRoundedRect(64, 64, 31, Color.white, new Color(1f, 1f, 1f, 0.35f), 2);

        public static Sprite CreateRoundedRect(int width, int height, int radius, Color fill, Color border, int borderWidth = 0)
        {
            var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
            tex.filterMode = FilterMode.Bilinear;
            float r = radius;
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    float dx = (x < r) ? r - x : (x > width - 1 - r) ? x - (width - 1 - r) : 0f;
                    float dy = (y < r) ? r - y : (y > height - 1 - r) ? y - (height - 1 - r) : 0f;
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);

                    float alpha = Mathf.Clamp01(r - dist + 0.5f);
                    if (alpha <= 0f)
                    {
                        tex.SetPixel(x, y, Color.clear);
                    }
                    else
                    {
                        bool isBorder = borderWidth > 0 && (dist >= r - borderWidth || x < borderWidth || x >= width - borderWidth || y < borderWidth || y >= height - borderWidth);
                        Color c = isBorder ? border : fill;
                        c.a *= alpha;
                        tex.SetPixel(x, y, c);
                    }
                }
            }
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(radius, radius, radius, radius));
        }
    }

    /// <summary>
    /// Tactile physical micro-interactions: squash on click, expand on hover, audio tick feedback.
    /// </summary>
    public class UIButtonFeedback : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        Vector3 baseScale = Vector3.one;
        Coroutine animRoutine;

        void Awake() => baseScale = transform.localScale;

        public void OnPointerEnter(PointerEventData e)
        {
            AnimateScale(baseScale * 1.06f);
            if (AudioManager.I != null) AudioManager.I.Play("tick");
        }

        public void OnPointerExit(PointerEventData e) => AnimateScale(baseScale);

        public void OnPointerDown(PointerEventData e) => AnimateScale(baseScale * 0.92f);

        public void OnPointerUp(PointerEventData e) => AnimateScale(baseScale);

        void AnimateScale(Vector3 target)
        {
            if (animRoutine != null) StopCoroutine(animRoutine);
            if (gameObject.activeInHierarchy) animRoutine = StartCoroutine(CoScale(target));
            else transform.localScale = target;
        }

        IEnumerator CoScale(Vector3 target)
        {
            Vector3 from = transform.localScale;
            float t = 0f;
            while (t < 0.08f)
            {
                t += Time.unscaledDeltaTime;
                transform.localScale = Vector3.Lerp(from, target, t / 0.08f);
                yield return null;
            }
            transform.localScale = target;
        }
    }

    /// <summary>
    /// Modern glassmorphic arcade HUD, main menu with mode + level select,
    /// animated sequenced star reveal, tactile powerup cards,
    /// and dynamic rolling score count-up.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        Text scoreText, highScoreText, levelText, modeText, movesText, pressureText, timerText, bannerText;
        Text comboCalloutText, tutorialText;
        Text winScoreText, winConservedText, winHighScoreText, loseScoreText, muteButtonText;

        Text[] starSlots;

        Button btnBomb, btnFire, btnZap, btnSwap, btnPause;
        Button btnFreeze, btnMulti, btnShield;
        Text btnBombText, btnFireText, btnZapText;
        Text btnFreezeText, btnMultiText, btnShieldText;
        Image btnBombImg, btnFireImg, btnZapImg;
        Image btnFreezeImg, btnMultiImg, btnShieldImg;
        Image btnBombGlow, btnFireGlow, btnZapGlow;
        Image btnFreezeGlow, btnMultiGlow, btnShieldGlow;

        GameObject winPanel, losePanel, pausePanel, menuPanel;
        GameObject winCard, loseCard, pauseCard;
        GameObject levelGridRoot;
        Text[] modeBestTexts = new Text[3];
        GameMode menuMode = GameMode.Classic;

        GameObject bossBarBg, bossBarFill, bossLabelObj;
        Font font;

        Coroutine comboRoutine;
        Coroutine winTallyRoutine;

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
            if (FindAnyObjectByType<EventSystem>() == null)
#else
            if (FindObjectOfType<EventSystem>() == null)
#endif
            {
                var es = new GameObject("EventSystem");
                es.AddComponent<EventSystem>();
                es.AddComponent<StandaloneInputModule>();
            }
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1280, 800);
            go.AddComponent<GraphicRaycaster>();
            return c;
        }

        Text MakeText(Transform parent, string name, string content, int size,
                      TextAnchor anchor, Vector2 pos, Vector2 anchorPoint, int width = 600)
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
            rt.sizeDelta = new Vector2(width, size + 20);
            rt.anchoredPosition = pos;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.75f);
            shadow.effectDistance = new Vector2(1.5f, -1.5f);
            return t;
        }

        GameObject MakeCard(Transform parent, string name, Vector2 anchorPoint, Vector2 pos, Vector2 size, Sprite sprite = null, Color? tint = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = sprite ?? UISpriteFactory.GetCardSprite();
            img.type = Image.Type.Sliced;
            img.color = tint ?? Color.white;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorPoint; rt.anchorMax = anchorPoint; rt.pivot = anchorPoint;
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            return go;
        }

        Button MakeButton(Transform parent, string label, Vector2 pos, Vector2 size, Color bgColor, int fontSize = 20)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = UISpriteFactory.GetButtonSprite();
            img.type = Image.Type.Sliced;
            img.color = bgColor;
            img.raycastTarget = true;
            var btn = go.AddComponent<Button>();
            go.AddComponent<UIButtonFeedback>();
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size; rt.anchoredPosition = pos;
            var t = MakeText(go.transform, "Label", label, fontSize, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(0.5f, 0.5f), (int)size.x);
            t.GetComponent<RectTransform>().sizeDelta = rt.sizeDelta;
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => AudioManager.I.Play("click"));
            return btn;
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

            // ==================== TOP STATUS PILLS (Glassmorphic) ====================
            // 1. Left Score & HighScore Card
            MakeCard(root, "ScoreCard", topLeft, new Vector2(20, -16), new Vector2(250, 64), UISpriteFactory.GetCardSprite());
            scoreText = MakeText(root, "Score", "SCORE 0", 24, TextAnchor.UpperLeft, new Vector2(36, -22), topLeft);
            scoreText.color = new Color(1f, 0.88f, 0.25f);
            highScoreText = MakeText(root, "HighScore", "🏆 BEST 0", 15, TextAnchor.UpperLeft, new Vector2(36, -50), topLeft);
            highScoreText.color = new Color(0.85f, 0.88f, 0.95f, 0.8f);

            // 2. Center Level & Tension Meter
            MakeCard(root, "CenterCard", topCenter, new Vector2(0, -16), new Vector2(310, 64), UISpriteFactory.GetCardSprite());
            levelText = MakeText(root, "Level", "LEVEL 1", 23, TextAnchor.UpperCenter, new Vector2(0, -22), topCenter);
            levelText.color = new Color(0.35f, 0.92f, 1f);
            modeText = MakeText(root, "Mode", "CLASSIC", 13, TextAnchor.UpperCenter, new Vector2(0, -48), topCenter, 300);
            modeText.color = new Color(0.75f, 0.85f, 1f, 0.75f);
            pressureText = MakeText(root, "Pressure", "CEILING [● ● ○]", 16, TextAnchor.UpperCenter, new Vector2(0, -68), topCenter);
            pressureText.color = new Color(1f, 0.70f, 0.25f);
            timerText = MakeText(root, "Timer", "⏱ 1:30", 26, TextAnchor.UpperCenter, new Vector2(0, -70), topCenter);
            timerText.color = new Color(0.4f, 1f, 0.6f);
            timerText.gameObject.SetActive(false);

            // Boss HP bar (hidden unless boss level)
            bossBarBg = MakeCard(root, "BossBarBg", topCenter, new Vector2(0, -104), new Vector2(320, 20), UISpriteFactory.GetPillSprite(), new Color(0.1f, 0.05f, 0.06f, 0.9f));
            var fillGo = new GameObject("BossBarFill");
            fillGo.transform.SetParent(bossBarBg.transform, false);
            var fillImg = fillGo.AddComponent<Image>();
            fillImg.sprite = UISpriteFactory.GetPillSprite();
            fillImg.type = Image.Type.Sliced;
            fillImg.color = new Color(1f, 0.25f, 0.2f);
            fillImg.raycastTarget = false;
            var frt = fillGo.GetComponent<RectTransform>();
            frt.anchorMin = new Vector2(0f, 0.5f); frt.anchorMax = new Vector2(0f, 0.5f); frt.pivot = new Vector2(0f, 0.5f);
            frt.sizeDelta = new Vector2(312, 12);
            frt.anchoredPosition = new Vector2(4, 0);
            bossBarFill = fillGo;
            var bossLabel = MakeText(root, "BossLabel", "👹 PRESSURE CORE", 13, TextAnchor.UpperCenter, new Vector2(0, -122), topCenter, 300);
            bossLabel.color = new Color(1f, 0.4f, 0.35f);
            bossLabelObj = bossLabel.gameObject;
            bossBarBg.SetActive(false);
            bossLabelObj.SetActive(false);

            // 3. Right Shots Left & Pause Card
            MakeCard(root, "ShotsCard", topRight, new Vector2(-20, -16), new Vector2(260, 64), UISpriteFactory.GetCardSprite());
            movesText = MakeText(root, "Moves", "🎯 SHOTS 30/30", 21, TextAnchor.UpperLeft, new Vector2(-215, -34), topRight, 160);
            movesText.color = new Color(1f, 0.92f, 0.40f);

            // Pause Button (Round circular pill on right)
            btnPause = MakeButton(root, "⏸", new Vector2(-46, -48), new Vector2(44, 44), new Color(0.18f, 0.24f, 0.35f), 22);
            btnPause.GetComponent<Image>().sprite = UISpriteFactory.GetCircleSprite();
            btnPause.GetComponent<RectTransform>().anchorMin = topRight;
            btnPause.GetComponent<RectTransform>().anchorMax = topRight;
            btnPause.GetComponent<RectTransform>().pivot = topRight;
            btnPause.onClick.AddListener(() => GameManager.I.TogglePause());

            // Center Dynamic Combo Callout Banner
            comboCalloutText = MakeText(root, "ComboCallout", "", 38, TextAnchor.MiddleCenter, new Vector2(0, 140), center);
            comboCalloutText.color = new Color(1f, 0.85f, 0.2f);
            comboCalloutText.gameObject.SetActive(false);

            // Tutorial hint line
            tutorialText = MakeText(root, "TutorialHint", "", 20, TextAnchor.LowerCenter, new Vector2(0, 92), bottomCenter, 800);
            tutorialText.color = new Color(0.55f, 0.95f, 1f);
            tutorialText.gameObject.SetActive(false);

            // ==================== BOTTOM ACTION BAR (Arcade Powerups) ====================
            // Swap Bubble Action Button (Bottom Left)
            btnSwap = MakeButton(root, "🔄 SWAP [X]", new Vector2(20, 22), new Vector2(148, 52), new Color(0.16f, 0.42f, 0.72f), 18);
            btnSwap.GetComponent<RectTransform>().anchorMin = bottomLeft;
            btnSwap.GetComponent<RectTransform>().anchorMax = bottomLeft;
            btnSwap.GetComponent<RectTransform>().pivot = bottomLeft;
            btnSwap.onClick.AddListener(() => GameManager.I.Shooter.Swap());

            // 💣 BOMB CARD
            btnBomb = MakeButton(root, "💣 BOMB [B]\nx2", new Vector2(-380, 22), new Vector2(138, 52), new Color(0.38f, 0.16f, 0.20f), 16);
            SetupPowerupButton(btnBomb, bottomRight, out btnBombImg, out btnBombText, out btnBombGlow, new Color(1f, 0.3f, 0.3f));
            btnBomb.onClick.AddListener(() => GameManager.I.Shooter.ToggleBomb());

            // 🔥 FIREBALL CARD
            btnFire = MakeButton(root, "🔥 FIRE [F]\nx1", new Vector2(-230, 22), new Vector2(138, 52), new Color(0.48f, 0.24f, 0.08f), 16);
            SetupPowerupButton(btnFire, bottomRight, out btnFireImg, out btnFireText, out btnFireGlow, new Color(1f, 0.6f, 0.1f));
            btnFire.onClick.AddListener(() => GameManager.I.Shooter.ToggleFireball());

            // ⚡ LIGHTNING CARD
            btnZap = MakeButton(root, "⚡ ZAP [L]\nx1", new Vector2(-80, 22), new Vector2(138, 52), new Color(0.12f, 0.45f, 0.58f), 16);
            SetupPowerupButton(btnZap, bottomRight, out btnZapImg, out btnZapText, out btnZapGlow, new Color(0.2f, 0.9f, 1f));
            btnZap.onClick.AddListener(() => GameManager.I.Shooter.ToggleLightning());

            // ❄ FREEZE CARD
            btnFreeze = MakeButton(root, "❄ FREEZE [V]\nx1", new Vector2(-530, 22), new Vector2(138, 52), new Color(0.14f, 0.38f, 0.55f), 16);
            SetupPowerupButton(btnFreeze, bottomRight, out btnFreezeImg, out btnFreezeText, out btnFreezeGlow, new Color(0.5f, 0.9f, 1f));
            btnFreeze.onClick.AddListener(() => GameManager.I.Shooter.ToggleFreeze());

            // ✸ MULTISHOT CARD
            btnMulti = MakeButton(root, "✸ MULTI [M]\nx1", new Vector2(-680, 22), new Vector2(138, 52), new Color(0.42f, 0.22f, 0.55f), 16);
            SetupPowerupButton(btnMulti, bottomRight, out btnMultiImg, out btnMultiText, out btnMultiGlow, new Color(0.8f, 0.4f, 1f));
            btnMulti.onClick.AddListener(() => GameManager.I.Shooter.ToggleMultishot());

            // 🛡 SHIELD CARD
            btnShield = MakeButton(root, "🛡 SHIELD [H]\nx1", new Vector2(-830, 22), new Vector2(138, 52), new Color(0.16f, 0.45f, 0.28f), 16);
            SetupPowerupButton(btnShield, bottomRight, out btnShieldImg, out btnShieldText, out btnShieldGlow, new Color(0.35f, 1f, 0.55f));
            btnShield.onClick.AddListener(() => GameManager.I.Shooter.ToggleShield());

            MakeText(root, "Hint", "Aim: Mouse / Drag  |  Fire: Click / Release  |  [X] Swap  [B] [F] [L] [V] [M] [H] Power-ups  |  [ESC] Pause",
                     14, TextAnchor.LowerCenter, new Vector2(-70, 24), bottomCenter)
                .color = new Color(1f, 1f, 1f, 0.45f);

            bannerText = MakeText(root, "Banner", "", 52, TextAnchor.MiddleCenter, new Vector2(0, 60), center);

            // ==================== MODAL PANELS (Menu, Win, Lose, Pause) ====================
            BuildMainMenu(root, center);
            BuildWinPanel(root, center);
            BuildLosePanel(root, center);
            BuildPausePanel(root, center);
        }

        void SetupPowerupButton(Button btn, Vector2 anchor, out Image img, out Text txt, out Image glow, Color glowColor)
        {
            var rt = btn.GetComponent<RectTransform>();
            rt.anchorMin = anchor; rt.anchorMax = anchor; rt.pivot = anchor;
            img = btn.GetComponent<Image>();
            txt = btn.GetComponentInChildren<Text>();
            glow = AddGlowRing(btn.transform, glowColor);
        }

        Image AddGlowRing(Transform parent, Color col)
        {
            var go = new GameObject("GlowRing");
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.sprite = UISpriteFactory.GetGlowFrameSprite();
            img.type = Image.Type.Sliced;
            img.color = col;
            img.raycastTarget = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.sizeDelta = new Vector2(10, 10);
            rt.anchoredPosition = Vector2.zero;
            go.SetActive(false);
            return img;
        }

        // ==================== MAIN MENU ====================

        void BuildMainMenu(Transform root, Vector2 center)
        {
            menuPanel = new GameObject("MainMenu");
            menuPanel.transform.SetParent(root, false);
            var bg = menuPanel.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.03f, 0.06f, 0.94f);
            bg.raycastTarget = true;
            var rt = menuPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            MakeText(menuPanel.transform, "Title", "🫧 BUBBLE SHOOTER 3D", 64, TextAnchor.MiddleCenter, new Vector2(0, 300), center, 900)
                .color = new Color(0.45f, 0.9f, 1f);
            var sub = MakeText(menuPanel.transform, "Subtitle", "ADVANCED ARCADE EDITION", 20, TextAnchor.MiddleCenter, new Vector2(0, 252), center, 600);
            sub.color = new Color(1f, 0.85f, 0.25f, 0.9f);

            // Mode buttons
            string[] modeLabels = { "🕹 CLASSIC", "⏱ TIME ATTACK", "🧩 PUZZLE" };
            Color[] modeColors = {
                new Color(0.16f, 0.45f, 0.80f),
                new Color(0.75f, 0.35f, 0.12f),
                new Color(0.35f, 0.55f, 0.18f)
            };
            float[] modeX = { -260f, 0f, 260f };
            for (int i = 0; i < 3; i++)
            {
                int idx = i;
                var mb = MakeButton(menuPanel.transform, modeLabels[i] + "\nBEST 0", new Vector2(modeX[i], 150), new Vector2(230, 78), modeColors[i], 19);
                modeBestTexts[i] = mb.GetComponentInChildren<Text>();
                mb.onClick.AddListener(() => OnModeSelected((GameMode)idx));
            }

            MakeText(menuPanel.transform, "LevelLabel", "— SELECT LEVEL —", 18, TextAnchor.MiddleCenter, new Vector2(0, 70), center, 400)
                .color = new Color(0.8f, 0.88f, 1f, 0.8f);

            levelGridRoot = new GameObject("LevelGrid");
            levelGridRoot.transform.SetParent(menuPanel.transform, false);
            var grt = levelGridRoot.AddComponent<RectTransform>();
            grt.anchorMin = center; grt.anchorMax = center; grt.pivot = center;
            grt.sizeDelta = new Vector2(560, 170);
            grt.anchoredPosition = new Vector2(0, -60);

            var muteBtn = MakeButton(menuPanel.transform, "🔊 AUDIO", new Vector2(0, -260), new Vector2(220, 48), new Color(0.28f, 0.34f, 0.48f), 18);
            var menuMuteText = muteBtn.GetComponentInChildren<Text>();
            muteBtn.onClick.AddListener(() => {
                AudioManager.I.ToggleMute();
                menuMuteText.text = AudioManager.I.IsMuted ? "🔇 MUTED" : "🔊 AUDIO";
            });

            var tip = MakeText(menuPanel.transform, "Tip", "Tip: match 4+ for avalanches · bank shots off walls · every 10th level is a BOSS 👹", 15, TextAnchor.MiddleCenter, new Vector2(0, -310), center, 900);
            tip.color = new Color(1f, 1f, 1f, 0.5f);

            menuPanel.SetActive(false);
        }

        void OnModeSelected(GameMode mode)
        {
            menuMode = mode;
            if (mode == GameMode.TimeAttack)
            {
                GameManager.I.StartLevel(1, mode);
                return;
            }
            RefreshLevelGrid();
        }

        void RefreshLevelGrid()
        {
            foreach (Transform child in levelGridRoot.transform) Destroy(child.gameObject);
            var pm = ProgressManager.I;
            int unlocked = pm != null ? pm.UnlockedLevel(menuMode) : 1;
            int show = 12;
            for (int i = 0; i < show; i++)
            {
                int lvl = i + 1;
                bool isUnlocked = lvl <= unlocked;
                int stars = pm != null ? pm.GetStars(menuMode, lvl) : 0;
                string starStr = stars == 0 ? "☆☆☆" : new string('★', stars) + new string('☆', 3 - stars);
                var b = MakeButton(levelGridRoot.transform, isUnlocked ? $"{lvl}\n{starStr}" : $"{lvl}\n🔒",
                    new Vector2(-275 + (i % 6) * 110, 45 - (i / 6) * 95), new Vector2(100, 85),
                    isUnlocked ? new Color(0.18f, 0.38f, 0.62f) : new Color(0.12f, 0.14f, 0.18f), 17);
                if (!isUnlocked) b.interactable = false;
                else
                {
                    int lv = lvl;
                    b.onClick.AddListener(() => GameManager.I.StartLevel(lv, menuMode));
                }
            }
        }

        public void ShowMainMenu()
        {
            if (menuPanel == null) return;
            menuMode = GameMode.Classic;
            var pm = ProgressManager.I;
            string[] labels = { "🕹 CLASSIC", "⏱ TIME ATTACK", "🧩 PUZZLE" };
            for (int i = 0; i < 3; i++)
            {
                int best = pm != null ? pm.GetBest((GameMode)i) : 0;
                int stars = (i == 1) ? 0 : (pm != null ? pm.TotalStars((GameMode)i) : 0);
                string extra = i == 1 ? $"BEST {best:N0}" : $"BEST {best:N0} · {stars}★";
                if (modeBestTexts[i] != null) modeBestTexts[i].text = $"{labels[i]}\n{extra}";
            }
            RefreshLevelGrid();
            menuPanel.SetActive(true);
            ShowBossBar(false);
            HideTutorialHint();
        }

        public void HideMainMenu()
        {
            if (menuPanel != null) menuPanel.SetActive(false);
        }

        // ==================== WIN / LOSE / PAUSE ====================

        void BuildWinPanel(Transform root, Vector2 center)
        {
            winPanel = new GameObject("WinPanel");
            winPanel.transform.SetParent(root, false);
            var bg = winPanel.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.08f, 0.82f);
            var rt = winPanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            winCard = MakeCard(winPanel.transform, "WinCard", center, Vector2.zero, new Vector2(520, 560),
                               UISpriteFactory.GetCardSprite(), new Color(0.08f, 0.12f, 0.20f, 0.96f));

            MakeText(winCard.transform, "WinTitle", "VICTORY!", 54, TextAnchor.MiddleCenter, new Vector2(0, 215), center)
                .color = new Color(0.35f, 1f, 0.65f);

            // 3 Animated Star Slots
            starSlots = new Text[3];
            float[] starX = { -95f, 0f, 95f };
            for (int i = 0; i < 3; i++)
            {
                var s = MakeText(winCard.transform, "Star_" + i, "★", 62, TextAnchor.MiddleCenter, new Vector2(starX[i], 135), center, 90);
                s.color = new Color(0.25f, 0.28f, 0.35f, 0.5f); // Unearned grey
                starSlots[i] = s;
            }

            winScoreText = MakeText(winCard.transform, "WinScore", "SCORE 0", 34, TextAnchor.MiddleCenter, new Vector2(0, 50), center);
            winScoreText.color = new Color(1f, 0.88f, 0.2f);

            winConservedText = MakeText(winCard.transform, "WinConserved", "", 18, TextAnchor.MiddleCenter, new Vector2(0, 2), center);
            winConservedText.color = new Color(0.4f, 0.9f, 0.55f);

            winHighScoreText = MakeText(winCard.transform, "WinHigh", "BEST 0", 16, TextAnchor.MiddleCenter, new Vector2(0, -32), center);
            winHighScoreText.color = new Color(0.85f, 0.88f, 0.95f, 0.7f);

            var next = MakeButton(winCard.transform, "NEXT LEVEL ▶", new Vector2(0, -105), new Vector2(280, 56), new Color(0.18f, 0.72f, 0.38f), 22);
            next.onClick.AddListener(() => { winPanel.SetActive(false); GameManager.I.NextLevel(); });

            var winRestart = MakeButton(winCard.transform, "REPLAY ↺", new Vector2(0, -175), new Vector2(280, 48), new Color(0.24f, 0.32f, 0.48f), 19);
            winRestart.onClick.AddListener(() => { winPanel.SetActive(false); GameManager.I.Restart(); });

            var winMenu = MakeButton(winCard.transform, "🏠 MENU", new Vector2(0, -240), new Vector2(280, 44), new Color(0.30f, 0.24f, 0.42f), 18);
            winMenu.onClick.AddListener(() => { winPanel.SetActive(false); GameManager.I.ShowMenu(); });

            winPanel.SetActive(false);
        }

        void BuildLosePanel(Transform root, Vector2 center)
        {
            losePanel = new GameObject("LosePanel");
            losePanel.transform.SetParent(root, false);
            var bg = losePanel.AddComponent<Image>();
            bg.color = new Color(0.08f, 0.02f, 0.02f, 0.85f);
            var rt = losePanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            loseCard = MakeCard(losePanel.transform, "LoseCard", center, Vector2.zero, new Vector2(480, 480),
                                UISpriteFactory.GetCardSprite(), new Color(0.16f, 0.06f, 0.08f, 0.96f));

            MakeText(loseCard.transform, "LoseTitle", "GAME OVER", 54, TextAnchor.MiddleCenter, new Vector2(0, 165), center)
                .color = new Color(1f, 0.32f, 0.32f);

            loseScoreText = MakeText(loseCard.transform, "LoseScore", "OUT OF SHOTS!\n\nSCORE 0", 22, TextAnchor.MiddleCenter, new Vector2(0, 55), center);

            var retry = MakeButton(loseCard.transform, "TRY AGAIN ↺", new Vector2(0, -50), new Vector2(260, 58), new Color(0.88f, 0.24f, 0.24f), 22);
            retry.onClick.AddListener(() => { losePanel.SetActive(false); GameManager.I.Restart(); });

            var loseMenu = MakeButton(loseCard.transform, "🏠 MENU", new Vector2(0, -125), new Vector2(260, 50), new Color(0.30f, 0.24f, 0.42f), 19);
            loseMenu.onClick.AddListener(() => { losePanel.SetActive(false); GameManager.I.ShowMenu(); });

            losePanel.SetActive(false);
        }

        void BuildPausePanel(Transform root, Vector2 center)
        {
            pausePanel = new GameObject("PausePanel");
            pausePanel.transform.SetParent(root, false);
            var bg = pausePanel.AddComponent<Image>();
            bg.color = new Color(0.02f, 0.04f, 0.08f, 0.85f);
            var rt = pausePanel.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;

            pauseCard = MakeCard(pausePanel.transform, "PauseCard", center, Vector2.zero, new Vector2(440, 470),
                                 UISpriteFactory.GetCardSprite(), new Color(0.09f, 0.12f, 0.20f, 0.96f));

            MakeText(pauseCard.transform, "PauseTitle", "GAME PAUSED", 48, TextAnchor.MiddleCenter, new Vector2(0, 165), center)
                .color = new Color(0.85f, 0.92f, 1f);

            var resumeBtn = MakeButton(pauseCard.transform, "RESUME ▶", new Vector2(0, 70), new Vector2(260, 54), new Color(0.18f, 0.62f, 0.95f), 22);
            resumeBtn.onClick.AddListener(() => GameManager.I.TogglePause());

            var muteBtn = MakeButton(pauseCard.transform, "AUDIO: ON", new Vector2(0, -3), new Vector2(260, 52), new Color(0.28f, 0.34f, 0.48f), 20);
            muteButtonText = muteBtn.GetComponentInChildren<Text>();
            muteBtn.onClick.AddListener(() => {
                AudioManager.I.ToggleMute();
                UpdateMuteButtonText();
            });

            var pauseRestart = MakeButton(pauseCard.transform, "RESTART LEVEL ↺", new Vector2(0, -75), new Vector2(260, 50), new Color(0.72f, 0.28f, 0.28f), 19);
            pauseRestart.onClick.AddListener(() => {
                GameManager.I.TogglePause();
                GameManager.I.Restart();
            });

            var pauseMenu = MakeButton(pauseCard.transform, "🏠 QUIT TO MENU", new Vector2(0, -143), new Vector2(260, 48), new Color(0.30f, 0.24f, 0.42f), 18);
            pauseMenu.onClick.AddListener(() => {
                GameManager.I.TogglePause();
                GameManager.I.ShowMenu();
            });

            pausePanel.SetActive(false);
            UpdateMuteButtonText();
        }

        void UpdateMuteButtonText()
        {
            if (muteButtonText != null)
                muteButtonText.text = AudioManager.I.IsMuted ? "🔇 AUDIO: MUTED" : "🔊 AUDIO: ACTIVE";
        }

        // ==================== RUNTIME HUD ====================

        public void SetHud(int popped = 0, int dropped = 0)
        {
            var g = GameManager.I;
            if (g == null) return;

            scoreText.text = $"SCORE {g.Score:N0}";
            highScoreText.text = $"🏆 BEST {g.HighScore:N0}";
            levelText.text = g.Mode == GameMode.TimeAttack ? "TIME ATTACK" : $"LEVEL {g.Level}";
            modeText.text = g.Mode == GameMode.Puzzle ? $"PUZZLE · PAR {g.PuzzlePar}" : g.ModeLabel;

            bool isTA = g.Mode == GameMode.TimeAttack;
            timerText.gameObject.SetActive(isTA);
            pressureText.gameObject.SetActive(!isTA);

            if (isTA)
            {
                movesText.text = "🎯 ∞ SHOTS";
                movesText.color = new Color(0.5f, 1f, 0.65f);
            }
            else
            {
                int remaining = g.MovesRemaining;
                if (remaining <= 5)
                {
                    movesText.text = $"⚠️ SHOTS {remaining}/{g.MaxMoves}";
                    movesText.color = new Color(1f, 0.25f, 0.25f);
                }
                else
                {
                    movesText.text = $"🎯 SHOTS {remaining}/{g.MaxMoves}";
                    movesText.color = new Color(1f, 0.88f, 0.35f);
                }

                int shotsLeft = g.ShotsUntilDescend;
                int total = g.ShotsPerDescend;
                string dots = "";
                for (int i = 0; i < total; i++)
                    dots += (i < total - shotsLeft) ? "● " : "○ ";

                if (g.FreezeShotsLeft > 0)
                {
                    pressureText.text = $"❄ FROZEN x{g.FreezeShotsLeft}";
                    pressureText.color = new Color(0.55f, 0.9f, 1f);
                }
                else if (shotsLeft <= 1)
                {
                    pressureText.text = g.ShieldActive ? $"🛡 DROP BLOCKED! [{dots.Trim()}]" : $"⚠️ DROP NEXT! [{dots.Trim()}]";
                    pressureText.color = g.ShieldActive ? new Color(0.4f, 1f, 0.55f) : new Color(1f, 0.3f, 0.3f);
                }
                else
                {
                    pressureText.text = $"CEILING [{dots.Trim()}]";
                    pressureText.color = new Color(1f, 0.65f, 0.3f);
                }
            }

            // Power-up button updates with pulsing glowing rings
            var shooter = g.Shooter;
            bool bombArmed = shooter != null && shooter.ActivePowerup == PowerupType.Bomb;
            bool fireArmed = shooter != null && shooter.ActivePowerup == PowerupType.Fireball;
            bool zapArmed = shooter != null && shooter.ActivePowerup == PowerupType.Lightning;
            bool multiArmed = shooter != null && shooter.ActivePowerup == PowerupType.Multishot;

            btnBombText.text = $"💣 BOMB [B]\nx{g.BombsLeft}";
            btnBombImg.color = bombArmed ? new Color(0.95f, 0.25f, 0.25f) : new Color(0.38f, 0.16f, 0.20f);
            btnBombGlow.gameObject.SetActive(bombArmed);

            btnFireText.text = $"🔥 FIRE [F]\nx{g.FireballsLeft}";
            btnFireImg.color = fireArmed ? new Color(1.0f, 0.5f, 0.05f) : new Color(0.48f, 0.24f, 0.08f);
            btnFireGlow.gameObject.SetActive(fireArmed);

            btnZapText.text = $"⚡ ZAP [L]\nx{g.LightningsLeft}";
            btnZapImg.color = zapArmed ? new Color(0.2f, 0.85f, 1f) : new Color(0.12f, 0.45f, 0.58f);
            btnZapGlow.gameObject.SetActive(zapArmed);

            btnFreezeText.text = $"❄ FREEZE [V]\nx{g.FreezesLeft}";
            btnFreezeImg.color = g.FreezeShotsLeft > 0 ? new Color(0.45f, 0.8f, 1f) : new Color(0.14f, 0.38f, 0.55f);
            btnFreezeGlow.gameObject.SetActive(g.FreezeShotsLeft > 0);

            btnMultiText.text = $"✸ MULTI [M]\nx{g.MultishotsLeft}";
            btnMultiImg.color = multiArmed ? new Color(0.75f, 0.4f, 1f) : new Color(0.42f, 0.22f, 0.55f);
            btnMultiGlow.gameObject.SetActive(multiArmed);

            btnShieldText.text = $"🛡 SHIELD [H]\nx{g.ShieldsLeft}";
            btnShieldImg.color = g.ShieldActive ? new Color(0.35f, 0.95f, 0.5f) : new Color(0.16f, 0.45f, 0.28f);
            btnShieldGlow.gameObject.SetActive(g.ShieldActive);

            // Boss HP bar
            var boss = BossLevel.I;
            if (boss != null && boss.IsActive && bossBarBg.activeSelf)
            {
                var frt = bossBarFill.GetComponent<RectTransform>();
                frt.sizeDelta = new Vector2(312f * Mathf.Clamp01(boss.CoreHpFrac), 12f);
            }

            // Dynamic Combo streak callout
            if (g.Combo > 1)
            {
                ShowComboCallout(g.Combo, dropped);
            }
        }

        public void SetTimer(float seconds)
        {
            if (timerText == null) return;
            int s = Mathf.Max(0, Mathf.CeilToInt(seconds));
            timerText.text = $"⏱ {s / 60}:{s % 60:D2}";
            timerText.color = seconds < 10f ? new Color(1f, 0.3f, 0.3f) : new Color(0.4f, 1f, 0.6f);
        }

        public void ShowBossBar(bool show)
        {
            if (bossBarBg != null) bossBarBg.SetActive(show);
            if (bossLabelObj != null) bossLabelObj.SetActive(show);
            if (show && bossBarFill != null)
            {
                var frt = bossBarFill.GetComponent<RectTransform>();
                frt.sizeDelta = new Vector2(312f, 12f);
            }
        }

        public void ShowTutorialHint(string text)
        {
            if (tutorialText == null) return;
            tutorialText.text = text;
            tutorialText.gameObject.SetActive(true);
        }

        public void HideTutorialHint()
        {
            if (tutorialText != null) tutorialText.gameObject.SetActive(false);
        }

        void ShowComboCallout(int combo, int dropped)
        {
            if (comboRoutine != null) StopCoroutine(comboRoutine);
            comboRoutine = StartCoroutine(CoComboCallout(combo, dropped));
        }

        IEnumerator CoComboCallout(int combo, int dropped)
        {
            comboCalloutText.gameObject.SetActive(true);
            string title = combo switch
            {
                2 => "⚡ 2x COMBO!",
                3 => "🔥 3x ON FIRE!",
                4 => "💥 4x MEGA STRIKE!",
                _ => $"👑 {combo}x UNSTOPPABLE!"
            };
            Color col = combo switch
            {
                2 => new Color(0.3f, 0.85f, 1f),
                3 => new Color(1f, 0.65f, 0.1f),
                4 => new Color(0.95f, 0.35f, 1f),
                _ => new Color(1f, 0.85f, 0.15f)
            };
            if (dropped > 0) title += $"\n+{dropped} DROPPED!";
            comboCalloutText.text = title;
            comboCalloutText.color = col;

            // Punch scale bounce
            float t = 0f;
            while (t < 0.2f)
            {
                t += Time.deltaTime;
                float k = Mathf.Sin(t / 0.2f * Mathf.PI * 0.5f);
                comboCalloutText.transform.localScale = Vector3.Lerp(Vector3.one * 0.5f, Vector3.one * 1.35f, k);
                yield return null;
            }
            t = 0f;
            while (t < 0.15f)
            {
                t += Time.deltaTime;
                comboCalloutText.transform.localScale = Vector3.Lerp(Vector3.one * 1.35f, Vector3.one, t / 0.15f);
                yield return null;
            }
            yield return new WaitForSeconds(1.2f);
            t = 0f;
            while (t < 0.25f)
            {
                t += Time.deltaTime;
                var c = comboCalloutText.color; c.a = 1f - (t / 0.25f);
                comboCalloutText.color = c;
                yield return null;
            }
            comboCalloutText.gameObject.SetActive(false);
            comboRoutine = null;
        }

        public void ShowLevelBanner(string text, Color? accent = null)
        {
            bannerText.text = text;
            bannerText.color = accent ?? new Color(0.4f, 0.9f, 1f);
            CancelInvoke(nameof(ClearBanner));
            Invoke(nameof(ClearBanner), 2.4f);
        }

        public void ShowLevelBanner(int level) => ShowLevelBanner($"LEVEL {level}");

        void ClearBanner() => bannerText.text = "";

        public void ShowPause(bool paused)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(paused);
                if (paused && pauseCard != null) StartCoroutine(CoCardPop(pauseCard.transform));
                UpdateMuteButtonText();
            }
        }

        public void ShowWin(int score, int level, int stars = 3, bool isTimeAttack = false, bool newRecord = false)
        {
            winPanel.SetActive(true);
            if (winTallyRoutine != null) StopCoroutine(winTallyRoutine);
            winTallyRoutine = StartCoroutine(CoWinSequence(score, level, stars, isTimeAttack, newRecord));
        }

        IEnumerator CoWinSequence(int targetScore, int level, int stars, bool isTimeAttack, bool newRecord)
        {
            AudioManager.I.Play("whoosh");
            yield return StartCoroutine(CoCardPop(winCard.transform));

            // Reset star slots
            for (int i = 0; i < 3; i++)
            {
                starSlots[i].color = new Color(0.25f, 0.28f, 0.35f, 0.45f);
                starSlots[i].transform.localScale = Vector3.one * 0.85f;
            }

            var g = GameManager.I;
            if (isTimeAttack)
            {
                winConservedText.text = newRecord
                    ? "<color=#FBBF24>🏆 NEW TIME ATTACK RECORD!</color>"
                    : "<color=#4ADE80>⏱ TIME'S UP — SCORE ATTACK COMPLETE!</color>";
            }
            else
            {
                int conserved = g != null ? g.MovesRemaining : 0;
                int bonus = conserved * 50;
                winConservedText.text = $"<color=#4ADE80>SHOTS CONSERVED: +{conserved} (x50 = +{bonus:N0} PTS)</color>";
            }
            winHighScoreText.text = $"🏆 HIGH SCORE: {GameManager.I.HighScore:N0}";

            // Sequenced Star Drops
            for (int i = 0; i < stars; i++)
            {
                yield return new WaitForSecondsRealtime(0.22f);
                var slot = starSlots[i];
                slot.color = new Color(1f, 0.88f, 0.18f);
                AudioManager.I.PlayStarChime(i);

                // Punch scale animation on star
                float t = 0f;
                while (t < 0.2f)
                {
                    t += Time.unscaledDeltaTime;
                    float k = Mathf.Sin(t / 0.2f * Mathf.PI * 0.5f);
                    slot.transform.localScale = Vector3.Lerp(Vector3.one * 1.8f, Vector3.one * 1.0f, k);
                    yield return null;
                }
                slot.transform.localScale = Vector3.one;
            }

            // Rolling score count-up (Odometer)
            float elapsed = 0f;
            float duration = 0.85f;
            int startScore = Mathf.Max(0, targetScore - 1200);

            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                int current = (int)Mathf.Lerp(startScore, targetScore, elapsed / duration);
                winScoreText.text = $"SCORE {current:N0}";
                AudioManager.I.Play("tick");
                yield return new WaitForSecondsRealtime(0.04f);
            }
            winScoreText.text = $"SCORE {targetScore:N0}";
            winTallyRoutine = null;
        }

        public void ShowLose(int score, int level)
        {
            losePanel.SetActive(true);
            AudioManager.I.Play("whoosh");
            if (loseCard != null) StartCoroutine(CoCardPop(loseCard.transform));

            string reason = GameManager.I.MovesRemaining <= 0 ? "⚠️ OUT OF SHOTS!" : "⚠️ BUBBLES CROSSED THE LINE!";
            loseScoreText.text = $"<color=#F87171>{reason}</color>\n\nFINAL SCORE: {score:N0}\n🏆 BEST: {GameManager.I.HighScore:N0}";
        }

        IEnumerator CoCardPop(Transform card)
        {
            float t = 0f;
            while (t < 0.25f)
            {
                t += Time.unscaledDeltaTime;
                float p = t / 0.25f;
                // Elastic bounce curve: 0.5 -> 1.08 -> 1.0
                float s = Mathf.Sin(p * Mathf.PI * 0.5f);
                card.localScale = Vector3.Lerp(Vector3.one * 0.55f, Vector3.one * 1.05f, s);
                yield return null;
            }
            t = 0f;
            while (t < 0.12f)
            {
                t += Time.unscaledDeltaTime;
                card.localScale = Vector3.Lerp(Vector3.one * 1.05f, Vector3.one, t / 0.12f);
                yield return null;
            }
            card.localScale = Vector3.one;
        }
    }
}
