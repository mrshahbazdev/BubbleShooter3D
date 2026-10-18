using UnityEngine;
using UnityEngine.UI;

namespace BubbleShooter
{
    /// <summary>All HUD and menus built in code — no prefab assets required.</summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager I { get; private set; }

        Text scoreText, levelText, movesText, comboText, bombsText, bannerText;
        GameObject winPanel, losePanel;
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
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
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
            t.supportRichText = false;
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = anchorPoint;
            rt.anchorMax = anchorPoint;
            rt.pivot = anchorPoint;
            rt.sizeDelta = new Vector2(600, size + 20);
            rt.anchoredPosition = pos;
            var shadow = go.AddComponent<Shadow>();
            shadow.effectColor = new Color(0, 0, 0, 0.6f);
            return t;
        }

        GameObject MakePanel(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0f, 0f, 0f, 0.78f);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return go;
        }

        Button MakeButton(Transform parent, string label, Vector2 pos)
        {
            var go = new GameObject("Btn_" + label);
            go.transform.SetParent(parent, false);
            var img = go.AddComponent<Image>();
            img.color = new Color(0.25f, 0.55f, 0.95f);
            var btn = go.AddComponent<Button>();
            var rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(260, 64); rt.anchoredPosition = pos;
            var t = MakeText(go.transform, "Label", label, 26, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(0.5f, 0.5f));
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

            MakeChip(root, topLeft, new Vector2(12, -10), new Vector2(210, 46), new Color(0f, 0f, 0f, 0.55f));
            MakeChip(root, topCenter, new Vector2(0, -10), new Vector2(190, 46), new Color(0f, 0f, 0f, 0.55f));
            MakeChip(root, topRight, new Vector2(-12, -10), new Vector2(210, 46), new Color(0f, 0f, 0f, 0.55f));
            MakeChip(root, bottomLeft, new Vector2(12, 12), new Vector2(180, 40), new Color(0f, 0f, 0f, 0.55f));

            scoreText = MakeText(root, "Score", "SCORE 0", 30, TextAnchor.UpperLeft, new Vector2(26, -17), topLeft);
            scoreText.color = new Color(1f, 0.85f, 0.2f);
            levelText = MakeText(root, "Level", "LEVEL 1", 30, TextAnchor.UpperCenter, new Vector2(0, -17), topCenter);
            levelText.color = new Color(0.4f, 0.9f, 1f);
            movesText = MakeText(root, "Moves", "MOVES 0", 26, TextAnchor.UpperRight, new Vector2(-26, -20), topRight);
            comboText = MakeText(root, "Combo", "", 34, TextAnchor.MiddleCenter, new Vector2(0, -110), center);
            comboText.color = new Color(1f, 0.85f, 0.2f);
            bombsText = MakeText(root, "Bombs", "BOMB [B] x2", 22, TextAnchor.LowerLeft, new Vector2(20, 20), bottomLeft);
            MakeText(root, "Hint", "Aim: mouse   Fire: click   Swap: X   Bomb: B",
                     18, TextAnchor.LowerCenter, new Vector2(0, 18), bottomCenter)
                .color = new Color(1f, 1f, 1f, 0.45f);

            bannerText = MakeText(root, "Banner", "", 54, TextAnchor.MiddleCenter, new Vector2(0, 60), center);

            winPanel = MakePanel(root, "WinPanel");
            MakeText(winPanel.transform, "WinTitle", "LEVEL CLEARED!", 64,
                     TextAnchor.MiddleCenter, new Vector2(0, 140), center).color = new Color(0.3f, 1f, 0.5f);
            var next = MakeButton(winPanel.transform, "NEXT LEVEL", new Vector2(0, -40));
            next.onClick.AddListener(() => { winPanel.SetActive(false); GameManager.I.NextLevel(); });
            winPanel.SetActive(false);

            losePanel = MakePanel(root, "LosePanel");
            MakeText(losePanel.transform, "LoseTitle", "GAME OVER", 64,
                     TextAnchor.MiddleCenter, new Vector2(0, 140), center).color = new Color(1f, 0.35f, 0.35f);
            var retry = MakeButton(losePanel.transform, "RETRY", new Vector2(0, -40));
            retry.onClick.AddListener(() => { losePanel.SetActive(false); GameManager.I.Restart(); });
            losePanel.SetActive(false);
        }

        public void SetHud(int popped = 0, int dropped = 0)
        {
            var g = GameManager.I;
            scoreText.text = $"SCORE {g.Score}";
            levelText.text = $"LEVEL {g.Level}";
            movesText.text = $"MOVES {g.Moves}";
            bombsText.text = $"BOMB [B] x{g.BombsLeft}";
            comboText.text = g.Combo > 1 ? $"COMBO x{g.Combo}!" : "";
            if (dropped > 0) comboText.text += $"  +{dropped} DROPPED";
        }

        public void ShowLevelBanner(int level)
        {
            bannerText.text = $"LEVEL {level}";
            CancelInvoke();
            Invoke(nameof(ClearBanner), 1.6f);
        }
        void ClearBanner() => bannerText.text = "";

        public void ShowWin(int score, int level)
        {
            winPanel.SetActive(true);
            MakeText(winPanel.transform, "Score", $"SCORE {score}", 34,
                     TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(0.5f, 0.5f));
        }

        public void ShowLose(int score, int level)
        {
            losePanel.SetActive(true);
            MakeText(losePanel.transform, "Score", $"SCORE {score}", 34,
                     TextAnchor.MiddleCenter, new Vector2(0, 40), new Vector2(0.5f, 0.5f));
        }
    }
}
