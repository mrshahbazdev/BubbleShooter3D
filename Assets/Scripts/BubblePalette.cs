using UnityEngine;

namespace BubbleShooter
{
    public enum BubbleColor { Red, Green, Blue, Yellow, Purple, Orange, Bomb, Rainbow, Fireball, Lightning }

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
            new Color(1.00f, 0.35f, 0.05f), // Fireball
            new Color(0.20f, 0.95f, 1.00f), // Lightning
        };

        public static bool IsPlayable(BubbleColor c) => (int)c <= (int)BubbleColor.Orange;

        public static Material CreateMaterial(BubbleColor c)
        {
            var m = new Material(Shader.Find("Standard"));
            Color baseCol = Colors[(int)c];
            m.color = baseCol;

            if (c == BubbleColor.Bomb)
            {
                m.color = new Color(0.14f, 0.14f, 0.18f);
                m.SetFloat("_Glossiness", 0.90f);
                m.SetFloat("_Metallic", 0.82f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(0.35f, 0.06f, 0.06f));
            }
            else if (c == BubbleColor.Fireball)
            {
                m.color = new Color(1.00f, 0.40f, 0.05f);
                m.SetFloat("_Glossiness", 0.96f);
                m.SetFloat("_Metallic", 0.10f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(1.00f, 0.45f, 0.08f) * 0.85f);
            }
            else if (c == BubbleColor.Lightning)
            {
                m.color = new Color(0.25f, 0.92f, 1.00f);
                m.SetFloat("_Glossiness", 0.96f);
                m.SetFloat("_Metallic", 0.10f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(0.20f, 0.85f, 1.00f) * 0.85f);
            }
            else if (c == BubbleColor.Rainbow)
            {
                m.color = Color.white;
                m.SetFloat("_Glossiness", 0.98f);
                m.SetFloat("_Metallic", 0.30f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", new Color(0.3f, 0.3f, 0.35f));
            }
            else
            {
                // Playable glossy 3D candy/marble spheres:
                // Keep emission subtle (0.10f) so directional lights create realistic 3D sphere curvature shading
                m.SetFloat("_Glossiness", 0.94f);
                m.SetFloat("_Metallic", 0.12f);
                m.EnableKeyword("_EMISSION");
                m.SetColor("_EmissionColor", baseCol * 0.10f);
            }
            return m;
        }

        /// <summary>Transparent material used for ghost aim indicator and specular highlights.</summary>
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

        public static Material CreateGhostMaterial(BubbleColor c)
        {
            Color baseCol = Colors[(int)c];
            var m = TransparentMat(new Color(baseCol.r, baseCol.g, baseCol.b, 0.42f));
            m.EnableKeyword("_EMISSION");
            m.SetColor("_EmissionColor", baseCol * 0.5f);
            return m;
        }

        /// <summary>Adds comprehensive 3D ornaments: inner optical refraction core, dual specular glints, and powerup accessories.</summary>
        public static void Add3DOrnaments(GameObject bubble, float radius, BubbleColor color)
        {
            Color baseCol = Colors[(int)color];

            // 1. Inner glowing 3D crystal core (creates volumetric refraction depth)
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "InnerCore";
            Object.Destroy(core.GetComponent<Collider>());
            core.transform.SetParent(bubble.transform, false);
            core.transform.localPosition = new Vector3(0f, 0.02f, 0.05f);
            core.transform.localScale = Vector3.one * 0.56f;
            var coreMat = new Material(Shader.Find("Standard"));
            coreMat.color = baseCol;
            coreMat.SetFloat("_Glossiness", 0.90f);
            coreMat.EnableKeyword("_EMISSION");
            coreMat.SetColor("_EmissionColor", baseCol * (color == BubbleColor.Bomb ? 0.2f : 0.65f));
            core.GetComponent<Renderer>().material = coreMat;

            // 2. Primary 3D specular highlight glint (upper-left, facing key light)
            var glint1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glint1.name = "GlintPrimary";
            Object.Destroy(glint1.GetComponent<Collider>());
            glint1.transform.SetParent(bubble.transform, false);
            glint1.transform.localPosition = new Vector3(-0.16f, 0.19f, -0.36f);
            glint1.transform.localScale = new Vector3(0.24f, 0.20f, 0.12f);
            var gMat1 = new Material(Shader.Find("Standard"));
            gMat1.color = Color.white;
            gMat1.EnableKeyword("_EMISSION");
            gMat1.SetColor("_EmissionColor", Color.white * 0.85f);
            glint1.GetComponent<Renderer>().material = gMat1;

            // 3. Secondary 3D bounce rim glint (lower-right, ambient bounce light)
            var glint2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            glint2.name = "GlintBounce";
            Object.Destroy(glint2.GetComponent<Collider>());
            glint2.transform.SetParent(bubble.transform, false);
            glint2.transform.localPosition = new Vector3(0.18f, -0.17f, -0.32f);
            glint2.transform.localScale = new Vector3(0.14f, 0.11f, 0.08f);
            var gMat2 = new Material(Shader.Find("Standard"));
            gMat2.color = new Color(0.85f, 0.92f, 1.0f);
            gMat2.EnableKeyword("_EMISSION");
            gMat2.SetColor("_EmissionColor", new Color(0.85f, 0.92f, 1.0f) * 0.40f);
            glint2.GetComponent<Renderer>().material = gMat2;

            // 4. Special 3D accessories
            if (color == BubbleColor.Bomb)
            {
                // Bomb fuse cylinder on top
                var fuse = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                fuse.name = "BombFuse";
                Object.Destroy(fuse.GetComponent<Collider>());
                fuse.transform.SetParent(bubble.transform, false);
                fuse.transform.localPosition = new Vector3(0f, 0.46f, 0f);
                fuse.transform.localScale = new Vector3(0.10f, 0.12f, 0.10f);
                var fm = fuse.GetComponent<Renderer>().material;
                fm.color = new Color(0.45f, 0.35f, 0.22f);

                // Fuse spark
                var spark = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                spark.name = "FuseSpark";
                Object.Destroy(spark.GetComponent<Collider>());
                spark.transform.SetParent(bubble.transform, false);
                spark.transform.localPosition = new Vector3(0f, 0.58f, 0f);
                spark.transform.localScale = Vector3.one * 0.12f;
                var sm = spark.GetComponent<Renderer>().material;
                sm.color = new Color(1f, 0.8f, 0.1f);
                sm.EnableKeyword("_EMISSION");
                sm.SetColor("_EmissionColor", new Color(1f, 0.7f, 0f));
            }
            else if (color == BubbleColor.Lightning)
            {
                // Electric halo ring
                var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                ring.name = "ElectricHalo";
                Object.Destroy(ring.GetComponent<Collider>());
                ring.transform.SetParent(bubble.transform, false);
                ring.transform.localRotation = Quaternion.Euler(45f, 30f, 0f);
                ring.transform.localScale = new Vector3(1.12f, 0.02f, 1.12f);
                var rm = ring.GetComponent<Renderer>().material;
                rm.color = new Color(0.3f, 0.95f, 1f);
                rm.EnableKeyword("_EMISSION");
                rm.SetColor("_EmissionColor", new Color(0.2f, 0.9f, 1f));
            }
        }

        /// <summary>Backward compatibility alias.</summary>
        public static void AddGloss(GameObject bubble, float radius)
        {
            Add3DOrnaments(bubble, radius, BubbleColor.Blue);
        }
    }
}
