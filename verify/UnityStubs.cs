// Minimal UnityEngine / UnityEngine.UI stubs for compile verification only.
// These signatures mirror the real Unity API surface used by the game scripts.
#pragma warning disable
using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name;
        public static void Destroy(Object o) { }
        public static void Destroy(Object o, float t) { }
        public static T Instantiate<T>(T o) where T : Object
        {
            if (o is GameObject g)
            {
                var n = new GameObject(g.name);
                n._comps.Add(new Renderer());
                return (T)(object)n;
            }
            return o;
        }
        public static T Instantiate<T>(T o, Transform parent) where T : Object => Instantiate(o);
        public static T FindObjectOfType<T>() where T : class => null;
        public static implicit operator bool(Object o) => o != null;
    }

    public struct Vector2 { public float x, y; public Vector2(float x, float y) { this.x = x; this.y = y; } public static Vector2 zero => new Vector2(0, 0); public static Vector2 one => new Vector2(1, 1); }
    public struct Vector2Int : IEquatable<Vector2Int>
    {
        public int x, y;
        public Vector2Int(int x, int y) { this.x = x; this.y = y; }
        public bool Equals(Vector2Int o) => x == o.x && y == o.y;
        public override bool Equals(object o) => o is Vector2Int v && Equals(v);
        public override int GetHashCode() => x * 397 ^ y;
    }
    public struct Vector3
    {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero => new Vector3(0, 0, 0);
        public static Vector3 one => new Vector3(1, 1, 1);
        public static Vector3 up => new Vector3(0, 1, 0);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator *(float d, Vector3 a) => a * d;
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
        public override bool Equals(object o) => o is Vector3 v && this == v;
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode();
        public static Vector3 forward => new Vector3(0, 0, 1);
        public static Vector3 right => new Vector3(1, 0, 0);
        public static implicit operator Vector2(Vector3 v) => new Vector2(v.x, v.y);
        public float sqrMagnitude => x * x + y * y + z * z;
        public Vector3 normalized => this;
        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
        public float magnitude => (float)Math.Sqrt(sqrMagnitude);
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * Mathf.Clamp01(t);
    }
    public struct Vector4
    {
        public float x, y, z, w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }
    public struct Quaternion
    {
        public static Quaternion Euler(float x, float y, float z) => new Quaternion();
        public static Quaternion identity => new Quaternion();
    }
    public struct Color
    {
        public float r, g, b, a;
        public Color(float r, float g, float b) { this.r = r; this.g = g; this.b = b; a = 1; }
        public Color(float r, float g, float b, float a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => new Color(1, 1, 1);
        public static Color black => new Color(0, 0, 0);
        public static Color clear => new Color(0, 0, 0, 0);
        public static Color red => new Color(1, 0, 0);
        public static Color green => new Color(0, 1, 0);
        public static Color blue => new Color(0, 0, 1);
        public static Color cyan => new Color(0, 1, 1);
        public static Color yellow => new Color(1, 1, 0);
        public static Color magenta => new Color(1, 0, 1);
        public static Color gray => new Color(0.5f, 0.5f, 0.5f);
        public static Color operator *(Color c, float d) => new Color(c.r * d, c.g * d, c.b * d, c.a);
        public static Color Lerp(Color a, Color b, float t) => a;
    }
    public static class Mathf
    {
        public const float Rad2Deg = 57.29578f;
        public const float PI = 3.14159274f;
        public static float Min(float a, float b) => Math.Min(a, b);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static float Clamp01(float v) => v < 0 ? 0 : v > 1 ? 1 : v;
        public static float Clamp(float v, float a, float b) => v < a ? a : v > b ? b : v;
        public static int Clamp(int v, int a, int b) => v < a ? a : v > b ? b : v;
        public static int RoundToInt(float v) => (int)Math.Round(v);
        public static int FloorToInt(float v) => (int)Math.Floor(v);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Sin(float v) => (float)Math.Sin(v);
        public static float Cos(float v) => (float)Math.Cos(v);
        public static float Tan(float v) => (float)Math.Tan(v);
        public static float Sign(float v) => Math.Sign(v);
        public static float Abs(float v) => Math.Abs(v);
        public static int Abs(int v) => Math.Abs(v);
        public static float Exp(float v) => (float)Math.Exp(v);
        public static float PingPong(float t, float len) => t;
        public const float Deg2Deg = 1f;
        public const float Deg2Rad = 0.0174532924f;
        public static float Sqrt(float v) => (float)Math.Sqrt(v);
        public static float PerlinNoise(float x, float y) => 0.5f;
        public static int CeilToInt(float v) => (int)Math.Ceiling(v);
        public static float SmoothStep(float a, float b, float t) => a;
    }
    public static class Random
    {
        public static float value => 0.5f;
        public static int Range(int a, int b) => a;
        public static float Range(float a, float b) => a;
        public static Vector3 insideUnitSphere => Vector3.one;
    }
    public static class Time
    {
        public static float deltaTime => 0.016f;
        public static float unscaledDeltaTime => 0.016f;
        public static float time => 100f;
        public static float timeScale { get; set; } = 1f;
    }
    public class WaitForSeconds { public WaitForSeconds(float t) { } }
    public static class Application { public static int targetFrameRate { get; set; } }
    public static class RenderSettings
    {
        public static Color ambientLight { get; set; }
        public static bool fog { get; set; }
        public static Color fogColor { get; set; }
        public static float fogDensity { get; set; }
        public static FogMode fogMode { get; set; }
    }
    public static class Input
    {
        public static Vector3 mousePosition => Vector3.zero;
        public static bool GetMouseButtonDown(int b) => false;
        public static bool GetKeyDown(KeyCode k) => false;
        public static int touchCount => 0;
        public static Touch GetTouch(int i) => new Touch();
    }
    public enum KeyCode { X, B, F, L, V, M, H, Escape }
    public enum PrimitiveType { Sphere, Cube, Cylinder, Plane, Capsule }
    public enum ForceMode { Impulse, Force }
    public enum LightType { Directional, Point, Spot }
    public enum LightShadows { None, Soft, Hard }
    public enum CameraClearFlags { SolidColor, Skybox }
    public enum LineTextureMode { Stretch, Tile }
    public enum ParticleSystemShapeType { Sphere, Cone, Box }
    public enum ParticleSystemSimulationSpace { Local, World }
    public enum SendMessageOptions { DontRequireReceiver }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; } = Vector3.one;
        public Quaternion rotation { get; set; }
        public Quaternion localRotation { get; set; }
        public Vector3 up => new Vector3(0, 1, 0);
        public void Rotate(Vector3 axis, float angle, Space space) { }
        public void SetParent(Transform p, bool worldPositionStays = true) { }
        public Transform Find(string n) => new GameObject(n).transform;
        public IEnumerator GetEnumerator() { yield break; }
    }

    public class Component : Object
    {
        internal GameObject _go;
        public GameObject gameObject => _go;
        public Transform transform => _go.transform;
        public string tag { get; set; }
        public T GetComponent<T>() where T : class => _go.GetComponent<T>();
        public T GetComponentInParent<T>() where T : class => null;
        public T GetComponentInChildren<T>() where T : class => null;
    }

    public class GameObject : Object
    {
        internal readonly List<object> _comps = new List<object>();
        internal Transform _t;
        public GameObject() { _t = new Transform(); _t._go = this; _comps.Add(_t); }
        public GameObject(string n) : this() { name = n; }
        public new Transform transform => _t;
        public GameObject gameObject => this;
        public string tag { get; set; }
        public bool activeSelf => true;
        public bool activeInHierarchy => true;
        public void SetActive(bool v) { }
        public T AddComponent<T>() where T : class
        {
            var c = Activator.CreateInstance<T>();
            if (c is Component cc) cc._go = this;
            _comps.Add(c);
            return c;
        }
        public T GetComponent<T>() where T : class
        {
            foreach (var c in _comps) if (c is T t) return t;
            return null;
        }
        public T GetComponentInParent<T>() where T : class => GetComponent<T>();
        public static GameObject CreatePrimitive(PrimitiveType t)
        {
            var g = new GameObject(t.ToString());
            g._comps.Add(new Renderer());
            g._comps.Add(new Collider());
            return g;
        }
        public static GameObject Find(string n) => null;
        public new static T FindObjectOfType<T>() where T : class => null;
    }

    public class MonoBehaviour : Component
    {
        public static readonly List<IEnumerator> Pending = new List<IEnumerator>();
        public Coroutine StartCoroutine(IEnumerator e) { Pending.Add(e); return null; }
        public void StopCoroutine(Coroutine c) { }
        public void StopAllCoroutines() { }
        public static void Pump(float dt)
        {
            for (int i = 0; i < 500; i++)
            {
                var cur = new List<IEnumerator>(Pending);
                Pending.Clear();
                foreach (var e in cur) if (!e.MoveNext()) Pending.Remove(e);
                if (Pending.Count == 0) break;
            }
        }
        public void Invoke(string m, float t) { }
        public void CancelInvoke() { }
        public void CancelInvoke(string m) { }
        public void Destroy(Object o) { }
    }
    public class Coroutine { }

    public class Collider : Component { }
    public class MeshRenderer : Renderer { }
    public class AudioListener : Component { }
    public class SphereCollider : Collider { }
    public class Rigidbody : Component
    {
        public bool useGravity { get; set; }
        public float mass { get; set; }
        public Vector3 velocity { get; set; }
        public void AddTorque(Vector3 t, ForceMode m) { }
    }
    public class Renderer : Component
    {
        public Material material { get; set; } = new Material();
        public Material sharedMaterial { get; set; }
    }
    public class Material : Object
    {
        public Material() { }
        public Material(Shader s) { }
        public Color color { get; set; }
        public int renderQueue { get; set; }
        public void SetFloat(string n, float v) { }
        public void SetInt(string n, int v) { }
        public void SetColor(string n, Color c) { }
        public void EnableKeyword(string k) { }
        public void DisableKeyword(string k) { }
        public Color GetColor(string n) => color;
    }
    public class Shader : Object { public static Shader Find(string n) => new Shader(); }
    public class Font : Object { public Material material { get; set; } }
    public static class Resources
    {
        public static T GetBuiltinResource<T>(string path) where T : class => null;
    }
    public class Light : Component
    {
        public bool enabled { get; set; }
        public LightType type { get; set; }
        public float intensity { get; set; }
        public Color color { get; set; }
        public float range { get; set; }
        public LightShadows shadows { get; set; }
    }
    public class Camera : Component
    {
        public static Camera main => null;
        public float fieldOfView { get; set; }
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public Vector3 ScreenToWorldPoint(Vector3 p) => p;
        public Ray ScreenPointToRay(Vector3 p) => new Ray(p, new Vector3(0, 0, 1));
    }
    public class LineRenderer : Component
    {
        public int positionCount { get; set; }
        public float startWidth { get; set; }
        public float endWidth { get; set; }
        public Material material { get; set; }
        public Color startColor { get; set; }
        public Color endColor { get; set; }
        public LineTextureMode textureMode { get; set; }
        public int numCornerVertices { get; set; }
        public int numCapVertices { get; set; }
        public void SetPositions(Vector3[] pts) { }
    }
    public class ParticleSystem : Component
    {
        public struct MinMaxCurve
        {
            public float min, max;
            public MinMaxCurve(float min, float max) { this.min = min; this.max = max; }
            public static implicit operator MinMaxCurve(float v) => new MinMaxCurve(v, v);
        }
        public MainModule main => new MainModule();
        public EmissionModule emission => new EmissionModule();
        public ShapeModule shape => new ShapeModule();
        public ColorOverLifetimeModule colorOverLifetime => new ColorOverLifetimeModule();
        public struct ColorOverLifetimeModule
        {
            public bool enabled { get; set; }
            public Gradient color { get; set; }
        }
        public struct Burst { public Burst(float t, int c) { } }
        public struct MainModule
        {
            public MinMaxCurve startLifetime { get; set; }
            public MinMaxCurve startSpeed { get; set; }
            public MinMaxCurve startSize { get; set; }
            public Color startColor { get; set; }
            public int maxParticles { get; set; }
            public float gravityModifier { get; set; }
            public bool loop { get; set; }
            public ParticleSystemSimulationSpace simulationSpace { get; set; }
        }
        public struct EmissionModule
        {
            public float rateOverTime { get; set; }
            public void SetBursts(Burst[] b) { }
        }
        public struct ShapeModule
        {
            public ParticleSystemShapeType shapeType { get; set; }
            public float radius { get; set; }
            public float angle { get; set; }
            public Vector3 scale { get; set; }
        }
    }
    public class ParticleSystemRenderer : Renderer { }
    public class AudioSource : Component
    {
        public float spatialBlend { get; set; }
        public bool mute { get; set; }
        public bool loop { get; set; }
        public float volume { get; set; }
        public AudioClip clip { get; set; }
        public bool isPlaying => false;
        public void Play() { }
        public void PlayOneShot(AudioClip c) { }
        public void PlayOneShot(AudioClip c, float vol) { }
    }
    public class AudioClip : Object
    {
        public static AudioClip Create(string n, int len, int ch, int sr, bool stream) => new AudioClip();
        public void SetData(float[] d, int off) { }
    }
    namespace Rendering { public enum BlendMode { SrcAlpha, OneMinusSrcAlpha } }
}

