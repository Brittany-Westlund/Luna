using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PixelCrushers;
using MoreMountains.CorgiEngine; // If your BackgroundMusic class lives here

public class SceneTimerLoader : MonoBehaviour
{
    public float delay = 30f;
    public string sceneToLoad = "NextSceneName";
    public Text timerText;

    [Header("Music Fade")]
    public BackgroundMusic bgMusic; // Drag in your BackgroundMusic component
    public float fadeDuration = 2f;

    private float timer;
    private bool isFading = false;
    private AudioSource musicSource;

    void Start()
    {
        timer = delay;
        musicSource = bgMusic != null ? bgMusic.GetComponent<AudioSource>() : null;
        UpdateTimerUI();
    }

    void Update()
    {
        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (!isFading && timer <= fadeDuration && musicSource != null)
        {
            StartCoroutine(FadeOutMusic());
            isFading = true;
        }

        if (timer <= 0f)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(timer);
            timerText.text = FormatTime(seconds);
        }
    }

    string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    System.Collections.IEnumerator FadeOutMusic()
    {
        float startVolume = musicSource.volume;

        for (float t = 0; t < fadeDuration; t += Time.deltaTime)
        {
            musicSource.volume = Mathf.Lerp(startVolume, 0f, t / fadeDuration);
            yield return null;
        }

        musicSource.volume = 0f;
        musicSource.Stop();
    }
}
