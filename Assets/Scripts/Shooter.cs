using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// The cannon: aims at the pointer, renders a bouncing trajectory preview,
    /// and fires the current bubble. Handles next-bubble queue and power-up shots.
    /// </summary>
    public class Shooter : MonoBehaviour
    {
        public float Radius = 0.48f;
        public int TrajectoryBounces = 3;

        BubbleBoard board;
        GameObject currentGo, nextGo;
        GameObject barrel;
        LineRenderer line;
        Projectile active;
        Camera cam;
        Vector3 muzzle => transform.position + Vector3.up * 0.9f;

        public BubbleColor Current { get; private set; }
        public BubbleColor Next { get; private set; }
        public bool BombArmed;
        public bool Busy => active != null;

        public void Init(BubbleBoard board, Camera cam)
        {
            this.board = board; this.cam = cam;
            transform.position = new Vector3(0f, -4.6f, 0f);

            // cannon base
            var baseGo = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            baseGo.name = "CannonBase";
            Destroy(baseGo.GetComponent<Collider>());
            baseGo.transform.SetParent(transform, false);
            baseGo.transform.localScale = new Vector3(1.6f, 0.15f, 1.6f);
            var bm = baseGo.GetComponent<Renderer>().material;
            bm.color = new Color(0.16f, 0.17f, 0.22f);
            bm.SetFloat("_Glossiness", 0.9f);

            // barrel (rotates with aim)
            barrel = new GameObject("Barrel");
            barrel.transform.SetParent(transform, false);
            barrel.transform.position = transform.position;
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            Destroy(shaft.GetComponent<Collider>());
            shaft.transform.SetParent(barrel.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            shaft.transform.localScale = new Vector3(0.5f, 0.45f, 0.5f);
            var sm = shaft.GetComponent<Renderer>().material;
            sm.color = new Color(0.22f, 0.24f, 0.30f);
            sm.SetFloat("_Glossiness", 0.8f);

            // trajectory line
            var lineGo = new GameObject("Trajectory");
            line = lineGo.AddComponent<LineRenderer>();
            line.positionCount = 0;
            line.startWidth = line.endWidth = 0.06f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = line.endColor = new Color(1f, 1f, 1f, 0.55f);
            line.textureMode = LineTextureMode.Tile;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;

            // current / next bubble visual holders
            currentGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(currentGo.GetComponent<Collider>());
            currentGo.transform.SetParent(transform, false);
            nextGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(nextGo.GetComponent<Collider>());
            nextGo.transform.SetParent(transform, false);
            nextGo.transform.localPosition = new Vector3(1.5f, 0.3f, 0f);
            nextGo.transform.localScale = Vector3.one * Radius * 1.4f;

            Reload(first: true);
        }

        BubbleColor PickColor()
        {
            var alive = board.RemainingPlayableColors();
            if (alive.Count == 0) return (BubbleColor)Random.Range(0, 6);
            var arr = new List<BubbleColor>(alive);
            return arr[Random.Range(0, arr.Count)];
        }

        void Reload(bool first = false)
        {
            if (first) { Current = PickColor(); Next = PickColor(); }
            else
            {
                Current = Next;
                // ~8% chance the next bubble is a wildcard Rainbow
                Next = Random.value < 0.08f ? BubbleColor.Rainbow : PickColor();
            }
            BombArmed = false;
            currentGo.transform.position = muzzle;
            currentGo.transform.localScale = Vector3.one * Radius * 2f;
            currentGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Current);
            nextGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Next);
        }

        public void Swap() // X key — swap current and next
        {
            (Current, Next) = (Next, Current);
            currentGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Current);
            nextGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Next);
        }

        Vector3 AimDir()
        {
            Vector3 world = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 d = new Vector3(world.x - muzzle.x, world.y - muzzle.y, 0f);
            if (d.y < 0.05f) d = new Vector3(d.x, 0.05f, 0f);
            return d.normalized;
        }

        void Update()
        {
            if (Busy || GameManager.I.State != GameState.Playing) { line.positionCount = 0; return; }
            Vector3 d = AimDir();
            barrel.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f);
            DrawTrajectory(muzzle, d);
            currentGo.transform.position = muzzle;

            if (Input.GetMouseButtonDown(0)) Fire(d);
            if (Input.GetKeyDown(KeyCode.X)) Swap();
            if (Input.GetKeyDown(KeyCode.B) && GameManager.I.BombsLeft > 0) ArmBomb();
        }

        void DrawTrajectory(Vector3 origin, Vector3 dir)
        {
            var pts = new List<Vector3> { origin };
            Vector3 p = origin, d = dir;
            int bounces = 0;
            for (int guard = 0; guard < 400; guard++)
            {
                Vector3 next = p + d * (Radius * 0.5f);
                float lim = (d.x < 0 ? board.LeftWallX : board.RightWallX) + (d.x < 0 ? Radius : -Radius);
                if ((d.x < 0 && next.x <= lim) || (d.x > 0 && next.x >= lim))
                {
                    next.x = lim; d.x = -d.x; bounces++;
                    pts.Add(next);
                    if (bounces > TrajectoryBounces) break;
                }
                else if (next.y >= board.TopY - Radius * 0.2f) { pts.Add(next); break; }
                else
                {
                    bool hit = false;
                    foreach (var b in board.AllBubbles())
                        if (b != null && !b.IsPopping &&
                            (next - b.transform.position).sqrMagnitude <= (Radius * 2f * 0.95f) * (Radius * 2f * 0.95f))
                        { hit = true; break; }
                    if (hit) { pts.Add(next); break; }
                }
                p = next;
            }
            line.positionCount = pts.Count;
            line.SetPositions(pts.ToArray());
        }

        void Fire(Vector3 dir)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());
            active = go.AddComponent<Projectile>();
            bool bomb = BombArmed;
            var color = bomb ? BubbleColor.Bomb : Current;
            if (bomb) GameManager.I.BombsLeft--;
            active.Launch(color, bomb, muzzle, dir, board, Radius);
            currentGo.transform.localScale = Vector3.zero;
            line.positionCount = 0;
            AudioManager.I.Play("shoot");
        }

        void ArmBomb()
        {
            BombArmed = true;
            currentGo.GetComponent<Renderer>().sharedMaterial =
                BubblePalette.CreateMaterial(BubbleColor.Bomb);
        }

        public void OnProjectileDone()
        {
            active = null;
            if (GameManager.I.State == GameState.Playing) Reload();
        }
    }
}
