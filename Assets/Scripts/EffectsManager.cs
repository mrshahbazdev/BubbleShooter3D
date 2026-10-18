using System.Collections;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>Particle bursts and shockwaves — all generated in code, no assets needed.</summary>
    public class EffectsManager : MonoBehaviour
    {
        public static EffectsManager I { get; private set; }

        Material particleMat;
        Font popupFont;
        Vector3 camRestPos;
        float shakeAmount;

        void Awake()
        {
            I = this;
            particleMat = new Material(Shader.Find("Particles/Standard Unlit"));
            particleMat.SetColor("_Color", Color.white);
            try { popupFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); }
            catch { popupFont = Resources.GetBuiltinResource<Font>("Arial.ttf"); }
        }

        public void Burst(Vector3 pos, BubbleColor color)
        {
            var go = new GameObject("Burst");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.4f, 0.9f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3f, 7f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.08f, 0.22f);
            main.startColor = BubblePalette.Colors[(int)color];
            main.maxParticles = 60;
            main.gravityModifier = 0.5f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 42) });
            var col = ps.colorOverLifetime;
            col.enabled = true;
            var grad = new Gradient();
            grad.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            col.color = grad;
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

        /// <summary>Floating "+points" text that rises and fades.</summary>
        public void Popup(Vector3 pos, string text, Color color)
        {
            var go = new GameObject("Popup");
            go.transform.position = pos + new Vector3(0f, 0.5f, -1f);
            var tm = go.AddComponent<TextMesh>();
            tm.font = popupFont;
            tm.GetComponent<MeshRenderer>().material = popupFont.material;
            tm.text = text;
            tm.fontSize = 64;
            tm.characterSize = 0.12f;
            tm.anchor = TextAnchor.MiddleCenter;
            tm.color = color;
            StartCoroutine(FloatUp(go));
        }

        IEnumerator FloatUp(GameObject go)
        {
            float t = 0;
            var tm = go.GetComponent<TextMesh>();
            Vector3 from = go.transform.position;
            while (t < 0.9f)
            {
                t += Time.deltaTime;
                go.transform.position = from + Vector3.up * (t * 2.2f);
                var c = tm.color;
                c.a = Mathf.Lerp(1f, 0f, t / 0.9f);
                tm.color = c;
                yield return null;
            }
            Destroy(go);
        }

        public void ElectricBurst(Vector3 pos)
        {
            var go = new GameObject("ElectricBurst");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(6f, 14f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.18f);
            main.startColor = new Color(0.3f, 0.95f, 1f, 1f);
            main.maxParticles = 50;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 35) });
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleMat;
            Destroy(go, 1f);
        }

        public void FireBurst(Vector3 pos)
        {
            var go = new GameObject("FireBurst");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.1f, 0.28f);
            main.startColor = new Color(1f, 0.45f, 0.08f, 1f);
            main.maxParticles = 45;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 30) });
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleMat;
            Destroy(go, 1f);
        }

        public void FloorSplash(Vector3 pos, BubbleColor color)
        {
            var go = new GameObject("FloorSplash");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.2f, 0.45f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(2.5f, 5.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.06f, 0.16f);
            main.startColor = BubblePalette.Colors[(int)color];
            main.maxParticles = 25;
            main.gravityModifier = 0.8f;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 18) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 35f;
            shape.radius = 0.15f;
            go.transform.rotation = Quaternion.Euler(-90f, 0f, 0f); // Upward cone
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleMat;
            Destroy(go, 0.8f);
        }

        public void MegaConfetti(Vector3 pos)
        {
            var go = new GameObject("MegaConfetti");
            go.transform.position = pos;
            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.6f, 1.4f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 10f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.28f);
            main.startColor = new Color(1f, 0.85f, 0.2f, 1f);
            main.maxParticles = 80;
            main.gravityModifier = 0.35f;
            var emission = ps.emission;
            emission.rateOverTime = 0;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 75) });
            var shape = ps.shape;
            shape.shapeType = ParticleSystemShapeType.Sphere;
            shape.radius = 0.8f;
            var psr = go.GetComponent<ParticleSystemRenderer>();
            psr.material = particleMat;
            Destroy(go, 1.8f);
        }

        public void ShowComboCallout(int combo)
        {
            if (combo < 2) return;
            string text = combo switch
            {
                2 => "NICE! x2",
                3 => "GREAT! x3",
                4 => "AWESOME! x4",
                _ => "UNSTOPPABLE! x" + combo
            };
            Color col = combo switch
            {
                2 => new Color(0.4f, 0.9f, 1f),
                3 => new Color(1f, 0.85f, 0.2f),
                4 => new Color(1f, 0.45f, 0.1f),
                _ => new Color(1f, 0.2f, 0.6f)
            };
            Popup(new Vector3(0f, 1.2f, -1f), text, col);
            Shake(0.12f + combo * 0.04f);
        }

        /// <summary>Camera shake — call once, decays automatically.</summary>
        public void Shake(float amount)
        {
            if (Camera.main == null) return;
            camRestPos = Camera.main.transform.localPosition;
            shakeAmount = Mathf.Max(shakeAmount, amount);
        }

        void LateUpdate()
        {
            if (Camera.main == null || shakeAmount <= 0f) return;
            Camera.main.transform.localPosition = camRestPos +
                new Vector3(
                    Mathf.PerlinNoise(Time.time * 40f, 0f) - 0.5f,
                    Mathf.PerlinNoise(0f, Time.time * 40f) - 0.5f,
                    0f) * shakeAmount;
            shakeAmount = Mathf.Lerp(shakeAmount, 0f, Time.deltaTime * 6f);
            if (shakeAmount < 0.005f)
            {
                shakeAmount = 0f;
                Camera.main.transform.localPosition = camRestPos;
            }
        }
    }
}