namespace UnityEngine
{
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
}
namespace UnityEngine.UI
{
    public class Text : UnityEngine.Component
    {
        public UnityEngine.Font font { get; set; }
        public string text { get; set; }
        public int fontSize { get; set; }
        public TextAnchor alignment { get; set; }
        public UnityEngine.Color color { get; set; }
        public bool supportRichText { get; set; }
        public bool raycastTarget { get; set; }
    }
    public class Graphic : UnityEngine.Component { }
    public class Image : Graphic
    {
        public UnityEngine.Color color { get; set; }
        public UnityEngine.Sprite sprite { get; set; }
        public Type type { get; set; }
        public bool raycastTarget { get; set; }
        public enum Type { Simple, Sliced, Tiled, Filled }
    }
    public class Shadow : Graphic
    {
        public UnityEngine.Color effectColor { get; set; }
        public UnityEngine.Vector2 effectDistance { get; set; }
    }
    public class Button : Graphic
    {
        public Graphic targetGraphic { get; set; }
        public UnityEngine.Events.UnityEvent onClick { get; } = new UnityEngine.Events.UnityEvent();
        public bool interactable { get; set; } = true;
    }
    public class Canvas : UnityEngine.Component { public RenderMode renderMode { get; set; } }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public class CanvasScaler : UnityEngine.Component
    {
        public ScaleMode uiScaleMode { get; set; }
        public UnityEngine.Vector2 referenceResolution { get; set; }
        public enum ScaleMode { ScaleWithScreenSize, ConstantPixelSize }
    }
    public class GraphicRaycaster : UnityEngine.Component { }
    public class RectTransform : UnityEngine.Transform
    {
        public UnityEngine.Vector2 pivot { get; set; }
        public UnityEngine.Vector2 sizeDelta { get; set; }
        public UnityEngine.Vector2 anchoredPosition { get; set; }
        public UnityEngine.Vector2 anchorMin { get; set; }
        public UnityEngine.Vector2 anchorMax { get; set; }
        public UnityEngine.Vector2 offsetMin { get; set; }
        public UnityEngine.Vector2 offsetMax { get; set; }
    }
}

