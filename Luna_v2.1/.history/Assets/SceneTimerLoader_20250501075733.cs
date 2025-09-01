using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTimerLoader : MonoBehaviour
{
    public float delay = 30f;
    public string sceneToLoad = "NextSceneName";
    public Text timerText; // Drag your UI Text object here

    private float timer;

    void Start()
    {
        timer = delay;
        UpdateTimerUI();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        UpdateTimerUI();

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
}
