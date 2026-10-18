using UnityEngine;

namespace BubbleShooter
{
    public enum BubbleColor { Red, Green, Blue, Yellow, Purple, Orange, Bomb, Rainbow }

    public static class BubblePalette
    {
        public static readonly Color[] Colors =
        {
            new Color(0.98f, 0.18f, 0.32f), // Red
            new Color(0.15f, 0.90f, 0.42f), // Green
            new Color(0.15f, 0.55f, 1.00f), // Blue
            new Color(1.00f, 0.80f, 0.10f), // Yellow
            new Color(0.72f, 0.28f, 1.00f), // Purple
            new Color(1.00f, 0.52f, 0.08f), // Orange
            new Color(0.15f, 0.15f, 0.18f), // Bomb
            new Color(0.90f, 0.90f, 0.95f), // Rainbow
        };

        public static bool IsPlayable(BubbleColor c) => (int)c <= (int)BubbleColor.Orange;

        public static Material CreateMaterial(BubbleColor c)
        {
            var m = new Material(Shader.Find("Standard"));
            Color baseCol = Colors[(int)c];
            m.color = baseCol;
            m.SetFloat("_Glossiness", 0.95f);
            m.SetFloat("_Metallic", 0.15f);
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", baseCol * 0.45f);
            return m;
        }

        /// <summary>Transparent white material used for fake specular highlights.</summary>
        public static Material TransparentMat(Color color)
        {
            var m = new Material(Shader.Find("Standard"));
            m.SetFloat("_Mode", 3f);
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = 3000;
            m.color = color;
            return m;
        }

        /// <summary>Adds a fake specular highlight — small white sphere toward the camera.</summary>
        public static void AddGloss(GameObject bubble, float radius)
        {
            var gloss = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gloss.name = "Gloss";
            Object.Destroy(gloss.GetComponent<Collider>());
            gloss.transform.SetParent(bubble.transform, false);
            gloss.transform.localPosition = new Vector3(-0.15f, 0.19f, -0.34f);
            gloss.transform.localScale = Vector3.one * 0.32f;
            gloss.GetComponent<Renderer>().material =
                TransparentMat(new Color(1f, 1f, 1f, 0.55f));
        }
    }
}
