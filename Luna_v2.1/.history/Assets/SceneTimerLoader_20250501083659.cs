using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PixelCrushers;
using MoreMountains.CorgiEngine;

public class SceneTimerLoader : MonoBehaviour
{
    public float delay = 30f;
    public string sceneToLoad = "NextSceneName";
    public Text timerText;

    [Header("Music Fade")]
    public BackgroundMusic bgMusic; // Drag in your BackgroundMusic component
    public float fadeDuration = 2f;

    private float timer;
    private bool transitionStarted = false;
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

        if (!transitionStarted && timer <= 0f)
        {
            transitionStarted = true;
            StartCoroutine(FadeOutAndLoadScene());
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

    System.Collections.IEnumerator FadeOutAndLoadScene()
    {
        if (musicSource != null)
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

        SceneManager.LoadScene(sceneToLoad);
    }
}
