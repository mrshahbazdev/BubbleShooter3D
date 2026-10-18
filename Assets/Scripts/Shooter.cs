using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    public enum PowerupType { None, Bomb, Fireball, Lightning }

    /// <summary>
    /// The cannon: aims at pointer, renders bouncing trajectory, shows exact
    /// ghost landing prediction on the hex grid, handles recoil, flash, and power-ups.
    /// </summary>
    public class Shooter : MonoBehaviour
    {
        public float Radius = 0.48f;
        public int TrajectoryBounces = 3;

        BubbleBoard board;
        GameObject currentGo, nextGo, ghostGo;
        GameObject barrel;
        Light muzzleLight;
        LineRenderer line;
        Projectile active;
        Camera cam;
        Vector3 muzzle => barrel.transform.position + barrel.transform.up * 0.9f;

        public BubbleColor Current { get; private set; }
        public BubbleColor Next { get; private set; }
        public PowerupType ActivePowerup { get; private set; } = PowerupType.None;
        public bool BombArmed => ActivePowerup == PowerupType.Bomb;
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
            barrel.transform.localPosition = Vector3.zero;

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            Destroy(shaft.GetComponent<Collider>());
            shaft.transform.SetParent(barrel.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            shaft.transform.localScale = new Vector3(0.5f, 0.45f, 0.5f);
            var sm = shaft.GetComponent<Renderer>().material;
            sm.color = new Color(0.22f, 0.24f, 0.30f);
            sm.SetFloat("_Glossiness", 0.8f);

            // glowing muzzle ring
            var ring = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ring.name = "MuzzleRing";
            Destroy(ring.GetComponent<Collider>());
            ring.transform.SetParent(barrel.transform, false);
            ring.transform.localPosition = new Vector3(0f, 0.92f, 0f);
            ring.transform.localScale = new Vector3(1.05f, 0.03f, 1.05f);
            var rm = ring.GetComponent<Renderer>().material;
            rm.color = new Color(0.1f, 0.75f, 1f);
            rm.EnableKeyword("_EMISSION");
            rm.SetColor("_EmissionColor", new Color(0.05f, 0.5f, 0.8f));

            // muzzle flash light
            var mlGo = new GameObject("MuzzleFlashLight");
            mlGo.transform.SetParent(barrel.transform, false);
            mlGo.transform.localPosition = new Vector3(0f, 0.95f, -0.5f);
            muzzleLight = mlGo.AddComponent<Light>();
            muzzleLight.type = LightType.Point;
            muzzleLight.range = 5f;
            muzzleLight.intensity = 2.5f;
            muzzleLight.color = new Color(0.4f, 0.8f, 1f);
            muzzleLight.enabled = false;

            // trajectory line
            var lineGo = new GameObject("Trajectory");
            line = lineGo.AddComponent<LineRenderer>();
            line.positionCount = 0;
            line.startWidth = line.endWidth = 0.06f;
            line.material = new Material(Shader.Find("Sprites/Default"));
            line.startColor = new Color(0.4f, 0.9f, 1f, 0.85f);
            line.endColor = new Color(0.4f, 0.9f, 1f, 0.15f);
            line.textureMode = LineTextureMode.Tile;
            line.numCornerVertices = 4;
            line.numCapVertices = 4;

            // ghost aim indicator (predicted landing bubble)
            ghostGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ghostGo.name = "AimGhost";
            Destroy(ghostGo.GetComponent<Collider>());
            ghostGo.transform.localScale = Vector3.one * Radius * 1.95f;
            ghostGo.SetActive(false);

            // current / next bubble visual holders
            currentGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(currentGo.GetComponent<Collider>());
            currentGo.transform.SetParent(transform, false);
            nextGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(nextGo.GetComponent<Collider>());
            nextGo.transform.SetParent(transform, false);
            nextGo.transform.localPosition = new Vector3(1.6f, 0.35f, 0f);
            nextGo.transform.localScale = Vector3.one * Radius * 1.4f;
            BubblePalette.AddGloss(currentGo, Radius);
            BubblePalette.AddGloss(nextGo, Radius);

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
                Next = Random.value < 0.08f ? BubbleColor.Rainbow : PickColor();
            }
            ActivePowerup = PowerupType.None;
            currentGo.transform.position = muzzle;
            currentGo.transform.localScale = Vector3.one * Radius * 2f;
            UpdateCurrentMaterial();
            nextGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Next);
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        void UpdateCurrentMaterial()
        {
            BubbleColor c = GetActiveColor();
            currentGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(c);
        }

        public BubbleColor GetActiveColor()
        {
            return ActivePowerup switch
            {
                PowerupType.Bomb => BubbleColor.Bomb,
                PowerupType.Fireball => BubbleColor.Fireball,
                PowerupType.Lightning => BubbleColor.Lightning,
                _ => Current
            };
        }

        public void Swap() // X key or UI button
        {
            if (Busy || GameManager.I.State != GameState.Playing) return;
            (Current, Next) = (Next, Current);
            ActivePowerup = PowerupType.None;
            UpdateCurrentMaterial();
            nextGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Next);
            AudioManager.I.Play("click");
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        public void ToggleBomb()
        {
            if (GameManager.I.BombsLeft <= 0) return;
            ActivePowerup = ActivePowerup == PowerupType.Bomb ? PowerupType.None : PowerupType.Bomb;
            UpdateCurrentMaterial();
            AudioManager.I.Play("click");
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        public void ToggleFireball()
        {
            if (GameManager.I.FireballsLeft <= 0) return;
            ActivePowerup = ActivePowerup == PowerupType.Fireball ? PowerupType.None : PowerupType.Fireball;
            UpdateCurrentMaterial();
            AudioManager.I.Play("click");
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        public void ToggleLightning()
        {
            if (GameManager.I.LightningsLeft <= 0) return;
            ActivePowerup = ActivePowerup == PowerupType.Lightning ? PowerupType.None : PowerupType.Lightning;
            UpdateCurrentMaterial();
            AudioManager.I.Play("click");
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        Vector3 AimDir()
        {
            if (cam == null) return Vector3.up;
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.forward, Vector3.zero);
            Vector3 world;
            if (plane.Raycast(ray, out float enter))
            {
                world = ray.GetPoint(enter);
            }
            else
            {
                world = cam.transform.position + ray.direction * 16f;
            }

            Vector3 d = new Vector3(world.x - transform.position.x, world.y - transform.position.y, 0f);
            if (d.y < 0.15f) d = new Vector3(d.x, 0.15f, 0f);
            return d.normalized;
        }

        void Update()
        {
            if (Busy || GameManager.I.State != GameState.Playing || GameManager.I.IsPaused)
            {
                line.positionCount = 0;
                if (ghostGo != null) ghostGo.SetActive(false);
                return;
            }

            Vector3 d = AimDir();
            barrel.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f);
            currentGo.transform.position = muzzle;

            DrawTrajectoryAndGhost(muzzle, d);

            bool isOverUI = UnityEngine.EventSystems.EventSystem.current != null &&
                            UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject();

            if (Input.GetMouseButtonDown(0) && !isOverUI) Fire(d);
            if (Input.GetKeyDown(KeyCode.X)) Swap();
            if (Input.GetKeyDown(KeyCode.B)) ToggleBomb();
            if (Input.GetKeyDown(KeyCode.F)) ToggleFireball();
            if (Input.GetKeyDown(KeyCode.L)) ToggleLightning();
        }

        void DrawTrajectoryAndGhost(Vector3 origin, Vector3 dir)
        {
            var pts = new List<Vector3> { origin };
            Vector3 p = origin, d = dir;
            int bounces = 0;
            Vector2Int? predictedCell = null;

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
                else if (next.y >= board.TopY - Radius * 0.2f)
                {
                    pts.Add(next);
                    predictedCell = board.FindCeilingCell(next);
                    break;
                }
                else
                {
                    bool hit = false;
                    foreach (var b in board.AllBubbles())
                    {
                        if (b != null && !b.IsPopping &&
                            (next - b.transform.position).sqrMagnitude <= (Radius * 2f * 0.95f) * (Radius * 2f * 0.95f))
                        {
                            hit = true;
                            predictedCell = board.FindSnapCell(next);
                            break;
                        }
                    }
                    if (hit) { pts.Add(next); break; }
                }
                p = next;
            }

            line.positionCount = pts.Count;
            line.SetPositions(pts.ToArray());

            // Update ghost bubble indicator
            if (predictedCell.HasValue && ActivePowerup != PowerupType.Fireball)
            {
                ghostGo.SetActive(true);
                Vector3 ghostPos = board.CellToWorld(predictedCell.Value.x, predictedCell.Value.y);
                ghostGo.transform.position = ghostPos;
                float pulse = 1f + 0.08f * Mathf.Sin(Time.time * 8f);
                ghostGo.transform.localScale = Vector3.one * Radius * 1.9f * pulse;
                ghostGo.GetComponent<Renderer>().sharedMaterial =
                    BubblePalette.CreateGhostMaterial(GetActiveColor());
            }
            else
            {
                ghostGo.SetActive(false);
            }
        }

        void Fire(Vector3 dir)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            Destroy(go.GetComponent<Collider>());
            active = go.AddComponent<Projectile>();

            var color = GetActiveColor();
            bool bomb = ActivePowerup == PowerupType.Bomb;

            if (ActivePowerup == PowerupType.Bomb) GameManager.I.BombsLeft--;
            else if (ActivePowerup == PowerupType.Fireball) GameManager.I.FireballsLeft--;
            else if (ActivePowerup == PowerupType.Lightning) GameManager.I.LightningsLeft--;

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.28f;
            trail.startWidth = Radius * 1.5f;
            trail.endWidth = 0f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = BubblePalette.Colors[(int)color];
            trail.endColor = new Color(0f, 0f, 0f, 0f);

            active.Launch(color, bomb, muzzle, dir, board, Radius);
            EffectsManager.I.Burst(muzzle, color);

            // Barrel recoil & muzzle flash
            StartCoroutine(RecoilBarrel(dir));
            StartCoroutine(FlashMuzzle());

            currentGo.transform.localScale = Vector3.zero;
            line.positionCount = 0;
            if (ghostGo != null) ghostGo.SetActive(false);

            if (color == BubbleColor.Fireball)
                AudioManager.I.Play("fire");
            else if (color == BubbleColor.Lightning)
                AudioManager.I.Play("zap");
            else
                AudioManager.I.Play("shoot");

            if (UIManager.I != null) UIManager.I.SetHud();
        }

        IEnumerator RecoilBarrel(Vector3 dir)
        {
            float dur = 0.12f;
            float el = 0f;
            Vector3 startPos = Vector3.zero;
            Vector3 kickPos = -dir.normalized * 0.35f;
            while (el < dur && barrel != null)
            {
                el += Time.deltaTime;
                float t = el / dur;
                barrel.transform.localPosition = Vector3.Lerp(kickPos, startPos, t);
                yield return null;
            }
            if (barrel != null) barrel.transform.localPosition = startPos;
        }

        IEnumerator FlashMuzzle()
        {
            if (muzzleLight == null) yield break;
            muzzleLight.enabled = true;
            yield return new WaitForSeconds(0.06f);
            if (muzzleLight != null) muzzleLight.enabled = false;
        }

        public void OnProjectileDone()
        {
            active = null;
            if (GameManager.I.State == GameState.Playing) Reload();
        }
    }
}
