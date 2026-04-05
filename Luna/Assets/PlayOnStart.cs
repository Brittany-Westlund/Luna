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
    public float fadeOutDelay = 3f;     // Wait this long before fading out
    public float fadeOutDuration = 1f;  // How long the fade takes

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null) return;

        if (useFadeIn)
        {
            audioSource.volume = 0f;
            audioSource.Play();
            StartCoroutine(FadeIn());
        }
        else
        {
            audioSource.volume = startVolume;
            audioSource.Play();
        }

        // Start fade out sequence if enabled
        if (useFadeOut)
        {
            StartCoroutine(FadeOutAfterDelay());
        }
    }

    private IEnumerator FadeIn()
    {
        float time = 0f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(0f, startVolume, time / fadeInDuration);
            yield return null;
        }

        audioSource.volume = startVolume;
    }

    private IEnumerator FadeOutAfterDelay()
    {
        yield return new WaitForSeconds(fadeOutDelay);
        yield return StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float time = 0f;
        float startVol = audioSource.volume;

        while (time < fadeOutDuration)
        {
            time += Time.deltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, time / fadeOutDuration);
            yield return null;
        }

        audioSource.volume = 0f;
        audioSource.Stop();
    }
}