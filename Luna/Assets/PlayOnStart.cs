using UnityEngine;
using System.Collections;

public class PlayOnStart : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;

    [Range(0f, 1f)]
    public float startVolume = 1f;

    [Header("Optional Fade In")]
    public bool useFadeIn = false;
    public float fadeInDuration = 1f;

    [Header("Optional Fade Out")]
    public bool useFadeOut = false;
    public float fadeOutDelay = 3f;
    public float fadeOutDuration = 1f;

    private Coroutine currentFade;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            Debug.LogWarning("PlayOnStart: No AudioSource assigned.");
            return;
        }

        if (useFadeIn)
        {
            audioSource.volume = 0f;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
            currentFade = StartCoroutine(FadeRoutine(0f, startVolume, fadeInDuration, false));
        }
        else
        {
            audioSource.volume = startVolume;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }

        if (useFadeOut)
        {
            StartCoroutine(FadeOutAfterDelay());
        }
    }

    public void FadeIn(float duration)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("PlayOnStart.FadeIn: No AudioSource assigned.");
            return;
        }

        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }

        if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        currentFade = StartCoroutine(FadeRoutine(audioSource.volume, startVolume, duration, false));
    }

    public void FadeOut(float duration)
    {
        if (audioSource == null)
        {
            Debug.LogWarning("PlayOnStart.FadeOut: No AudioSource assigned.");
            return;
        }

        if (currentFade != null)
        {
            StopCoroutine(currentFade);
        }

        currentFade = StartCoroutine(FadeRoutine(audioSource.volume, 0f, duration, true));
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(fadeOutDelay);
        FadeOut(fadeOutDuration);
    }

    private IEnumerator FadeRoutine(float start, float end, float duration, bool stopAtEnd)
    {
        duration = Mathf.Max(0.0001f, duration);

        float time = 0f;
        audioSource.volume = start;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            audioSource.volume = Mathf.Lerp(start, end, t);
            yield return null;
        }

        audioSource.volume = end;

        if (stopAtEnd && Mathf.Approximately(end, 0f))
        {
            audioSource.Stop();
        }

        currentFade = null;
    }
}