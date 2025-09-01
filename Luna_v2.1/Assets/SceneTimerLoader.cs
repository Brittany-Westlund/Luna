using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTimerLoader : MonoBehaviour
{
    public float delay = 30f;
    public string sceneToLoad = "NextSceneName";
    public Text timerText;

    [Header("Music Fade")]
    public AudioSource musicSource;  // Drag your AudioSource directly here
    public float fadeDuration = 2f;

    private float timer;
    private bool transitionStarted = false;

    void Start()
    {
        timer = delay;
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
            StartCoroutine(FadeOutAndLoadScene());
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

    System.Collections.IEnumerator FadeOutAndLoadScene()
    {
        // Hide timer text
        if (timerText != null)
        {
            timerText.enabled = false;
        }

        // Fade out music
        if (musicSource != null && musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / fadeDuration;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, t);
                yield return null;
            }

            musicSource.volume = 0f;
            musicSource.Stop();
        }

        // Slight buffer before loading
        yield return new WaitForSeconds(0.1f);

        SceneManager.LoadScene(sceneToLoad);
    }
}
