using System.Collections;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>Particle bursts and shockwaves — all generated in code, no assets needed.</summary>
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager I { get; private set; }

        Material particleMat;

        void Awake()
        {
            I = this;
            particleMat = new Material(Shader.Find("Particles/Standard Unlit"));
            particleMat.SetColor("_Color", Color.white);
        }

        public void Burst(Vector3 pos, BubbleColor color)
        {
            var go = new GameObject("Burst");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 5f;
            main.startSize = 0.18f;
            main.startColor = BubblePalette.Colors[(int)color];
            main.maxParticles = 30;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 26) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.2f;
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleMat;
            Destroy(go, 1.5f);
        }

        public void Shockwave(Vector3 pos)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());
            go.transform.position = pos;
            var m = go.GetComponent<Renderer>().material;
            m.color = new Color(1f, 0.6f, 0.1f, 0.5f);
            m.SetFloat("_Mode", 3f); // transparent
            m.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            m.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            m.EnableKeyword("_ALPHABLEND_ON");
            m.renderQueue = 3000;
            StartCoroutine(Expand(go));
        }

        IEnumerator Expand(GameObject go)
        {
            float t = 0;
            while (t < 0.35f)
            {
                t += Time.deltaTime;
                go.transform.localScale = Vector3.one * (0.5f + t * 18f);
                var m = go.GetComponent<Renderer>().material;
                var c = m.color; c.a = Mathf.Lerp(0.5f, 0f, t / 0.35f); m.color = c;
                yield return null;
            }
            Destroy(go);
        }
    }
}
