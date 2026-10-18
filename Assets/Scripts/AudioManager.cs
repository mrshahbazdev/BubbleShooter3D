using System.Collections.Generic;
using UnityEngine;

namespace BubbleShooter
{
    /// <summary>Procedurally synthesized SFX and ambient BGM — zero audio assets required.</summary>
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager I { get; private set; }

        readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        readonly List<AudioClip> popScale = new List<AudioClip>();
        readonly List<AudioClip> dropBounces = new List<AudioClip>();
        AudioSource sfxSrc;
        AudioSource bgmSrc;

        public bool IsMuted { get; private set; }

        void Awake()
        {
            I = this;
            sfxSrc = gameObject.AddComponent<AudioSource>();
            sfxSrc.spatialBlend = 0f;

            bgmSrc = gameObject.AddComponent<AudioSource>();
            bgmSrc.spatialBlend = 0f;
            bgmSrc.loop = true;
            bgmSrc.volume = 0.16f;

            IsMuted = PlayerPrefs.GetInt("BS3D_Muted", 0) == 1;
            UpdateMute();

            // Pentatonic scale pops for musical cascade (C5, D5, E5, G5, A5, C6, D6, E6)
            float[] popFreqs = { 523.25f, 587.33f, 659.25f, 783.99f, 880.00f, 1046.50f, 1174.66f, 1318.51f };
            for (int i = 0; i < popFreqs.Length; i++)
            {
                popScale.Add(Tone(popFreqs[i], 0.11f, "sine", 0.65f, decay: 11f));
            }

            // Bouncy cartoon plop tones for floor bounces
            float[] bounceFreqs = { 440f, 392f, 349f, 329f, 293f, 261f, 220f, 196f };
            for (int i = 0; i < bounceFreqs.Length; i++)
            {
                dropBounces.Add(SweepTone(bounceFreqs[i], bounceFreqs[i] * 0.45f, 0.085f, "sine", 0.65f));
            }

            clips["pop"] = popScale[4]; // Default pop
            clips["shoot"] = SweepTone(340f, 120f, 0.10f, "sine", 0.65f);
            clips["bomb"] = Tone(95f, 0.42f, "noise", 0.85f, decay: 2.5f);
            clips["zap"] = SweepTone(980f, 160f, 0.24f, "saw", 0.55f);
            clips["fire"] = Tone(130f, 0.32f, "noise", 0.7f, decay: 3.2f);
            clips["drop"] = Tone(330f, 0.12f, "sine", 0.4f, decay: 4f);
            clips["avalanche"] = Tone(68f, 0.55f, "sine", 0.9f, decay: 2.2f);
            clips["descend"] = Tone(196f, 0.18f, "square", 0.3f, decay: 5f);
            clips["combo"] = Arpeggio(new[] { 587f, 740f, 880f, 1174f }, 0.08f);
            clips["win"] = Arpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.12f);
            clips["click"] = Tone(660f, 0.05f, "sine", 0.3f, decay: 10f);
            clips["alarm"] = SweepTone(750f, 520f, 0.22f, "saw", 0.45f);
            clips["tick"] = Tone(180f, 0.04f, "triangle", 0.5f, decay: 15f);
            clips["podHit"] = Arpeggio(new[] { 523f, 659f, 784f }, 0.05f);
            clips["swap"] = SweepTone(260f, 520f, 0.08f, "sine", 0.4f);

            // Generate relaxing procedural BGM
            bgmSrc.clip = GenerateAmbientBGM();
            if (!IsMuted) bgmSrc.Play();
        }

        public void ToggleMute()
        {
            IsMuted = !IsMuted;
            PlayerPrefs.SetInt("BS3D_Muted", IsMuted ? 1 : 0);
            PlayerPrefs.Save();
            UpdateMute();
        }

        void UpdateMute()
        {
            if (sfxSrc != null) sfxSrc.mute = IsMuted;
            if (bgmSrc != null)
            {
                bgmSrc.mute = IsMuted;
                if (!IsMuted && !bgmSrc.isPlaying) bgmSrc.Play();
            }
        }

        public void Play(string name)
        {
            if (IsMuted) return;
            if (clips.TryGetValue(name, out var c) && sfxSrc != null) sfxSrc.PlayOneShot(c);
        }

        /// <summary>Plays a pop tone with pitch scaling dynamically based on index in cascade.</summary>
        public void PlayPop(int pitchIndex = 0)
        {
            if (IsMuted || sfxSrc == null || popScale.Count == 0) return;
            int idx = Mathf.Clamp(pitchIndex, 0, popScale.Count - 1);
            sfxSrc.PlayOneShot(popScale[idx]);
        }

        /// <summary>Plays a bouncy cartoon pop when a falling bubble bounces on the floor.</summary>
        public void PlayDropBounce(int index = 0)
        {
            if (IsMuted || sfxSrc == null || dropBounces.Count == 0) return;
            int idx = Mathf.Clamp(index, 0, dropBounces.Count - 1);
            sfxSrc.PlayOneShot(dropBounces[idx], 0.75f);
        }

        /// <summary>Cascading musical chimes when bulk bubbles drop together.</summary>
        public void PlayDropCascade(int count)
        {
            if (IsMuted || sfxSrc == null) return;
            StartCoroutine(CoDropCascade(count));
        }

        System.Collections.IEnumerator CoDropCascade(int count)
        {
            int notes = Mathf.Clamp(count, 2, 8);
            float[] freqs = { 261.63f, 329.63f, 392.00f, 523.25f, 659.25f, 783.99f, 1046.50f, 1318.51f };
            for (int i = 0; i < notes; i++)
            {
                if (sfxSrc != null && !IsMuted)
                {
                    sfxSrc.PlayOneShot(Tone(freqs[i], 0.12f, "triangle", 0.5f, decay: 7f));
                }
                yield return new WaitForSeconds(0.04f);
            }
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

        static AudioClip SweepTone(float startFreq, float endFreq, float dur, string wave, float vol)
        {
            int sr = 44100, n = (int)(sr * dur);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                float f = Mathf.Lerp(startFreq, endFreq, t / dur);
                float env = 1f - (t / dur);
                float v = wave == "saw" ? (2f * ((f * t) % 1f) - 1f) : Mathf.Sin(2 * Mathf.PI * f * t);
                data[i] = v * env * vol;
            }
            var clip = AudioClip.Create("swp", n, 1, sr, false);
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

        static AudioClip GenerateAmbientBGM()
        {
            // 8-second ambient loop with pentatonic progression (Cmaj9 arpeggios)
            int sr = 22050;
            float dur = 8f;
            int n = (int)(sr * dur);
            var data = new float[n];
            float[] chord = { 261.63f, 329.63f, 392.00f, 493.88f, 587.33f }; // C, E, G, B, D
            float noteDur = 0.5f;

            for (int i = 0; i < n; i++)
            {
                float t = (float)i / sr;
                int noteIdx = (int)(t / noteDur) % chord.Length;
                float f = chord[noteIdx];
                float noteT = (t % noteDur);
                float env = Mathf.Sin(noteT / noteDur * Mathf.PI); // smooth bell curve envelope
                float sample = Mathf.Sin(2 * Mathf.PI * f * t) * 0.12f * env;
                // Add soft sub-bass pad
                sample += Mathf.Sin(2 * Mathf.PI * 65.41f * t) * 0.05f;
                data[i] = sample;
            }
            var clip = AudioClip.Create("bgm_ambient", n, 1, sr, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
