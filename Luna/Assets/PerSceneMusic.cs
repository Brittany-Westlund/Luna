using UnityEngine;
using System;
using System.Collections.Generic;

public class PerSceneMusic : MonoBehaviour
{
    [Serializable]
    public struct ChannelClip
    {
        public string channelName;            // e.g. "music", "ambience", "rain"
        public AudioClip clip;
        [Range(0f,1f)] public float volume;   // 0 => will default to 1
        public float fadeSeconds;             // -1 => use MusicPlayer.defaultFadeSeconds
        public bool playOnStart;              // auto play in Start()
        public bool loop;                     // loop this clip
    }

    [Header("Clips to start in this scene")]
    public ChannelClip[] clips;

    [Header("On Disable")]
    [Tooltip("If true, will fade out all channels started by this component when it disables (scene unload, etc).")]
    public bool fadeOutOnDisable = true;
    [Tooltip("Fade time used on disable. -1 => use MusicPlayer.defaultFadeSeconds.")]
    public float fadeOutSeconds = -1f;

    // track which channels we started, so we can cleanly fade them out on disable if desired
    private readonly List<string> _startedChannels = new List<string>();

    private void Start()
    {
        // Convenience bootstrap if you forgot to place MusicPlayer in your bootstrap scene
        if (MusicPlayer.Instance == null)
        {
            var go = new GameObject("~MusicPlayer");
            go.AddComponent<MusicPlayer>();
        }

        _startedChannels.Clear();

        if (clips == null) return;

        foreach (var cc in clips)
        {
            if (!cc.playOnStart || cc.clip == null) continue;

            var vol = (cc.volume <= 0f) ? 1f : cc.volume;
            MusicPlayer.Instance.Play(cc.channelName, cc.clip, vol, cc.fadeSeconds, cc.loop);

            if (!string.IsNullOrEmpty(cc.channelName) && !_startedChannels.Contains(cc.channelName))
                _startedChannels.Add(cc.channelName);
        }
    }

    private void OnDisable()
    {
        if (!fadeOutOnDisable || MusicPlayer.Instance == null) return;

        foreach (var ch in _startedChannels)
        {
            MusicPlayer.Instance.FadeOut(ch, fadeOutSeconds);
        }

        _startedChannels.Clear();
    }

    // -------- Optional helpers for UI buttons / code --------
    public void PlayOnChannel(string channel, AudioClip clip, float volume = 1f, float fade = -1f, bool loop = true)
    {
        if (MusicPlayer.Instance == null || clip == null) return;
        MusicPlayer.Instance.Play(channel, clip, Mathf.Clamp01(volume), fade, loop);

        if (!string.IsNullOrEmpty(channel) && !_startedChannels.Contains(channel))
            _startedChannels.Add(channel);
    }

    public void FadeOutChannel(string channel, float fade = -1f)
    {
        MusicPlayer.Instance?.FadeOut(channel, fade);
        _startedChannels.Remove(channel);
    }

    public void StopChannel(string channel)
    {
        MusicPlayer.Instance?.Stop(channel);
        _startedChannels.Remove(channel);
    }

    public void SetLoop(string channel, bool loop)
        => MusicPlayer.Instance?.SetLoop(channel, loop);

    public void SetVolume(string channel, float volume01)
        => MusicPlayer.Instance?.SetVolume(channel, Mathf.Clamp01(volume01));
}
