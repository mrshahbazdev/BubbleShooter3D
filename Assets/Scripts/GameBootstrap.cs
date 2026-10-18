using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Entry point: builds the entire game (camera, lights, backdrop, board,
    /// shooter, managers, UI) procedurally so the project needs no assets.
    /// </summary>
    public class GameBootstrap : MonoBehaviour
    {
        void Awake()
        {
            Application.targetFrameRate = 120;

            // Camera — perspective, slight tilt for the 3D look
            var camGo = new GameObject("Main Camera");
            camGo.tag = "MainCamera";
            var cam = camGo.AddComponent<Camera>();
            cam.fieldOfView = 55f;
            camGo.transform.position = new Vector3(0f, 0.8f, -16f);
            camGo.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(0.05f, 0.06f, 0.10f);
            camGo.AddComponent<AudioListener>();

            // Lighting
            var key = new GameObject("KeyLight").AddComponent<Light>();
            key.type = LightType.Directional;
            key.intensity = 1.15f;
            key.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
            key.shadows = LightShadows.Soft;
            var fill = new GameObject("FillLight").AddComponent<Light>();
            fill.type = LightType.Directional;
            fill.intensity = 0.45f;
            fill.color = new Color(0.6f, 0.75f, 1f);
            fill.transform.rotation = Quaternion.Euler(20f, 140f, 0f);
            RenderSettings.ambientLight = new Color(0.22f, 0.24f, 0.32f);

            // Accent point lights for depth (cyan left, magenta right)
            Light accLeft = null, accRight = null;
            var acc1 = new GameObject("AccentLightLeft").AddComponent<Light>();
            acc1.type = LightType.Point;
            acc1.color = new Color(0.1f, 0.7f, 1f);
            acc1.intensity = 1.6f;
            acc1.range = 14f;
            acc1.transform.position = new Vector3(-9f, 1f, -4f);
            accLeft = acc1;

            var acc2 = new GameObject("AccentLightRight").AddComponent<Light>();
            acc2.type = LightType.Point;
            acc2.color = new Color(0.9f, 0.2f, 0.7f);
            acc2.intensity = 1.6f;
            acc2.range = 14f;
            acc2.transform.position = new Vector3(9f, 1f, -4f);
            accRight = acc2;

            // Starfield behind the board
            var stars = new GameObject("Starfield");
            stars.transform.position = new Vector3(0f, 1.6f, 6f);
            var sps = stars.AddComponent<ParticleSystem>();
            var smain = sps.main;
            smain.startLifetime = 999f;
            smain.startSpeed = 0f;
            smain.startSize = new ParticleSystem.MinMaxCurve(0.05f, 0.18f);
            smain.startColor = new Color(1f, 1f, 1f, 0.85f);
            smain.maxParticles = 160;
            var sem = sps.emission;
            sem.rateOverTime = 0;
            sem.SetBursts(new[] { new ParticleSystem.Burst(0f, 140) });
            var sshape = sps.shape;
            sshape.shapeType = ParticleSystemShapeType.Box;
            sshape.scale = new Vector3(24f, 18f, 1f);
            stars.GetComponent<ParticleSystemRenderer>().material =
                new Material(Shader.Find("Particles/Standard Unlit"));

            // Backdrop panel behind the board
            var back = GameObject.CreatePrimitive(PrimitiveType.Cube);
            back.name = "Backdrop";
            Destroy(back.GetComponent<Collider>());
            back.transform.position = new Vector3(0f, 1.6f, 1.2f);
            back.transform.localScale = new Vector3(16f, 16f, 0.3f);
            var backMat = back.GetComponent<Renderer>().material;
            backMat.color = new Color(0.08f, 0.10f, 0.16f);
            backMat.SetFloat("_Glossiness", 0.3f);

            // Glossy floor the scene reflects on
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            Destroy(floor.GetComponent<Collider>());
            floor.transform.position = new Vector3(0f, -5.4f, 2f);
            floor.transform.localScale = new Vector3(22f, 0.25f, 10f);
            var fm = floor.GetComponent<Renderer>().material;
            fm.color = new Color(0.07f, 0.08f, 0.12f);
            fm.SetFloat("_Glossiness", 0.9f);
            fm.SetFloat("_Metallic", 0.6f);

            // Side rails & ceiling
            var railMats = new System.Collections.Generic.List<Material>();
            var board = new GameObject("Board").AddComponent<BubbleBoard>();
            board.Init();
            foreach (float side in new[] { -1f, 1f })
            {
                var rail = GameObject.CreatePrimitive(PrimitiveType.Cube);
                rail.name = side < 0 ? "LeftRail" : "RightRail";
                Destroy(rail.GetComponent<Collider>());
                float x = side < 0 ? board.LeftWallX : board.RightWallX;
                rail.transform.position = new Vector3(x + side * -0.15f, 1.5f, 0f);
                rail.transform.localScale = new Vector3(0.3f, 22f, 0.4f);
                var rm = rail.GetComponent<Renderer>().material;
                rm.color = new Color(0.15f, 0.55f, 0.9f);
                rm.EnableKeyword("_EMISSION");
                rm.SetColor("_EmissionColor", new Color(0.1f, 0.35f, 0.6f));
                railMats.Add(rm);
            }
            // Ceiling bar
            var ceil = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ceil.name = "Ceiling";
            Destroy(ceil.GetComponent<Collider>());
            ceil.transform.position = new Vector3(0f, board.TopY + 0.5f, 0f);
            ceil.transform.localScale = new Vector3(board.RightWallX - board.LeftWallX + 0.9f, 0.35f, 0.4f);
            var cm = ceil.GetComponent<Renderer>().material;
            cm.color = new Color(0.15f, 0.55f, 0.9f);
            cm.EnableKeyword("_EMISSION");
            cm.SetColor("_EmissionColor", new Color(0.1f, 0.35f, 0.6f));
            railMats.Add(cm);

            // Death line (subtle red glow)
            var dl = GameObject.CreatePrimitive(PrimitiveType.Cube);
            dl.name = "DeathLine";
            Destroy(dl.GetComponent<Collider>());
            dl.transform.position = new Vector3(0f, board.DeathLineY, 0f);
            dl.transform.localScale = new Vector3(board.RightWallX - board.LeftWallX, 0.06f, 0.2f);
            var dm = dl.GetComponent<Renderer>().material;
            dm.color = new Color(0.9f, 0.15f, 0.2f);
            dm.EnableKeyword("_EMISSION");
            dm.SetColor("_EmissionColor", new Color(0.5f, 0.05f, 0.1f));
            StartCoroutine(PulseDeathLine(dl));

            // 3D Outer Arena Architecture & Collector Pods
            var outerNeonMats = new System.Collections.Generic.List<Material>();
            BuildOuterStadium(board, outerNeonMats);

            // Managers
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<EffectsManager>();
            var tm = gameObject.AddComponent<ThemeManager>();
            gameObject.AddComponent<UIManager>();
            var gm = gameObject.AddComponent<GameManager>();
            var shooter = new GameObject("Shooter").AddComponent<Shooter>();
            shooter.Init(board, cam);
            tm.Init(cam, key, fill, accLeft, accRight, backMat, fm, railMats, sps, shooter, outerNeonMats);
            gm.Init(board, shooter);
        }

        void BuildOuterStadium(BubbleBoard board, System.Collections.Generic.List<Material> outerNeon)
        {
            var arenaRoot = new GameObject("OuterStadium");
            arenaRoot.transform.position = Vector3.zero;

            // Shared metallic material for heavy structural beams
            var trussMat = new Material(Shader.Find("Standard"));
            trussMat.color = new Color(0.12f, 0.14f, 0.18f);
            trussMat.SetFloat("_Metallic", 0.85f);
            trussMat.SetFloat("_Glossiness", 0.72f);

            // 1. Left and Right Massive Pylon Towers
            foreach (float side in new[] { -1f, 1f })
            {
                float x = side * 5.35f;

                // Main vertical column
                var pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                pylon.name = side < 0 ? "LeftPylon" : "RightPylon";
                Destroy(pylon.GetComponent<Collider>());
                pylon.transform.SetParent(arenaRoot.transform);
                pylon.transform.position = new Vector3(x, 1.2f, 0.5f);
                pylon.transform.localScale = new Vector3(0.85f, 7.5f, 0.85f);
                pylon.GetComponent<Renderer>().sharedMaterial = trussMat;

                // Glowing Neon Collars along each column
                float[] ringYs = { -2.5f, 0.2f, 2.8f, 5.5f };
                foreach (float ry in ringYs)
                {
                    var collar = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                    Destroy(collar.GetComponent<Collider>());
                    collar.transform.SetParent(arenaRoot.transform);
                    collar.transform.position = new Vector3(x, ry, 0.5f);
                    collar.transform.localScale = new Vector3(1.15f, 0.12f, 1.15f);
                    var colMat = new Material(Shader.Find("Standard"));
                    colMat.color = new Color(0.2f, 0.85f, 1f);
                    colMat.EnableKeyword("_EMISSION");
                    colMat.SetColor("_EmissionColor", new Color(0.15f, 0.75f, 1f));
                    collar.GetComponent<Renderer>().material = colMat;
                    outerNeon.Add(colMat);
                }

                // Diagonal support trusses bracing to the rails
                float[] trussYs = { 3.8f, -0.6f };
                foreach (float ty in trussYs)
                {
                    var truss = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(truss.GetComponent<Collider>());
                    truss.transform.SetParent(arenaRoot.transform);
                    truss.transform.position = new Vector3(side * 4.45f, ty, 0.25f);
                    truss.transform.rotation = Quaternion.Euler(0f, 0f, side * 36f);
                    truss.transform.localScale = new Vector3(0.22f, 2.0f, 0.22f);
                    truss.GetComponent<Renderer>().sharedMaterial = trussMat;
                }

                // Top rotating hazard siren beacons
                var beaconBase = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                Destroy(beaconBase.GetComponent<Collider>());
                beaconBase.transform.SetParent(arenaRoot.transform);
                beaconBase.transform.position = new Vector3(x, 8.45f, 0.5f);
                beaconBase.transform.localScale = new Vector3(0.5f, 0.15f, 0.5f);
                beaconBase.GetComponent<Renderer>().sharedMaterial = trussMat;

                var beacon = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                beacon.name = "HazardBeacon";
                Destroy(beacon.GetComponent<Collider>());
                beacon.transform.SetParent(arenaRoot.transform);
                beacon.transform.position = new Vector3(x, 8.75f, 0.5f);
                beacon.transform.localScale = Vector3.one * 0.38f;
                var bm = new Material(Shader.Find("Standard"));
                bm.color = new Color(1f, 0.25f, 0.1f);
                bm.EnableKeyword("_EMISSION");
                bm.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.1f) * 1.5f);
                beacon.GetComponent<Renderer>().material = bm;
                StartCoroutine(PulseBeacon(beacon));

                // Outer Cyber Chevrons along flanks
                float[] chevYs = { -1.5f, 1.2f, 4.0f };
                foreach (float cy in chevYs)
                {
                    var chev = GameObject.CreatePrimitive(PrimitiveType.Cube);
                    Destroy(chev.GetComponent<Collider>());
                    chev.transform.SetParent(arenaRoot.transform);
                    chev.transform.position = new Vector3(side * 7.0f, cy, 0.8f);
                    chev.transform.rotation = Quaternion.Euler(0f, 0f, side * 24f);
                    chev.transform.localScale = new Vector3(1.5f, 0.1f, 0.15f);
                    var chMat = new Material(Shader.Find("Standard"));
                    chMat.color = new Color(0.2f, 0.85f, 1f);
                    chMat.EnableKeyword("_EMISSION");
                    chMat.SetColor("_EmissionColor", new Color(0.15f, 0.65f, 0.95f));
                    chev.GetComponent<Renderer>().material = chMat;
                    outerNeon.Add(chMat);
                }
            }

            // 2. Overhead Arena Arch Gantry (Top Stadium Crown)
            var archBeam = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archBeam.name = "ArchBeam";
            Destroy(archBeam.GetComponent<Collider>());
            archBeam.transform.SetParent(arenaRoot.transform);
            archBeam.transform.position = new Vector3(0f, 7.8f, 0.5f);
            archBeam.transform.localScale = new Vector3(11.8f, 0.6f, 0.8f);
            archBeam.GetComponent<Renderer>().sharedMaterial = trussMat;

            // Arch neon visor line
            var archNeon = GameObject.CreatePrimitive(PrimitiveType.Cube);
            archNeon.name = "ArchNeon";
            Destroy(archNeon.GetComponent<Collider>());
            archNeon.transform.SetParent(arenaRoot.transform);
            archNeon.transform.position = new Vector3(0f, 7.8f, 0.05f);
            archNeon.transform.localScale = new Vector3(10.6f, 0.16f, 0.15f);
            var anMat = new Material(Shader.Find("Standard"));
            anMat.color = new Color(0.2f, 0.85f, 1f);
            anMat.EnableKeyword("_EMISSION");
            anMat.SetColor("_EmissionColor", new Color(0.15f, 0.75f, 1f));
            archNeon.GetComponent<Renderer>().material = anMat;
            outerNeon.Add(anMat);

            // 3. Bottom 3D Score Collector Pods (The 5 Funnels)
            float[] podX = { -3.2f, -1.6f, 0.0f, 1.6f, 3.2f };
            int[] podVals = { 50, 150, 500, 150, 50 };

            for (int i = 0; i < podX.Length; i++)
            {
                float px = podX[i];
                int val = podVals[i];
                bool isJackpot = val == 500;

                // Pod housing bin
                var pod = GameObject.CreatePrimitive(PrimitiveType.Cube);
                pod.name = $"CollectorPod_{val}_{i}";
                Destroy(pod.GetComponent<Collider>());
                pod.transform.SetParent(arenaRoot.transform);
                pod.transform.position = new Vector3(px, -3.95f, 0.2f);
                pod.transform.localScale = new Vector3(1.35f, 0.75f, 0.75f);
                pod.GetComponent<Renderer>().sharedMaterial = trussMat;

                // Pod glowing neon rim
                var rim = GameObject.CreatePrimitive(PrimitiveType.Cube);
                Destroy(rim.GetComponent<Collider>());
                rim.transform.SetParent(pod.transform);
                rim.transform.localPosition = new Vector3(0f, 0.45f, 0f);
                rim.transform.localScale = new Vector3(1.06f, 0.14f, 1.06f);
                var rm = new Material(Shader.Find("Standard"));
                if (isJackpot)
                {
                    rm.color = new Color(1f, 0.85f, 0.15f);
                    rm.EnableKeyword("_EMISSION");
                    rm.SetColor("_EmissionColor", new Color(1f, 0.8f, 0.1f) * 1.4f);
                }
                else
                {
                    rm.color = new Color(0.2f, 0.85f, 1f);
                    rm.EnableKeyword("_EMISSION");
                    rm.SetColor("_EmissionColor", new Color(0.15f, 0.75f, 1f));
                    outerNeon.Add(rm);
                }
                rim.GetComponent<Renderer>().material = rm;

                // 3D Score text label on front
                var textGo = new GameObject("Label");
                textGo.transform.SetParent(pod.transform, false);
                textGo.transform.localPosition = new Vector3(0f, 0.05f, -0.52f);
                var tm = textGo.AddComponent<TextMesh>();
                tm.text = isJackpot ? "★ 500 ★\nJACKPOT" : val.ToString();
                tm.fontSize = isJackpot ? 26 : 24;
                tm.characterSize = 0.045f;
                tm.alignment = TextAlignment.Center;
                tm.anchor = TextAnchor.MiddleCenter;
                tm.color = isJackpot ? new Color(1f, 0.88f, 0.2f) : new Color(0.4f, 0.95f, 1f);
            }
        }

        static System.Collections.IEnumerator PulseBeacon(GameObject beacon)
        {
            var r = beacon.GetComponent<Renderer>().material;
            float t = 0f;
            while (beacon != null)
            {
                t += Time.deltaTime;
                float k = 0.5f + 0.5f * Mathf.Sin(t * 5f);
                r.SetColor("_EmissionColor", new Color(1f, 0.35f, 0.08f) * (0.8f + k * 1.5f));
                yield return null;
            }
        }

        static System.Collections.IEnumerator PulseDeathLine(GameObject dl)
        {
            var r = dl.GetComponent<Renderer>().material;
            float t = 0f;
            while (dl != null)
            {
                t += Time.deltaTime;
                float k = 0.35f + 0.25f * Mathf.Sin(t * 3f);
                r.SetColor("_EmissionColor", new Color(k + 0.2f, k * 0.15f, k * 0.2f));
                yield return null;
            }
        }
    }
}