namespace UnityEngine.Events
{
    public class UnityEvent { public void AddListener(UnityEngine.Events.UnityAction a) { } }
    public delegate void UnityAction();
}

namespace UnityEngine.EventSystems
{
    public class EventSystem : UnityEngine.Component
    {
        public static EventSystem current => null;
        public void RaycastAll(PointerEventData data, System.Collections.Generic.List<RaycastResult> results) { }
    }
    public class StandaloneInputModule : UnityEngine.Component { }
    public class PointerEventData
    {
        public PointerEventData(EventSystem es) { }
        public UnityEngine.Vector2 position { get; set; }
    }
    public struct RaycastResult
    {
        public UnityEngine.GameObject gameObject;
    }
}

// ---------------------------------------------------------------------------
// Harness-only additions: PlayerPrefs + tiny collaborator singletons so the
// engine-agnostic scripts (Bubble, BossLevel, ProgressManager, PuzzleLayouts)
// compile without the rendering/input/UI managers.
// ---------------------------------------------------------------------------

namespace UnityEngine
{
    public static class PlayerPrefs
    {
        static readonly Dictionary<string, int> ints = new Dictionary<string, int>();
        static readonly Dictionary<string, string> strs = new Dictionary<string, string>();
        public static int GetInt(string key, int defaultValue = 0)
            => ints.TryGetValue(key, out var v) ? v : defaultValue;
        public static void SetInt(string key, int value) { ints[key] = value; }
        public static string GetString(string key, string defaultValue = "")
            => strs.TryGetValue(key, out var v) ? v : defaultValue;
        public static void SetString(string key, string value) { strs[key] = value; }
        public static void DeleteKey(string key) { ints.Remove(key); strs.Remove(key); }
        public static void DeleteAll() { ints.Clear(); strs.Clear(); }
        public static void Save() { }
    }
}

