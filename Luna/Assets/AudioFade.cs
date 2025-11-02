using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(AudioSource))]
public class AudioFade : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 2f;
    public float fadeOutDelay = 0f;
    public bool playOnStart = false;
    public bool fadeInOnStart = true;
    public bool persistAcrossScenes = true; // 🔸 stays alive between levels

    private AudioSource audioSource;
    private Coroutine currentFade;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();

        if (persistAcrossScenes)
        {
            DontDestroyOnLoad(gameObject);

            // Ensure only one AudioFade persists (avoid duplicates stacking)
            var existing = FindObjectsOfType<AudioFade>();
            foreach (var fade in existing)
            {
                if (fade != this && fade.persistAcrossScenes)
                {
                    Destroy(gameObject);
                    return;
                }
            }
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        if (playOnStart)
        {
            audioSource.volume = fadeInOnStart ? 0f : 1f;
            audioSource.Play();
            if (fadeInOnStart) FadeIn();
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Optionally fade out automatically when scene changes
        FadeOut(0f, fadeDuration);
    }

    // 🌅 Fade In
    public void FadeIn(float duration = -1f)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeRoutine(audioSource.volume, 1f, duration > 0 ? duration : fadeDuration));
    }

    // 🌒 Fade Out
    public void FadeOut(float delay = -1f, float duration = -1f)
    {
        if (currentFade != null) StopCoroutine(currentFade);
        currentFade = StartCoroutine(FadeOutRoutine(
            delay > 0 ? delay : fadeOutDelay,
            duration > 0 ? duration : fadeDuration
        ));
    }

    private IEnumerator FadeOutRoutine(float delay, float duration)
    {
        if (delay > 0f)
            yield return new WaitForSeconds(delay);

        yield return StartCoroutine(FadeRoutine(audioSource.volume, 0f, duration));
    }

    private IEnumerator FadeRoutine(float from, float to, float duration)
    {
        if (!audioSource.isPlaying)
            audioSource.Play();

        float elapsed = 0f;
        float startVol = from;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            audioSource.volume = Mathf.Lerp(startVol, to, t);
            yield return null;
        }

        audioSource.volume = to;
    }
}
