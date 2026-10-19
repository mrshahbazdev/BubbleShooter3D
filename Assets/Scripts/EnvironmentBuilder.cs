using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Procedural 3D environment dressing around the bubble-shooter arena:
    /// crystal shard clusters on both sides, floating islands, ambient dust
    /// particles, and distance fog — all tinted per level theme.
    /// </summary>
    public class EnvironmentBuilder : MonoBehaviour
    {
        readonly List<Material> crystalMats = new List<Material>();
        readonly List<Material> islandTopMats = new List<Material>();
        readonly List<Transform> islands = new List<Transform>();
        readonly List<Vector3> islandBasePos = new List<Vector3>();
        readonly List<Transform> crystalTransforms = new List<Transform>();
        readonly List<Vector3> crystalBasePos = new List<Vector3>();
        ParticleSystem dustPs;
        GameObject root;

        public void Init()
        {
            root = new GameObject("Environment");
            root.transform.SetParent(transform, false);

            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.Exponential;
            RenderSettings.fogDensity = 0.012f;

            BuildCrystalClusters();
            BuildFloatingIslands();
            BuildAmbientDust();
        }

        void BuildCrystalClusters()
        {
            foreach (int side in new[] { -1, 1 })
            {
                for (int i = 0; i < 5; i++)
                {
                    var shard = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    shard.name = "CrystalShard_" + (side < 0 ? "L" : "R") + i;
                    Destroy(shard.GetComponent<Collider>());
                    shard.transform.SetParent(root.transform, false);
                    shard.transform.localScale = new Vector3(
                        Random.Range(0.5f, 0.9f),
                        Random.Range(1.6f, 3.2f),
                        Random.Range(0.5f, 0.9f));
                    Vector3 pos = new Vector3(
                        side * (7.2f + Random.Range(0f, 1.6f)),
                        -3.5f + i * 1.9f + Random.Range(0f, 0.5f),
                        2.2f + Random.Range(0f, 1.5f));
                    shard.transform.position = pos;
                    shard.transform.rotation = Quaternion.Euler(0f, 0f, Random.Range(-18f, 18f));

                    var mat = new Material(Shader.Find("Standard"));
                    mat.SetFloat("_Metallic", 0.85f);
                    mat.SetFloat("_Glossiness", 0.6f);
                    mat.color = new Color(0.10f, 0.12f, 0.18f);
                    mat.EnableKeyword("_EMISSION");
                    mat.SetColor("_EmissionColor", Color.black);
                    shard.GetComponent<Renderer>().material = mat;

                    crystalMats.Add(mat);
                    crystalTransforms.Add(shard.transform);
                    crystalBasePos.Add(pos);
                }
            }
        }

        void BuildFloatingIslands()
        {
            var spots = new[]
            {
                new Vector3(-4.5f, 5.2f, 3.8f),
                new Vector3(4.2f, 6.1f, 4.2f),
                new Vector3(0.5f, 7.4f, 5.0f),
            };

            for (int i = 0; i < spots.Length; i++)
            {
                var island = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                island.name = "FloatingIsland_" + i;
                Destroy(island.GetComponent<Collider>());
                island.transform.SetParent(root.transform, false);
                island.transform.position = spots[i];
                island.transform.localScale = new Vector3(2.2f, 0.7f, 1.4f);

                var rockMat = new Material(Shader.Find("Standard"));
                rockMat.color = new Color(0.12f, 0.11f, 0.15f);
                rockMat.SetFloat("_Glossiness", 0.3f);
                island.GetComponent<Renderer>().material = rockMat;

                // small emissive crystal perched on top
                var top = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                top.name = "IslandCrystal_" + i;
                Destroy(top.GetComponent<Collider>());
                top.transform.SetParent(island.transform, false);
                top.transform.localPosition = new Vector3(0f, 0.55f, 0f);
                top.transform.localScale = new Vector3(0.35f, 0.7f, 0.35f);

                var topMat = new Material(Shader.Find("Standard"));
                topMat.color = new Color(0.10f, 0.10f, 0.14f);
                topMat.EnableKeyword("_EMISSION");
                topMat.SetColor("_EmissionColor", Color.cyan * 0.8f);
                top.GetComponent<Renderer>().material = topMat;

                islandTopMats.Add(topMat);
                islands.Add(island.transform);
                islandBasePos.Add(spots[i]);
            }
        }

        void BuildAmbientDust()
        {
            var dustGo = new GameObject("AmbientDust");
            dustGo.transform.SetParent(root.transform, false);
            dustGo.transform.position = new Vector3(0f, 1.5f, -2f);

            dustPs = dustGo.AddComponent<ParticleSystem>();
            var main = dustPs.main;
            main.loop = true;
            main.startLifetime = new ParticleSystem.MinMaxCurve(6f, 10f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.15f, 0.4f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.04f, 0.1f);
            main.gravityModifier = -0.05f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 70;

            var shape = dustPs.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(26f, 16f, 6f);

            var emission = dustPs.emission;
            emission.rateOverTime = 12f;

            var psRenderer = dustGo.GetComponent<ParticleSystemRenderer>();
            if (psRenderer != null)
            {
                var dustMat = new Material(Shader.Find("Particles/Standard Unlit"));
                dustMat.color = new Color(0.6f, 0.8f, 1f, 0.5f);
                psRenderer.material = dustMat;
            }
        }

        public void ApplyTheme(ThemeData theme)
        {
            RenderSettings.fogColor = theme.Backdrop;

            Color darkBackdrop = new Color(theme.Backdrop.r * 0.35f, theme.Backdrop.g * 0.35f, theme.Backdrop.b * 0.35f);
            foreach (var m in crystalMats)
            {
                if (m == null) continue;
                m.color = darkBackdrop;
                m.SetColor("_EmissionColor", theme.Accent * 0.9f);
            }

            foreach (var m in islandTopMats)
            {
                if (m == null) continue;
                m.SetColor("_EmissionColor", theme.Accent);
            }

            if (dustPs != null)
            {
                var main = dustPs.main;
                main.startColor = theme.Starfield;
            }
        }

        void Update()
        {
            float t = Time.time;

            for (int i = 0; i < crystalTransforms.Count; i++)
            {
                var ct = crystalTransforms[i];
                if (ct == null) continue;
                Vector3 basePos = crystalBasePos[i];
                ct.position = new Vector3(basePos.x, basePos.y + Mathf.Sin(t * 0.6f + i * 1.3f) * 0.15f, basePos.z);
                ct.Rotate(Vector3.up, 6f * Time.deltaTime, Space.World);
            }

            for (int i = 0; i < islands.Count; i++)
            {
                var it = islands[i];
                if (it == null) continue;
                Vector3 basePos = islandBasePos[i];
                float phase = i * 2.1f;
                it.position = new Vector3(
                    basePos.x + Mathf.Sin(t * 0.4f + phase) * 0.2f,
                    basePos.y + Mathf.Sin(t * 0.55f + phase * 1.7f) * 0.35f,
                    basePos.z);
            }
        }
    }
}
