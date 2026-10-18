using System.Collections;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>A bubble placed on the board grid.</summary>
    public class Bubble : MonoBehaviour
    {
        public BubbleColor Color;
        public int Row;
        public int Col;
        public bool IsPopping;

        static readonly Color[] rainbowStops =
        {
            new Color(0.95f, 0.22f, 0.27f), new Color(0.98f, 0.55f, 0.12f),
            new Color(0.98f, 0.82f, 0.15f), new Color(0.22f, 0.80f, 0.35f),
            new Color(0.20f, 0.50f, 0.98f), new Color(0.65f, 0.30f, 0.90f),
        };

        Renderer rend;
        float rainbowPhase;

        Vector3 baseScale;

        public void Init(BubbleColor color, int row, int col, float radius)
        {
            Color = color; Row = row; Col = col;
            name = $"Bubble_{color}_{row}_{col}";
            baseScale = Vector3.one * radius * 2f;
            transform.localScale = baseScale;
            rend = GetComponent<Renderer>();
            rainbowPhase = Random.value * 6f;
            rend.sharedMaterial = BubblePalette.CreateMaterial(color);
            BubblePalette.AddGloss(gameObject, radius);
        }

        void Update()
        {
            if (Color == BubbleColor.Rainbow && rend != null)
            {
                rainbowPhase += Time.deltaTime * 2f;
                int i = Mathf.FloorToInt(rainbowPhase) % rainbowStops.Length;
                int j = (i + 1) % rainbowStops.Length;
                rend.material.color = UnityEngine.Color.Lerp(rainbowStops[i], rainbowStops[j], rainbowPhase % 1f);
            }

            if (!IsPopping)
            {
                // Organic 3D breathing pulse so the bubbles feel alive and glossy
                float breath = 1f + 0.015f * Mathf.Sin(Time.time * 2.2f + Row * 0.6f + Col * 0.4f);
                transform.localScale = baseScale * breath;
            }
        }

        public IEnumerator PopAnim(float delay)
        {
            IsPopping = true;
            yield return new WaitForSeconds(delay);
            float t = 0f;
            Vector3 s = transform.localScale;
            while (t < 0.16f)
            {
                t += Time.deltaTime;
                float p = t / 0.16f;
                transform.localScale = s * (1f + p * 3.5f);
                yield return null;
            }
            Destroy(gameObject);
        }

        public IEnumerator DropAnim()
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 0.12f;
            rb.AddTorque(Random.insideUnitSphere * 6f, ForceMode.Impulse);
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = new Vector3(Random.Range(-2f, 2f), Random.Range(1f, 3f), Random.Range(-0.8f, 0.8f));
#else
            rb.velocity = new Vector3(Random.Range(-2f, 2f), Random.Range(1f, 3f), Random.Range(-0.8f, 0.8f));
#endif
            yield return new WaitForSeconds(1.2f);
            if (EffectsManager.I != null) EffectsManager.I.Burst(transform.position, Color);
            Destroy(gameObject);
        }
    }
}