// ---------------------------------------------------------------------------
// Full-game compile surface: everything the gameplay/UI scripts reference.
// Mirrors real UnityEngine / UnityEngine.UI API shapes (compile-time only).
// ---------------------------------------------------------------------------

namespace UnityEngine
{
    public enum Space { World, Self }
    public enum FogMode { Linear, Exponential, ExponentialSquared }
    public enum TouchPhase { Began, Moved, Stationary, Ended, Canceled }

    public struct Ray
    {
        public Vector3 origin, direction;
        public Ray(Vector3 o, Vector3 d) { origin = o; direction = d; }
        public Vector3 GetPoint(float t) => origin + direction * t;
    }
    public struct Plane
    {
        Vector3 n; float d;
        public Plane(Vector3 normal, Vector3 point) { n = normal; d = 0; }
        public bool Raycast(Ray ray, out float enter) { enter = 10f; return true; }
    }
    public struct Touch
    {
        public int fingerId;
        public Vector2 position;
        public TouchPhase phase;
    }

    public struct Rect
    {
        public float x, y, width, height;
        public Rect(float x, float y, float w, float h) { this.x = x; this.y = y; width = w; height = h; }
    }
    public enum TextureFormat { RGBA32 }
    public enum FilterMode { Bilinear }
    public enum SpriteMeshType { FullRect }
    public class Texture2D : Object
    {
        public Texture2D(int w, int h, TextureFormat f, bool mipmap) { }
        public FilterMode filterMode { get; set; }
        public void SetPixel(int x, int y, Color c) { }
        public void Apply() { }
    }
    public class Sprite : Object
    {
        public static Sprite Create(Texture2D tex, Rect rect, Vector2 pivot, float px, uint extrude, SpriteMeshType mesh, Vector4 border) => new Sprite();
    }

