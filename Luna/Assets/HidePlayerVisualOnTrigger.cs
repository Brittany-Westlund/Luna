using UnityEngine;

public class HidePlayerVisualsAndAudioOnTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Visuals off
        var sr = other.GetComponentInChildren<SpriteRenderer>(true);
        var anim = other.GetComponentInChildren<Animator>(true);

        if (sr != null) sr.enabled = false;
        if (anim != null) anim.enabled = false;

        // Audio pause (not disable)
        var audioSources = other.GetComponentsInChildren<AudioSource>(true);
        foreach (var a in audioSources)
        {
            // Pause is safest for looping footsteps/ambience
            a.Pause();
            a.mute = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Visuals on
        var sr = other.GetComponentInChildren<SpriteRenderer>(true);
        var anim = other.GetComponentInChildren<Animator>(true);

        if (sr != null) sr.enabled = true;
        if (anim != null) anim.enabled = true;

        // Audio unpause immediately
        var audioSources = other.GetComponentsInChildren<AudioSource>(true);
        foreach (var a in audioSources)
        {
            a.mute = false;
            a.UnPause();
        }
    }
}
