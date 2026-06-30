using UnityEngine;
using Game2048.Persistence;

namespace Game2048.Services
{
    public sealed class AudioService : MonoBehaviour
    {
        private const int SampleRate = 44100;

        private AudioSource source;
        private SettingsService settings;

        private AudioClip moveClip;
        private AudioClip mergeClip;
        private AudioClip winClip;
        private AudioClip loseClip;

        public void Initialize(SettingsService settingsService)
        {
            settings = settingsService;
            source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;

            moveClip = CreateTone(220f, 0.06f, 0.25f);
            mergeClip = CreateTone(440f, 0.09f, 0.35f);
            winClip = CreateArpeggio(new[] { 523f, 659f, 784f, 1046f }, 0.09f, 0.35f);
            loseClip = CreateArpeggio(new[] { 392f, 330f, 262f }, 0.12f, 0.3f);
        }

        public void PlayMove() => Play(moveClip);

        public void PlayMerge(int value)
        {
            if (!CanPlay())
            {
                return;
            }
            float pitch = Mathf.Clamp(1f + Mathf.Log(value, 2) * 0.05f, 1f, 2.2f);
            source.pitch = pitch;
            source.PlayOneShot(mergeClip);
            source.pitch = 1f;
        }

        public void PlayWin() => Play(winClip);

        public void PlayLose() => Play(loseClip);

        private void Play(AudioClip clip)
        {
            if (CanPlay() && clip != null)
            {
                source.PlayOneShot(clip);
            }
        }

        private bool CanPlay()
        {
            return source != null && settings != null && settings.SoundEnabled;
        }

        private static AudioClip CreateTone(float frequency, float duration, float volume)
        {
            int samples = Mathf.CeilToInt(SampleRate * duration);
            var data = new float[samples];
            for (int i = 0; i < samples; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Exp(-8f * t / duration);
                data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * volume;
            }
            var clip = AudioClip.Create("tone", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip CreateArpeggio(float[] frequencies, float noteDuration, float volume)
        {
            int perNote = Mathf.CeilToInt(SampleRate * noteDuration);
            int samples = perNote * frequencies.Length;
            var data = new float[samples];
            for (int n = 0; n < frequencies.Length; n++)
            {
                for (int i = 0; i < perNote; i++)
                {
                    float t = (float)i / SampleRate;
                    float envelope = Mathf.Exp(-6f * t / noteDuration);
                    data[n * perNote + i] = Mathf.Sin(2f * Mathf.PI * frequencies[n] * t) * envelope * volume;
                }
            }
            var clip = AudioClip.Create("arp", samples, 1, SampleRate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
