using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Fired bubble: travels in a straight line, reflects off side walls,
    /// and reports back when it hits the ceiling or existing bubbles.
    /// Supports piercing Fireball, high-speed Lightning, and explosive Bomb shots.
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
        readonly HashSet<Bubble> piercedBubbles = new HashSet<Bubble>();

        public void Launch(BubbleColor color, bool isBomb, Vector3 origin, Vector3 direction,
                           BubbleBoard board, float radius)
        {
            Color = color; IsBomb = isBomb;
            this.board = board; this.radius = radius;
            transform.position = origin;
            dir = direction.normalized; dir.z = 0f;
            transform.localScale = Vector3.one * radius * 2f;
            Speed = color == BubbleColor.Lightning ? 32f : (color == BubbleColor.Fireball ? 28f : 25f);
            var rend = GetComponent<Renderer>();
            rend.sharedMaterial = BubblePalette.CreateMaterial(color);
            BubblePalette.Add3DOrnaments(gameObject, radius, color);
        }

        float lifetime;

        void Update()
        {
            if (done) return;
            lifetime += Time.deltaTime;
            // 8-second safety watchdog only — never snaps in mid-air
            if (lifetime > 8f)
            {
                done = true;
                GameManager.I.OnProjectileLost(this);
                Destroy(gameObject);
                return;
            }
            float step = Speed * Time.deltaTime;
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

            // Side wall bounce
            float lim = (dir.x < 0 ? board.LeftWallX : board.RightWallX) + (dir.x < 0 ? radius : -radius);
            if ((dir.x < 0 && p.x <= lim) || (dir.x > 0 && p.x >= lim))
            {
                p.x = lim;
                dir.x = -dir.x;
                if (AudioManager.I != null) AudioManager.I.Play("click");
            }

            // Fireball: pierce through bubbles
            if (Color == BubbleColor.Fireball)
            {
                foreach (var b in board.AllBubbles())
                {
                    if (b == null || b.IsPopping || piercedBubbles.Contains(b)) continue;
                    if ((p - b.transform.position).sqrMagnitude <= (radius * 2f * 0.95f) * (radius * 2f * 0.95f))
                    {
                        piercedBubbles.Add(b);
                        GameManager.I.OnFireballHitBubble(b);
                    }
                }

                if (p.y >= board.TopY - radius * 0.2f)
                {
                    transform.position = p;
                    done = true;
                    GameManager.I.OnFireballComplete(p);
                    Destroy(gameObject, 0.05f);
                    return;
                }

                transform.position = p;
                return;
            }

            // Ceiling attach
            if (p.y >= board.TopY - radius * 0.2f)
            {
                transform.position = p;
                Snap(board.FindCeilingCell(p), p);
                return;
            }

            // Normal bubble collision
            foreach (var b in board.AllBubbles())
            {
                if (b == null || b.IsPopping) continue;
                if ((p - b.transform.position).sqrMagnitude <= (radius * 2f * 0.95f) * (radius * 2f * 0.95f))
                {
                    transform.position = p;
                    Snap(board.FindSnapCell(p), p);
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
