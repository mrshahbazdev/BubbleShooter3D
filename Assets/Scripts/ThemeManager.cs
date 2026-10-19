using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    [System.Serializable]
    public struct ThemeData
    {
        public string Name;
        public Color CameraSky;
        public Color Backdrop;
        public Color Floor;
        public Color RailEmission;
        public Color Starfield;
        public Color KeyLight;
        public Color FillLight;
        public Color AccentLeft;
        public Color AccentRight;
        public Color Ambient;
        public Color Accent;
        public Color CoilEmission;
    }

    /// <summary>
    /// Transforms the complete 3D scene visual identity per level:
    /// Sky, backdrop, rails neon glow, reflective floor, directional & accent lighting,
    /// starfield particles, and cannon energy coils with cinematic crossfading transitions.
    /// </summary>
    public class ThemeManager : MonoBehaviour
    {
        public static ThemeManager I { get; private set; }

        public static readonly ThemeData[] Themes =
        {
            new ThemeData
            {
                Name = "CYBERPUNK NEON",
                CameraSky = new Color(0.04f, 0.05f, 0.10f),
                Backdrop = new Color(0.06f, 0.08f, 0.15f),
                Floor = new Color(0.04f, 0.05f, 0.09f),
                RailEmission = new Color(0.10f, 0.85f, 1.00f),
                Starfield = new Color(0.35f, 0.85f, 1.00f),
                KeyLight = new Color(0.90f, 0.95f, 1.00f),
                FillLight = new Color(0.15f, 0.55f, 0.90f),
                AccentLeft = new Color(1.00f, 0.10f, 0.65f), // Hot pink
                AccentRight = new Color(0.10f, 0.90f, 1.00f), // Cyan
                Ambient = new Color(0.16f, 0.22f, 0.35f),
                Accent = new Color(0.20f, 0.85f, 1.00f),
                CoilEmission = new Color(0.15f, 0.85f, 1.00f)
            },
            new ThemeData
            {
                Name = "VOLCANIC MAGMA",
                CameraSky = new Color(0.11f, 0.03f, 0.02f),
                Backdrop = new Color(0.14f, 0.05f, 0.03f),
                Floor = new Color(0.09f, 0.04f, 0.03f),
                RailEmission = new Color(1.00f, 0.38f, 0.05f),
                Starfield = new Color(1.00f, 0.55f, 0.15f), // Fiery sparks
                KeyLight = new Color(1.00f, 0.85f, 0.65f),
                FillLight = new Color(0.85f, 0.15f, 0.05f),
                AccentLeft = new Color(1.00f, 0.40f, 0.00f), // Amber
                AccentRight = new Color(1.00f, 0.08f, 0.08f), // Crimson
                Ambient = new Color(0.35f, 0.14f, 0.08f),
                Accent = new Color(1.00f, 0.45f, 0.05f),
                CoilEmission = new Color(1.00f, 0.35f, 0.05f)
            },
            new ThemeData
            {
                Name = "EMERALD CITADEL",
                CameraSky = new Color(0.02f, 0.08f, 0.05f),
                Backdrop = new Color(0.04f, 0.11f, 0.07f),
                Floor = new Color(0.03f, 0.08f, 0.05f),
                RailEmission = new Color(0.15f, 0.95f, 0.45f),
                Starfield = new Color(0.40f, 1.00f, 0.55f), // Bioluminescent spores
                KeyLight = new Color(0.85f, 1.00f, 0.88f),
                FillLight = new Color(0.10f, 0.70f, 0.30f),
                AccentLeft = new Color(0.30f, 1.00f, 0.20f), // Lime
                AccentRight = new Color(0.10f, 0.85f, 0.75f), // Teal
                Ambient = new Color(0.10f, 0.28f, 0.16f),
                Accent = new Color(0.20f, 0.95f, 0.45f),
                CoilEmission = new Color(0.15f, 0.95f, 0.40f)
            },
            new ThemeData
            {
                Name = "AMETHYST VOID",
                CameraSky = new Color(0.07f, 0.03f, 0.11f),
                Backdrop = new Color(0.09f, 0.04f, 0.15f),
                Floor = new Color(0.06f, 0.03f, 0.10f),
                RailEmission = new Color(0.75f, 0.25f, 1.00f),
                Starfield = new Color(0.85f, 0.50f, 1.00f), // Violet stardust
                KeyLight = new Color(0.92f, 0.86f, 1.00f),
                FillLight = new Color(0.55f, 0.15f, 0.85f),
                AccentLeft = new Color(0.95f, 0.15f, 0.85f), // Magenta
                AccentRight = new Color(0.35f, 0.35f, 1.00f), // Indigo
                Ambient = new Color(0.25f, 0.12f, 0.34f),
                Accent = new Color(0.80f, 0.30f, 1.00f),
                CoilEmission = new Color(0.70f, 0.20f, 1.00f)
            },
            new ThemeData
            {
                Name = "SOLARIS GOLD TEMPLE",
                CameraSky = new Color(0.09f, 0.06f, 0.03f),
                Backdrop = new Color(0.13f, 0.09f, 0.04f),
                Floor = new Color(0.08f, 0.06f, 0.03f),
                RailEmission = new Color(1.00f, 0.82f, 0.12f),
                Starfield = new Color(1.00f, 0.90f, 0.40f), // Golden motes
                KeyLight = new Color(1.00f, 0.95f, 0.80f),
                FillLight = new Color(0.85f, 0.55f, 0.15f),
                AccentLeft = new Color(1.00f, 0.80f, 0.10f), // Gold
                AccentRight = new Color(0.95f, 0.35f, 0.10f), // Bronze
                Ambient = new Color(0.30f, 0.24f, 0.12f),
                Accent = new Color(1.00f, 0.82f, 0.15f),
                CoilEmission = new Color(1.00f, 0.80f, 0.10f)
            },
            new ThemeData
            {
                Name = "ARCTIC GLACIER",
                CameraSky = new Color(0.04f, 0.08f, 0.14f),
                Backdrop = new Color(0.07f, 0.11f, 0.18f),
                Floor = new Color(0.07f, 0.11f, 0.17f),
                RailEmission = new Color(0.55f, 0.92f, 1.00f),
                Starfield = new Color(0.85f, 0.95f, 1.00f), // Snow diamonds
                KeyLight = new Color(0.92f, 0.97f, 1.00f),
                FillLight = new Color(0.35f, 0.65f, 0.95f),
                AccentLeft = new Color(0.20f, 1.00f, 0.90f), // Frost cyan
                AccentRight = new Color(0.55f, 0.65f, 1.00f), // Periwinkle
                Ambient = new Color(0.18f, 0.28f, 0.42f),
                Accent = new Color(0.50f, 0.92f, 1.00f),
                CoilEmission = new Color(0.45f, 0.90f, 1.00f)
            },
            new ThemeData
            {
                Name = "RETRO SYNTHWAVE",
                CameraSky = new Color(0.09f, 0.03f, 0.09f),
                Backdrop = new Color(0.13f, 0.04f, 0.12f),
                Floor = new Color(0.08f, 0.03f, 0.09f),
                RailEmission = new Color(1.00f, 0.18f, 0.68f),
                Starfield = new Color(1.00f, 0.45f, 0.80f), // Neon glitter
                KeyLight = new Color(1.00f, 0.82f, 0.78f),
                FillLight = new Color(0.85f, 0.20f, 0.65f),
                AccentLeft = new Color(1.00f, 0.55f, 0.20f), // Peach sunset
                AccentRight = new Color(0.70f, 0.15f, 1.00f), // Purple neon
                Ambient = new Color(0.30f, 0.14f, 0.28f),
                Accent = new Color(1.00f, 0.25f, 0.70f),
                CoilEmission = new Color(1.00f, 0.20f, 0.65f)
            },
            new ThemeData
            {
                Name = "DEEP OCEAN ABYSS",
                CameraSky = new Color(0.02f, 0.04f, 0.08f),
                Backdrop = new Color(0.03f, 0.07f, 0.12f),
                Floor = new Color(0.02f, 0.05f, 0.09f),
                RailEmission = new Color(0.05f, 0.88f, 0.78f),
                Starfield = new Color(0.20f, 0.95f, 0.85f), // Plankton glow
                KeyLight = new Color(0.75f, 0.95f, 0.92f),
                FillLight = new Color(0.08f, 0.40f, 0.70f),
                AccentLeft = new Color(0.10f, 0.95f, 0.85f), // Aqua
                AccentRight = new Color(0.15f, 0.35f, 0.95f), // Deep ocean
                Ambient = new Color(0.08f, 0.20f, 0.32f),
                Accent = new Color(0.10f, 0.90f, 0.80f),
                CoilEmission = new Color(0.05f, 0.85f, 0.75f)
            }
        };

        Camera cam;
        Light keyLight, fillLight;
        Light accentLeft, accentRight;
        Material backdropMat, floorMat;
        List<Material> railMats = new List<Material>();
        List<Material> outerNeonMats = new List<Material>();
        ParticleSystem starfield;
        Shooter shooter;
        EnvironmentBuilder envBuilder;
        Coroutine transitionRoutine;

        public ThemeData CurrentTheme { get; private set; }

        public void Init(Camera cam, Light key, Light fill, Light accLeft, Light accRight,
                         Material backdrop, Material floor, List<Material> rails,
                         ParticleSystem starfield, Shooter shooter, List<Material> outerNeon = null,
                         EnvironmentBuilder env = null)
        {
            I = this;
            this.cam = cam;
            this.keyLight = key;
            this.fillLight = fill;
            this.accentLeft = accLeft;
            this.accentRight = accRight;
            this.backdropMat = backdrop;
            this.floorMat = floor;
            this.railMats = rails ?? new List<Material>();
            this.outerNeonMats = outerNeon ?? new List<Material>();
            this.starfield = starfield;
            this.shooter = shooter;
            this.envBuilder = env;
        }

        public ThemeData GetThemeForLevel(int level)
        {
            int idx = (Mathf.Max(1, level) - 1) % Themes.Length;
            return Themes[idx];
        }

        public void ApplyTheme(int level, bool immediate = false)
        {
            ThemeData target = GetThemeForLevel(level);
            CurrentTheme = target;

            if (immediate)
            {
                SetThemeImmediate(target);
            }
            else
            {
                if (transitionRoutine != null) StopCoroutine(transitionRoutine);
                transitionRoutine = StartCoroutine(CoCrossfadeTheme(target, 1.2f));
            }
        }

        void SetThemeImmediate(ThemeData t)
        {
            if (cam != null) cam.backgroundColor = t.CameraSky;
            if (keyLight != null) keyLight.color = t.KeyLight;
            if (fillLight != null) fillLight.color = t.FillLight;
            if (accentLeft != null) accentLeft.color = t.AccentLeft;
            if (accentRight != null) accentRight.color = t.AccentRight;
            RenderSettings.ambientLight = t.Ambient;

            if (backdropMat != null) backdropMat.color = t.Backdrop;
            if (floorMat != null) floorMat.color = t.Floor;

            foreach (var m in railMats)
            {
                if (m == null) continue;
                m.color = t.RailEmission * 0.7f;
                m.SetColor("_EmissionColor", t.RailEmission * 0.8f);
            }

            foreach (var m in outerNeonMats)
            {
                if (m == null) continue;
                m.color = t.Accent * 0.8f;
                m.SetColor("_EmissionColor", t.Accent * 0.85f);
            }

            if (starfield != null)
            {
                var main = starfield.main;
                main.startColor = t.Starfield;
            }

            if (envBuilder != null) envBuilder.ApplyTheme(t);

            UpdateShooterTheme(t);
        }

        IEnumerator CoCrossfadeTheme(ThemeData target, float duration)
        {
            Color fromCam = cam != null ? cam.backgroundColor : target.CameraSky;
            Color fromKey = keyLight != null ? keyLight.color : target.KeyLight;
            Color fromFill = fillLight != null ? fillLight.color : target.FillLight;
            Color fromAccL = accentLeft != null ? accentLeft.color : target.AccentLeft;
            Color fromAccR = accentRight != null ? accentRight.color : target.AccentRight;
            Color fromAmb = RenderSettings.ambientLight;
            Color fromBack = backdropMat != null ? backdropMat.color : target.Backdrop;
            Color fromFloor = floorMat != null ? floorMat.color : target.Floor;
            Color fromRail = (railMats.Count > 0 && railMats[0] != null) ? railMats[0].GetColor("_EmissionColor") : target.RailEmission;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float p = Mathf.SmoothStep(0f, 1f, elapsed / duration);

                if (cam != null) cam.backgroundColor = Color.Lerp(fromCam, target.CameraSky, p);
                if (keyLight != null) keyLight.color = Color.Lerp(fromKey, target.KeyLight, p);
                if (fillLight != null) fillLight.color = Color.Lerp(fromFill, target.FillLight, p);
                if (accentLeft != null) accentLeft.color = Color.Lerp(fromAccL, target.AccentLeft, p);
                if (accentRight != null) accentRight.color = Color.Lerp(fromAccR, target.AccentRight, p);
                RenderSettings.ambientLight = Color.Lerp(fromAmb, target.Ambient, p);

                if (backdropMat != null) backdropMat.color = Color.Lerp(fromBack, target.Backdrop, p);
                if (floorMat != null) floorMat.color = Color.Lerp(fromFloor, target.Floor, p);

                Color currentRail = Color.Lerp(fromRail, target.RailEmission, p);
                foreach (var m in railMats)
                {
                    if (m == null) continue;
                    m.color = currentRail * 0.7f;
                    m.SetColor("_EmissionColor", currentRail * 0.8f);
                }

                Color currentOuter = Color.Lerp(fromRail, target.Accent, p);
                foreach (var m in outerNeonMats)
                {
                    if (m == null) continue;
                    m.color = currentOuter * 0.8f;
                    m.SetColor("_EmissionColor", currentOuter * 0.85f);
                }

                yield return null;
            }

            SetThemeImmediate(target);
            transitionRoutine = null;
        }

        void UpdateShooterTheme(ThemeData t)
        {
            if (shooter == null) return;
            var coil = shooter.transform.Find("Barrel/EnergyCoil");
            if (coil != null)
            {
                var cm = coil.GetComponent<Renderer>()?.material;
                if (cm != null)
                {
                    cm.color = t.CoilEmission;
                    cm.SetColor("_EmissionColor", t.CoilEmission * 0.9f);
                }
            }
            var core = shooter.transform.Find("Barrel/PowerCore");
            if (core != null)
            {
                var corm = core.GetComponent<Renderer>()?.material;
                if (corm != null)
                {
                    corm.color = t.CoilEmission;
                    corm.SetColor("_EmissionColor", t.CoilEmission * 0.85f);
                }
            }
            var ring = shooter.transform.Find("Barrel/MuzzleRing");
            if (ring != null)
            {
                var rm = ring.GetComponent<Renderer>()?.material;
                if (rm != null)
                {
                    rm.color = t.CoilEmission;
                    rm.SetColor("_EmissionColor", t.CoilEmission * 0.75f);
                }
            }
        }
    }
}
