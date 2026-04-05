using UnityEngine;

public class MusicFadeTrigger : MonoBehaviour
{
    public enum FadeMode
    {
        FadeIn,
        FadeOut
    }

    [Header("Mode")]
    public FadeMode fadeMode = FadeMode.FadeOut;

    [Header("Music")]
    public PlayOnStart musicController;
    public float fadeDuration = 2f;

    [Header("Player Detection")]
    public string playerTag = "Player";
    public bool acceptTaggedParent = true;
    public bool triggerOnce = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private bool hasTriggered = false;

    private void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
        {
            col.isTrigger = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggerOnce && hasTriggered)
        {
            return;
        }

        if (!IsPlayerCollider(other))
        {
            if (debugLogs)
            {
                Debug.Log("MusicFadeTrigger ignored collider: " + other.name);
            }
            return;
        }

        hasTriggered = true;

        if (musicController == null)
        {
            Debug.LogWarning("MusicFadeTrigger: No PlayOnStart assigned on " + gameObject.name);
            return;
        }

        if (debugLogs)
        {
            Debug.Log("MusicFadeTrigger fired on " + gameObject.name + " via collider " + other.name);
        }

        if (fadeMode == FadeMode.FadeIn)
        {
            musicController.FadeIn(fadeDuration);
        }
        else
        {
            musicController.FadeOut(fadeDuration);
        }
    }

    private bool IsPlayerCollider(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            return true;
        }

        if (!acceptTaggedParent)
        {
            return false;
        }

        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag(playerTag))
            {
                return true;
            }

            current = current.parent;
        }

        return false;
    }
}