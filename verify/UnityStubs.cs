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
        public float sqrMagnitude => x * x + y * y + z * z;
        public Vector3 normalized => this;
        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
        public float magnitude => (float)Math.Sqrt(sqrMagnitude);
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * Mathf.Clamp01(t);
    }
    public struct Vector4 { public float x, y, z, w; }
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
        public static int Clamp(int v, int a, int b) => v < a ? a : v > b ? b : v;
        public static int RoundToInt(float v) => (int)Math.Round(v);
        public static int FloorToInt(float v) => (int)Math.Floor(v);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Sin(float v) => (float)Math.Sin(v);
        public static float Sign(float v) => Math.Sign(v);
        public static float Exp(float v) => (float)Math.Exp(v);
        public static float PingPong(float t, float len) => t;
    }
    public static class Random
    {
        public static float value => 0.5f;
        public static int Range(int a, int b) => a;
        public static float Range(float a, float b) => a;
        public static Vector3 insideUnitSphere => Vector3.one;
    }
    public static class Time { public static float deltaTime => 0.016f; }
    public class WaitForSeconds { public WaitForSeconds(float t) { } }
    public static class Application { public static int targetFrameRate { get; set; } }
    public static class RenderSettings { public static Color ambientLight { get; set; } }
    public static class Input
    {
        public static Vector3 mousePosition => Vector3.zero;
        public static bool GetMouseButtonDown(int b) => false;
        public static bool GetKeyDown(KeyCode k) => false;
    }
    public enum KeyCode { X, B }
    public enum PrimitiveType { Sphere, Cube, Cylinder, Plane, Capsule }
    public enum ForceMode { Impulse, Force }
    public enum LightType { Directional, Point, Spot }
    public enum LightShadows { None, Soft, Hard }
    public enum CameraClearFlags { SolidColor, Skybox }
    public enum LineTextureMode { Stretch, Tile }
    public enum ParticleSystemShapeType { Sphere, Cone }
    public enum ParticleSystemSimulationSpace { Local, World }
    public enum SendMessageOptions { DontRequireReceiver }

    public class Transform : Component, IEnumerable
    {
        public Vector3 position { get; set; }
        public Vector3 localPosition { get; set; }
        public Vector3 localScale { get; set; } = Vector3.one;
        public Quaternion rotation { get; set; }
        public void SetParent(Transform p, bool worldPositionStays) { }
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
        public void Destroy(Object o) { }
    }
    public class Coroutine { }

    public class Collider : Component { }
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
    }
    public class Shader : Object { public static Shader Find(string n) => new Shader(); }
    public class Font : Object { }
    public static class Resources
    {
        public static T GetBuiltinResource<T>(string path) where T : class => null;
    }
    public class Light : Component
    {
        public LightType type { get; set; }
        public float intensity { get; set; }
        public Color color { get; set; }
        public LightShadows shadows { get; set; }
    }
    public class Camera : Component
    {
        public float fieldOfView { get; set; }
        public CameraClearFlags clearFlags { get; set; }
        public Color backgroundColor { get; set; }
        public Vector3 ScreenToWorldPoint(Vector3 p) => p;
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
        public MainModule main => new MainModule();
        public EmissionModule emission => new EmissionModule();
        public ShapeModule shape => new ShapeModule();
        public struct Burst { public Burst(float t, int c) { } }
        public struct MainModule
        {
            public float startLifetime { get; set; }
            public float startSpeed { get; set; }
            public float startSize { get; set; }
            public Color startColor { get; set; }
            public int maxParticles { get; set; }
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
        }
    }
    public class ParticleSystemRenderer : Renderer { }
    public class AudioSource : Component
    {
        public float spatialBlend { get; set; }
        public void PlayOneShot(AudioClip c) { }
    }
    public class AudioClip : Object
    {
        public static AudioClip Create(string n, int len, int ch, int sr, bool stream) => new AudioClip();
        public void SetData(float[] d, int off) { }
    }
    namespace Rendering { public enum BlendMode { SrcAlpha, OneMinusSrcAlpha } }
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
    }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public class Graphic : UnityEngine.Component { }
    public class Image : Graphic { public UnityEngine.Color color { get; set; } }
    public class Shadow : Graphic { public UnityEngine.Color effectColor { get; set; } }
    public class Button : Graphic
    {
        public Graphic targetGraphic { get; set; }
        public UnityEngine.Events.UnityEvent onClick { get; } = new UnityEngine.Events.UnityEvent();
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
    public class EventSystem : UnityEngine.Component { }
    public class StandaloneInputModule : UnityEngine.Component { }
}
