using UnityEngine;

namespace BubbleShooter
{
    public enum BubbleColor { Red, Green, Blue, Yellow, Purple, Orange, Bomb, Rainbow }

    public static class BubblePalette
    {
        public static readonly Color[] Colors =
        {
            new Color(0.95f, 0.22f, 0.27f), // Red
            new Color(0.22f, 0.80f, 0.35f), // Green
            new Color(0.20f, 0.50f, 0.98f), // Blue
            new Color(0.98f, 0.82f, 0.15f), // Yellow
            new Color(0.65f, 0.30f, 0.90f), // Purple
            new Color(0.98f, 0.55f, 0.12f), // Orange
            new Color(0.15f, 0.15f, 0.18f), // Bomb
            new Color(0.90f, 0.90f, 0.95f), // Rainbow
        };

        public static bool IsPlayable(BubbleColor c) => (int)c <= (int)BubbleColor.Orange;

        public static Material CreateMaterial(BubbleColor c)
        {
            var m = new Material(Shader.Find("Standard"));
            Color baseCol = Colors[(int)c];
            m.color = baseCol;
            m.SetFloat("_Glossiness", 0.85f);
            m.SetFloat("_Metallic", 0.1f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", baseCol * 0.35f);
            return m;
        }
    }
}
