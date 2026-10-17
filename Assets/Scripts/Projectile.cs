using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Fired bubble: travels in a straight line, reflects off the side walls,
    /// and reports back when it hits the ceiling or an existing bubble.
    /// Collision is analytic (distance to cell centers) — deterministic and cheap.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        public BubbleColor Color;
        public float Speed = 22f;
        public bool IsBomb;

        Vector3 dir;
        BubbleBoard board;
        float radius;
        bool done;

        public void Launch(BubbleColor color, bool isBomb, Vector3 origin, Vector3 direction,
                           BubbleBoard board, float radius)
        {
            Color = color; IsBomb = isBomb;
            this.board = board; this.radius = radius;
            transform.position = origin;
            dir = direction.normalized; dir.z = 0f;
            transform.localScale = Vector3.one * radius * 2f;
            var rend = GetComponent<Renderer>();
            rend.sharedMaterial = BubblePalette.CreateMaterial(color);
        }

        void Update()
        {
            if (done) return;
            float step = Speed * Time.deltaTime;
            // sub-step so fast shots can't tunnel through bubbles
            while (step > 0f && !done)
            {
                float s = Mathf.Min(step, radius * 0.4f);
                step -= s;
                Advance(s);
            }
        }

        void Advance(float s)
        {
            Vector3 p = transform.position + dir * s;

            // wall bounce
            float lim = (dir.x < 0 ? board.LeftWallX : board.RightWallX) + (dir.x < 0 ? radius : -radius);
            if ((dir.x < 0 && p.x <= lim) || (dir.x > 0 && p.x >= lim))
            {
                p.x = lim;
                dir.x = -dir.x;
            }

            // ceiling attach
            if (p.y >= board.TopY - radius * 0.2f)
            {
                transform.position = p;
                Snap(board.FindCeilingCell(p), p);
                return;
            }

            // bubble collision
            foreach (var b in board.AllBubbles())
            {
                if (b == null || b.IsPopping) continue;
                if ((p - b.transform.position).sqrMagnitude <= (radius * 2f * 0.95f) * (radius * 2f * 0.95f))
                {
                    transform.position = p;
                    Snap(board.FindSnapCell(b.transform.position), p);
                    return;
                }
            }

            transform.position = p;
        }

        void Snap(Vector2Int? cell, Vector3 contact)
        {
            done = true;
            if (cell == null)
            {
                GameManager.I.OnProjectileLost(this);
                return;
            }
            GameManager.I.OnProjectileSnapped(this, cell.Value, contact);
        }
    }
}
