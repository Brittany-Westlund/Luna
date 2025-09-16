using UnityEngine;
using System.Collections;

public class GardenGrowth : MonoBehaviour
{
    [Header("References")]
    public GameObject grassObject;         // Grass GameObject (can start disabled)
    public SpriteRenderer grassRenderer;   // SpriteRenderer on Grass

    [Header("Settings")]
    public float restDuration = 3f;        // Time Luna must rest
    public float fadeDuration = 1f;        // Fade-in duration

    [Header("Audio")]
    public AudioClip growthSFX;            // Growth sound
    [Range(0f, 1f)] public float growthVolume = 1f;
    public float soundDelay = 0f;          // Delay after restDuration before sound plays

    private Coroutine restCoroutine;
    private AudioSource audioSource;

    void Awake()
    {
        // Make sure grass is inactive at start if not already
        if (grassObject != null)
            grassObject.SetActive(false);

        if (grassRenderer == null && grassObject != null)
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();

        // Ensure AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void OnTriggerStay2D(Collider2D other)
    {
        var rest = other.GetComponent<LunaRest>();
        if (rest != null && rest.isResting)
        {
            if (restCoroutine == null)
                restCoroutine = StartCoroutine(WaitAndGrow(rest));
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (restCoroutine != null)
        {
            StopCoroutine(restCoroutine);
            restCoroutine = null;
        }
    }

    private IEnumerator WaitAndGrow(LunaRest rest)
    {
        float elapsed = 0f;

        while (elapsed < restDuration)
        {
            if (!rest.isResting) yield break; // bail if Luna stops resting
            elapsed += Time.deltaTime;
            yield return null;
        }

        // ✅ Growth triggered
        if (grassObject != null)
        {
            grassObject.SetActive(true);

            if (grassRenderer != null)
            {
                // start invisible
                Color c = grassRenderer.color;
                c.a = 0f;
                grassRenderer.color = c;

                // play sound after delay
                if (growthSFX != null)
                    StartCoroutine(PlayGrowthSound());

                // fade in
                yield return StartCoroutine(FadeInGrass());
            }
        }

        restCoroutine = null;
    }

    private IEnumerator PlayGrowthSound()
    {
        if (soundDelay > 0f)
            yield return new WaitForSeconds(soundDelay);

        audioSource.PlayOneShot(growthSFX, growthVolume);
    }

    private IEnumerator FadeInGrass()
    {
        float elapsed = 0f;
        Color c = grassRenderer.color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            c.a = t;
            grassRenderer.color = c;
            yield return null;
        }

        // lock to fully visible
        c.a = 1f;
        grassRenderer.color = c;
    }
}

/// perfect
