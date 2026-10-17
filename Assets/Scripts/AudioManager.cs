using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>Procedurally synthesized SFX — zero audio assets required.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        AudioSource src;

        void Awake()
        {
            I = this;
            src = gameObject.AddComponent<AudioSource>();
            src.spatialBlend = 0f;

            clips["pop"] = Tone(880f, 0.09f, "sine", 0.5f, decay: 8f);
            clips["shoot"] = Tone(440f, 0.08f, "triangle", 0.35f, decay: 6f);
            clips["bomb"] = Tone(110f, 0.35f, "noise", 0.7f, decay: 3f);
            clips["drop"] = Tone(330f, 0.12f, "sine", 0.4f, decay: 4f);
            clips["descend"] = Tone(196f, 0.15f, "square", 0.25f, decay: 5f);
            clips["win"] = Arpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.12f);
            clips["lose"] = Arpeggio(new[] { 392f, 330f, 262f, 196f }, 0.16f);
            clips["click"] = Tone(660f, 0.05f, "sine", 0.3f, decay: 10f);
        }

        public void Play(string name)
        {
            if (clips.TryGetValue(name, out var c)) src.PlayOneShot(c);
        }

        static AudioClip Tone(float freq, float dur, string wave, float vol, float decay)
        {
            int sr = 44100, n = (int)(sr * dur);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float env = Mathf.Exp(-decay * t);
                float v = wave switch
                {
                    "sine" => Mathf.Sin(2 * Mathf.PI * freq * t),
                    "square" => Mathf.Sign(Mathf.Sin(2 * Mathf.PI * freq * t)) * 0.6f,
                    "triangle" => Mathf.PingPong(4 * freq * t, 2f) - 1f,
                    _ => Random.Range(-1f, 1f), // noise
                };
                data[i] = v * env * vol;
            }
            var clip = AudioClip.Create("t" + freq, n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }

        static AudioClip Arpeggio(float[] freqs, float noteDur)
        {
            int sr = 44100, n = (int)(sr * noteDur * freqs.Length);
            var data = new float[n];
            int per = (int)(sr * noteDur);
            for (int i = 0; i < n; i++)
            {
                float f = freqs[Mathf.Min(i / per, freqs.Length - 1)];
                float t = (float)(i % per) / sr;
                data[i] = Mathf.Sin(2 * Mathf.PI * f * t) * Mathf.Exp(-3f * t) * 0.4f;
            }
            var clip = AudioClip.Create("arp", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
