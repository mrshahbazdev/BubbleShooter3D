using UnityEngine;

namespace BubbleShooter
{
    /// <summary>
    /// Cinematic camera rig mounted on the CameraPivot. The Main Camera is a CHILD of the
    /// pivot, and EffectsManager.Shake writes Camera.main.transform.localPosition directly
    /// in LateUpdate. To avoid fighting the shake, this rig moves ONLY the pivot Transform
    /// and never touches the camera's own transform.
    /// </summary>
    public class CameraRig : MonoBehaviour
    {
        public static CameraRig I { get; private set; }

        Transform pivot;
        Camera cam;

        Vector3 basePos;
        float baseRotX = 8f;

        float targetParallaxX;
        float targetPitchAdd;
        float parallaxXSmoothed;
        float pitchAddSmoothed;
        float zoomKick;

        /// <summary>Stores pivot/camera refs and records the pivot's rest pose.</summary>
        public void Init(Transform pivot, Camera cam)
        {
            I = this;
            this.pivot = pivot;
            this.cam = cam;
            if (pivot != null)
            {
                basePos = pivot.position;
                baseRotX = 8f;
            }
        }

        /// <summary>
        /// aimDir is a normalized 2D aim direction (x in [-1,1], y in [0,1]).
        /// Stores smoothed targets for lateral parallax and pitch offset.
        /// </summary>
        public void SetAim(Vector2 aimDir)
        {
            targetParallaxX = -aimDir.x * 0.7f;
            targetPitchAdd = aimDir.y * 2.5f;
        }

        /// <summary>
        /// Adds a dolly-in impulse toward the board (+z on the pivot). Positive only.
        /// </summary>
        public void KickZoom(float strength)
        {
            zoomKick = Mathf.Clamp(zoomKick + strength, 0f, 2.2f);
        }

        void LateUpdate()
        {
            if (pivot == null) return;

            float t = Time.time;
            float dt = Time.deltaTime;

            // Gentle idle sway, always on.
            float swayX = Mathf.Sin(t * 0.31f) * 0.16f;
            float swayY = Mathf.Sin(t * 0.23f + 1.7f) * 0.10f;

            // Smooth parallax / pitch toward aim targets.
            parallaxXSmoothed = Mathf.Lerp(parallaxXSmoothed, targetParallaxX, 6f * dt);
            pitchAddSmoothed = Mathf.Lerp(pitchAddSmoothed, targetPitchAdd, 6f * dt);

            // Zoom kick decays exponentially back to zero.
            zoomKick = Mathf.Lerp(zoomKick, 0f, dt * 2.2f);

            // Pivot only — never touch the camera's own transform (shake owns that).
            pivot.position = basePos + new Vector3(swayX + parallaxXSmoothed, swayY, zoomKick);
            pivot.rotation = Quaternion.Euler(
                baseRotX + pitchAddSmoothed + swayY * 0.4f,
                parallaxXSmoothed * 1.5f,
                0f);
        }
    }
}
