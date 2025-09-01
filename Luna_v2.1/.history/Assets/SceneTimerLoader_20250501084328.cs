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

        if (bgMusic != null)
        {
            musicSource = bgMusic.GetComponent<AudioSource>();
        }

        UpdateTimerUI();
    }

    void Update()
    {
        if (transitionStarted) return;

        timer -= Time.deltaTime;
        UpdateTimerUI();

        if (timer <= 0f)
        {
            transitionStarted = true;
            StartCoroutine(FadeOutMusicThenLoad());
        }
    }

    void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int seconds = Mathf.CeilToInt(Mathf.Max(timer, 0));
            timerText.text = FormatTime(seconds);
        }
    }

    string FormatTime(int totalSeconds)
    {
        int minutes = totalSeconds / 60;
        int seconds = totalSeconds % 60;
        return $"{minutes:D2}:{seconds:D2}";
    }

    System.Collections.IEnumerator FadeOutMusicThenLoad()
    {
        // Hide timer text
        if (timerText != null)
        {
            timerText.enabled = false;
        }

        // Fade music out
        if (musicSource != null && musicSource.isPlaying)
        {
            float startVol = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t);
                yield return null;
            }

            musicSource.volume = 0f;
            musicSource.Stop();
        }

        // Add tiny wait just to be sure fade finishes
        yield return new WaitForSeconds(0.1f);

        // Now load the scene
        SceneManager.LoadScene(sceneToLoad);
    }
}
