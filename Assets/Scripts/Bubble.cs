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

        public void Init(BubbleColor color, int row, int col, float radius)
        {
            Color = color; Row = row; Col = col;
            name = $"Bubble_{color}_{row}_{col}";
            transform.localScale = Vector3.one * radius * 2f;
            rend = GetComponent<Renderer>();
            rainbowPhase = Random.value * 6f;
            if (color != BubbleColor.Rainbow)
                rend.sharedMaterial = BubblePalette.CreateMaterial(color);
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
        }

        public IEnumerator PopAnim(float delay)
        {
            IsPopping = true;
            yield return new WaitForSeconds(delay);
            float t = 0f;
            Vector3 s = transform.localScale;
            while (t < 0.18f)
            {
                t += Time.deltaTime;
                transform.localScale = s * (1f + t * 4f); // grow then vanish
                yield return null;
            }
            Destroy(gameObject);
        }

        public IEnumerator DropAnim()
        {
            var rb = gameObject.AddComponent<Rigidbody>();
            rb.useGravity = true;
            rb.mass = 0.1f;
            rb.AddTorque(Random.insideUnitSphere * 5f, ForceMode.Impulse);
            rb.velocity = new Vector3(Random.Range(-1.5f, 1.5f), 0.5f, Random.Range(-0.5f, 0.5f));
            yield return new WaitForSeconds(1.6f);
            Destroy(gameObject);
        }
    }
}
