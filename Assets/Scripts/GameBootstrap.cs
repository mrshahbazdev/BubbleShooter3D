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
            foreach (var (x, c) in new[] { (-9f, new Color(0.1f, 0.7f, 1f)), (9f, new Color(0.9f, 0.2f, 0.7f)) })
            {
                var pl = new GameObject("AccentLight").AddComponent<Light>();
                pl.type = LightType.Point;
                pl.color = c;
                pl.intensity = 1.6f;
                pl.range = 14f;
                pl.transform.position = new Vector3(x, 1f, -4f);
            }

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

            // Side rails (visual walls the shots bounce off)
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

            // Managers
            gameObject.AddComponent<AudioManager>();
            gameObject.AddComponent<EffectsManager>();
            gameObject.AddComponent<UIManager>();
            var gm = gameObject.AddComponent<GameManager>();
            var shooter = new GameObject("Shooter").AddComponent<Shooter>();
            shooter.Init(board, cam);
            gm.Init(board, shooter);
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
