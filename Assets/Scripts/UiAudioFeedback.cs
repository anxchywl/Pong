using System.Collections.Generic;
using UnityEngine;

namespace Pong
{
    public sealed class UiAudioFeedback
    {
        private readonly AudioSource source;
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private AudioClip clickClip;

        public UiAudioFeedback(AudioSource source)
        {
            this.source = source;
        }

        public float Volume
        {
            set => source.volume = Mathf.Clamp01(value) * 0.16f;
        }

        public void ApplyTheme(GameTheme theme)
        {
            if (clips.TryGetValue(theme.Id, out clickClip))
            {
                return;
            }

            clickClip = CreateClickClip(theme);
            clips.Add(theme.Id, clickClip);
        }

        public void PlayClick()
        {
            if (clickClip != null)
            {
                source.PlayOneShot(clickClip);
            }
        }

        private static AudioClip CreateClickClip(GameTheme theme)
        {
            const int sampleRate = 44100;
            int sampleCount = Mathf.CeilToInt(sampleRate * theme.ClickDuration);
            float[] samples = new float[sampleCount];

            for (int index = 0; index < sampleCount; index++)
            {
                float time = (float)index / sampleRate;
                float progress = (float)index / sampleCount;
                float envelope = 1f - progress;
                float phase = 2f * Mathf.PI * theme.ClickFrequency * time;
                float wave = theme.AudioWaveform == ThemeAudioWaveform.Square
                    ? Mathf.Sign(Mathf.Sin(phase))
                    : Mathf.Sin(phase) + Mathf.Sin(phase * 1.5f) * 0.18f;
                samples[index] = wave * envelope * envelope;
            }

            AudioClip clip = AudioClip.Create($"{theme.DisplayName} UI Confirm", sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }
    }
}