    public struct GradientColorKey { public Color color; public float time; public GradientColorKey(Color c, float t) { color = c; time = t; } }
    public struct GradientAlphaKey { public float alpha; public float time; public GradientAlphaKey(float a, float t) { alpha = a; time = t; } }
    public class Gradient : Object
    {
        public void SetKeys(GradientColorKey[] ck, GradientAlphaKey[] ak) { }
    }

    public class TextMesh : Component
    {
        public Font font { get; set; }
        public string text { get; set; }
        public int fontSize { get; set; }
        public float characterSize { get; set; }
        public TextAnchor anchor { get; set; }
        public TextAlignment alignment { get; set; }
        public Color color { get; set; }
    }
    public enum TextAlignment { Left, Center, Right }

    public class TrailRenderer : Component
    {
        public float time { get; set; }
        public float startWidth { get; set; }
        public float endWidth { get; set; }
        public Material material { get; set; }
        public Color startColor { get; set; }
        public Color endColor { get; set; }
    }

    public class WaitForSecondsRealtime { public WaitForSecondsRealtime(float t) { } }


}

namespace UnityEngine.EventSystems
{
    // Pointer input handler interfaces used by UIButtonFeedback.
    public interface IPointerEnterHandler { void OnPointerEnter(PointerEventData e); }
    public interface IPointerExitHandler { void OnPointerExit(PointerEventData e); }
    public interface IPointerDownHandler { void OnPointerDown(PointerEventData e); }
    public interface IPointerUpHandler { void OnPointerUp(PointerEventData e); }
}
