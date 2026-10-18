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
        public int TrajectoryBounces = 8;

        BubbleBoard board;
        GameObject currentGo, nextGo, ghostGo;
        GameObject barrel;
        Light muzzleLight;
        LineRenderer line;
        Projectile active;
        Camera cam;
        Vector3 muzzle => barrel.transform.position + barrel.transform.up * 0.9f;

        const int MAX_TRAJECTORY_DOTS = 70;
        GameObject[] trajDots;
        Material trajDotMat;
        GameObject trajDotsRoot;

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

            // central glowing power core
            var core = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            core.name = "PowerCore";
            Destroy(core.GetComponent<Collider>());
            core.transform.SetParent(barrel.transform, false);
            core.transform.localPosition = new Vector3(0f, 0.08f, 0f);
            core.transform.localScale = Vector3.one * 0.75f;
            var corm = core.GetComponent<Renderer>().material;
            corm.color = new Color(0.15f, 0.75f, 1f);
            corm.EnableKeyword("_EMISSION");
            corm.SetColor("_EmissionColor", new Color(0.2f, 0.7f, 1f));

            var shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.name = "Shaft";
            Destroy(shaft.GetComponent<Collider>());
            shaft.transform.SetParent(barrel.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            shaft.transform.localScale = new Vector3(0.5f, 0.45f, 0.5f);
            var sm = shaft.GetComponent<Renderer>().material;
            sm.color = new Color(0.22f, 0.24f, 0.30f);
            sm.SetFloat("_Glossiness", 0.8f);

            // glowing mid-barrel energy coil
            var coil = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coil.name = "EnergyCoil";
            Destroy(coil.GetComponent<Collider>());
            coil.transform.SetParent(barrel.transform, false);
            coil.transform.localPosition = new Vector3(0f, 0.48f, 0f);
            coil.transform.localScale = new Vector3(0.58f, 0.06f, 0.58f);
            var cm = coil.GetComponent<Renderer>().material;
            cm.color = new Color(0.2f, 0.9f, 1f);
            cm.EnableKeyword("_EMISSION");
            cm.SetColor("_EmissionColor", new Color(0.15f, 0.65f, 0.95f));

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

            // animated trajectory dots pool
            trajDotsRoot = new GameObject("TrajectoryDots");
            trajDotsRoot.transform.SetParent(transform, false);
            trajDots = new GameObject[MAX_TRAJECTORY_DOTS];
            trajDotMat = new Material(Shader.Find("Standard"));
            trajDotMat.SetFloat("_Glossiness", 0.95f);
            trajDotMat.EnableKeyword("_EMISSION");

            for (int i = 0; i < MAX_TRAJECTORY_DOTS; i++)
            {
                var dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                dot.name = "TrajDot_" + i;
                Destroy(dot.GetComponent<Collider>());
                dot.transform.SetParent(trajDotsRoot.transform, false);
                dot.transform.localScale = Vector3.one * 0.14f;
                dot.GetComponent<Renderer>().sharedMaterial = trajDotMat;
                dot.SetActive(false);
                trajDots[i] = dot;
            }

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
            BubblePalette.Add3DOrnaments(currentGo, Radius, Current);
            BubblePalette.Add3DOrnaments(nextGo, Radius, Next);

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
            UpdateNextMaterial();
            if (UIManager.I != null) UIManager.I.SetHud();
        }

        void UpdateCurrentMaterial()
        {
            BubbleColor c = GetActiveColor();
            currentGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(c);
            var core = currentGo.transform.Find("InnerCore");
            if (core != null)
            {
                var cm = core.GetComponent<Renderer>().material;
                cm.color = BubblePalette.Colors[(int)c];
                cm.SetColor("_EmissionColor", BubblePalette.Colors[(int)c] * 0.7f);
            }
        }

        void UpdateNextMaterial()
        {
            nextGo.GetComponent<Renderer>().sharedMaterial = BubblePalette.CreateMaterial(Next);
            var nextCore = nextGo.transform.Find("InnerCore");
            if (nextCore != null)
            {
                var nm = nextCore.GetComponent<Renderer>().material;
                nm.color = BubblePalette.Colors[(int)Next];
                nm.SetColor("_EmissionColor", BubblePalette.Colors[(int)Next] * 0.7f);
            }
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
            UpdateNextMaterial();
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
            float angle = Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg;
            // Real arcade clamp: 16 degrees minimum from horizontal
            // prevents near-horizontal stall while allowing deep bank shots
            angle = Mathf.Clamp(angle, 16f, 164f);
            float rad = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
        }

        void Update()
        {
            if (Busy || GameManager.I.State != GameState.Playing || GameManager.I.IsPaused)
            {
                line.positionCount = 0;
                HideTrajectoryDots();
                if (ghostGo != null) ghostGo.SetActive(false);
                return;
            }

            Vector3 d = AimDir();
            barrel.transform.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(d.y, d.x) * Mathf.Rad2Deg - 90f);
            currentGo.transform.position = muzzle;

            DrawTrajectoryAndGhost(muzzle, d);

            bool isOverButton = false;
            if (UnityEngine.EventSystems.EventSystem.current != null)
            {
                var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current)
                {
                    position = Input.mousePosition
                };
                var results = new List<UnityEngine.EventSystems.RaycastResult>();
                UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);
                for (int i = 0; i < results.Count; i++)
                {
                    if (results[i].gameObject.GetComponentInParent<UnityEngine.UI.Button>() != null)
                    {
                        isOverButton = true;
                        break;
                    }
                }
            }

            if (Input.GetMouseButtonDown(0) && !isOverButton) Fire(d);
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

            for (int guard = 0; guard < 700; guard++)
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

            Color activeCol = BubblePalette.Colors[(int)GetActiveColor()];
            line.startColor = new Color(activeCol.r, activeCol.g, activeCol.b, 0.22f);
            line.endColor = new Color(activeCol.r, activeCol.g, activeCol.b, 0.05f);
            line.startWidth = line.endWidth = 0.035f;
            line.positionCount = pts.Count;
            line.SetPositions(pts.ToArray());

            DrawAnimatedDots(pts);

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

        void DrawAnimatedDots(List<Vector3> pts)
        {
            if (pts == null || pts.Count < 2 || trajDots == null)
            {
                HideTrajectoryDots();
                return;
            }

            float totalDist = 0f;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                totalDist += Vector3.Distance(pts[i], pts[i + 1]);
            }

            float dotSpacing = 0.38f;
            float animSpeed = 4.6f;
            float animOffset = (Time.time * animSpeed) % dotSpacing;

            Color activeCol = BubblePalette.Colors[(int)GetActiveColor()];
            trajDotMat.color = activeCol;
            trajDotMat.SetColor("_EmissionColor", activeCol * 0.95f);

            int dotIdx = 0;
            float currentDist = animOffset;

            while (currentDist < totalDist && dotIdx < MAX_TRAJECTORY_DOTS)
            {
                Vector3 dotPos = SamplePolyline(pts, currentDist);
                var dot = trajDots[dotIdx];
                dot.transform.position = dotPos;

                float progress = currentDist / Mathf.Max(0.01f, totalDist);
                float pulse = 1f + 0.18f * Mathf.Sin(Time.time * 9f - dotIdx * 0.35f);
                float sz = Mathf.Lerp(0.14f, 0.08f, progress) * pulse;
                dot.transform.localScale = Vector3.one * sz;

                if (!dot.activeSelf) dot.SetActive(true);
                dotIdx++;
                currentDist += dotSpacing;
            }

            for (int i = dotIdx; i < MAX_TRAJECTORY_DOTS; i++)
            {
                if (trajDots[i].activeSelf) trajDots[i].SetActive(false);
            }
        }

        Vector3 SamplePolyline(List<Vector3> pts, float dist)
        {
            float accumulated = 0f;
            for (int i = 0; i < pts.Count - 1; i++)
            {
                float segLen = Vector3.Distance(pts[i], pts[i + 1]);
                if (accumulated + segLen >= dist)
                {
                    float segT = (dist - accumulated) / Mathf.Max(0.0001f, segLen);
                    return Vector3.Lerp(pts[i], pts[i + 1], segT);
                }
                accumulated += segLen;
            }
            return pts[pts.Count - 1];
        }

        void HideTrajectoryDots()
        {
            if (trajDots == null) return;
            for (int i = 0; i < trajDots.Length; i++)
            {
                if (trajDots[i] != null && trajDots[i].activeSelf)
                    trajDots[i].SetActive(false);
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
            HideTrajectoryDots();
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
