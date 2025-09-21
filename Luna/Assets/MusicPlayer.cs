using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicPlayer : MonoBehaviour
{
    public static MusicPlayer Instance { get; private set; }

    [Header("Defaults")]
    [Tooltip("Used when fadeSeconds argument is < 0")]
    public float defaultFadeSeconds = 0.8f;

    // one AudioSource per named channel
    private readonly Dictionary<string, AudioSource> _channels = new Dictionary<string, AudioSource>();
    private readonly Dictionary<string, Coroutine> _runningFades = new Dictionary<string, Coroutine>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Ensure a channel exists and return its AudioSource.
    /// </summary>
    private AudioSource EnsureChannel(string channel)
    {
        if (string.IsNullOrEmpty(channel)) channel = "music";

        if (_channels.TryGetValue(channel, out var src) && src != null)
            return src;

        var go = new GameObject($"~AudioChannel_{channel}");
        go.transform.SetParent(transform, false);
        var newSrc = go.AddComponent<AudioSource>();
        newSrc.playOnAwake = false;
        newSrc.loop = true;
        newSrc.spatialBlend = 0f; // 2D
        newSrc.dopplerLevel = 0f;
        _channels[channel] = newSrc;
        return newSrc;
    }

    /// <summary>
    /// Play clip on channel, optionally crossfading from whatever was there.
    /// </summary>
    public void Play(string channel, AudioClip clip, float volume = 1f, float fadeSeconds = -1f, bool loop = true)
    {
        if (clip == null) return;

        var src = EnsureChannel(channel);
        volume = Mathf.Clamp01(volume);
        if (fadeSeconds < 0f) fadeSeconds = defaultFadeSeconds;

        // If same clip already playing, just set loop/volume and return
        if (src.isPlaying && src.clip == clip)
        {
            src.loop = loop;
            src.volume = volume;
            return;
        }

        // If a fade is already running on this channel, cancel it
        if (_runningFades.TryGetValue(channel, out var co) && co != null)
        {
            StopCoroutine(co);
            _runningFades[channel] = null;
        }

        // Start crossfade (or instant swap if fadeSeconds == 0)
        _runningFades[channel] = StartCoroutine(CrossfadeTo(src, clip, volume, loop, fadeSeconds));
    }

    /// <summary>
    /// Immediately stop a channel (no fade).
    /// </summary>
    public void Stop(string channel)
    {
        var src = EnsureChannel(channel);
        if (_runningFades.TryGetValue(channel, out var co) && co != null)
        {
            StopCoroutine(co);
            _runningFades[channel] = null;
        }

        src.Stop();
        src.clip = null;
    }

    /// <summary>
    /// Fade out the channel to silence, then stop.
    /// </summary>
    public void FadeOut(string channel, float fadeSeconds = -1f)
    {
        var src = EnsureChannel(channel);
        if (!src.isPlaying) return;

        if (fadeSeconds < 0f) fadeSeconds = defaultFadeSeconds;

        if (_runningFades.TryGetValue(channel, out var co) && co != null)
        {
            StopCoroutine(co);
        }
        _runningFades[channel] = StartCoroutine(FadeOutAndStop(src, fadeSeconds, channel));
    }

    /// <summary>
    /// Change loop flag at runtime.
    /// </summary>
    public void SetLoop(string channel, bool loop)
    {
        var src = EnsureChannel(channel);
        src.loop = loop;
    }

    /// <summary>
    /// Change volume immediately (0..1).
    /// </summary>
    public void SetVolume(string channel, float volume)
    {
        var src = EnsureChannel(channel);
        src.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// Optionally expose the AudioSource if you need fine control elsewhere.
    /// </summary>
    public AudioSource GetSource(string channel) => EnsureChannel(channel);

    private IEnumerator CrossfadeTo(AudioSource src, AudioClip newClip, float targetVolume, bool loop, float fadeSeconds)
    {
        // If nothing playing or fadeSeconds == 0 => instant swap
        if (!src.isPlaying || src.clip == null || fadeSeconds <= 0f)
        {
            src.clip = newClip;
            src.loop = loop;
            src.volume = targetVolume;
            src.Play();
            yield break;
        }

        // Fade out old, swap, fade in new
        float half = fadeSeconds * 0.5f;
        float startVol = src.volume;

        // Fade out
        float t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / half);
            yield return null;
        }

        // Swap
        src.Stop();
        src.clip = newClip;
        src.loop = loop;
        src.Play();

        // Fade in
        t = 0f;
        while (t < half)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(0f, targetVolume, t / half);
            yield return null;
        }
        src.volume = targetVolume;
    }

    private IEnumerator FadeOutAndStop(AudioSource src, float fadeSeconds, string channelKey)
    {
        float startVol = src.volume;
        float t = 0f;
        while (t < fadeSeconds)
        {
            t += Time.unscaledDeltaTime;
            src.volume = Mathf.Lerp(startVol, 0f, t / fadeSeconds);
            yield return null;
        }
        src.Stop();
        src.clip = null;
        _runningFades[channelKey] = null;
    }
}
