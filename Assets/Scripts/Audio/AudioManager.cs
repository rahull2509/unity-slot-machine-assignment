using System;
using UnityEngine;

namespace SlotMachineGame.Audio
{
    /// <summary>
    /// Manages slot machine audio effects (spin loop, reel stop tick, win fanfare, button clicks).
    /// Features procedural audio synthesis fallback if audio clip references are empty, ensuring
    /// complete audio feedback out-of-the-box without missing asset warnings.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource loopSource;

        [Header("Audio Clips")]
        [SerializeField] private AudioClip spinLoopClip;
        [SerializeField] private AudioClip reelStopClip;
        [SerializeField] private AudioClip winClip;
        [SerializeField] private AudioClip clickClip;

        [Header("Settings")]
        [SerializeField] private bool isMuted = false;
        [SerializeField] private float sfxVolume = 0.8f;
        [SerializeField] private float musicVolume = 0.5f;

        public bool IsMuted => isMuted;

        public event Action<bool> OnMuteStateChanged;

        private void Awake()
        {
            EnsureAudioSources();
            GenerateFallbackClipsIfNeeded();
        }

        private void EnsureAudioSources()
        {
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.playOnAwake = false;
                sfxSource.volume = sfxVolume;
            }

            if (loopSource == null)
            {
                loopSource = gameObject.AddComponent<AudioSource>();
                loopSource.playOnAwake = false;
                loopSource.loop = true;
                loopSource.volume = musicVolume;
            }
        }

        private void GenerateFallbackClipsIfNeeded()
        {
            if (reelStopClip == null)
                reelStopClip = CreateToneClip("ReelStopTick", 440f, 0.08f, 0.2f);

            if (clickClip == null)
                clickClip = CreateToneClip("ButtonClick", 660f, 0.05f, 0.15f);

            if (spinLoopClip == null)
                spinLoopClip = CreateSpinLoopClip("SpinLoop", 0.4f);

            if (winClip == null)
                winClip = CreateWinFanfareClip("WinFanfare", 1.0f);
        }

        public void PlaySpinLoop()
        {
            if (isMuted || loopSource == null || spinLoopClip == null) return;
            loopSource.clip = spinLoopClip;
            loopSource.Play();
        }

        public void StopSpinLoop()
        {
            if (loopSource != null && loopSource.isPlaying)
            {
                loopSource.Stop();
            }
        }

        public void PlayReelStop()
        {
            PlaySfx(reelStopClip, 1.0f);
        }

        public void PlayWin()
        {
            StopSpinLoop();
            PlaySfx(winClip, 1.2f);
        }

        public void PlayClick()
        {
            PlaySfx(clickClip, 0.8f);
        }

        public void ToggleMute()
        {
            SetMuted(!isMuted);
        }

        public void SetMuted(bool mute)
        {
            isMuted = mute;
            if (sfxSource != null) sfxSource.mute = isMuted;
            if (loopSource != null)
            {
                loopSource.mute = isMuted;
                if (isMuted) loopSource.Stop();
            }
            OnMuteStateChanged?.Invoke(isMuted);
        }

        private void PlaySfx(AudioClip clip, float pitch = 1.0f)
        {
            if (isMuted || clip == null || sfxSource == null) return;
            sfxSource.pitch = pitch;
            sfxSource.PlayOneShot(clip, sfxVolume);
        }

        #region Procedural Audio Synthesis Fallbacks

        private static AudioClip CreateToneClip(string name, float frequency, float duration, float decay)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float envelope = Mathf.Exp(-t / decay);
                samples[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.5f;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateSpinLoopClip(string name, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                float rumble = Mathf.Sin(2f * Mathf.PI * 90f * t) * 0.25f;
                float click = Mathf.Sin(2f * Mathf.PI * 300f * t) * 0.15f;
                samples[i] = (rumble + click) * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        private static AudioClip CreateWinFanfareClip(string name, float duration)
        {
            int sampleRate = 44100;
            int sampleCount = (int)(sampleRate * duration);
            float[] samples = new float[sampleCount];

            float[] notes = new float[] { 523.25f, 659.25f, 783.99f, 1046.50f }; // C5, E5, G5, C6
            float noteDuration = duration / notes.Length;

            for (int i = 0; i < sampleCount; i++)
            {
                float t = (float)i / sampleRate;
                int noteIndex = Mathf.Clamp((int)(t / noteDuration), 0, notes.Length - 1);
                float noteT = t - (noteIndex * noteDuration);
                float envelope = Mathf.Exp(-noteT / 0.18f);
                samples[i] = Mathf.Sin(2f * Mathf.PI * notes[noteIndex] * t) * envelope * 0.4f;
            }

            AudioClip clip = AudioClip.Create(name, sampleCount, 1, sampleRate, false);
            clip.SetData(samples, 0);
            return clip;
        }

        #endregion
    }
}
