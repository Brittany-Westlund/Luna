using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelEndTrigger : MonoBehaviour
{
    public SpriteRenderer targetRenderer; // The SpriteRenderer that needs to be active
    public Text pointsText; // The UI Text that displays the points
    public string nextSceneName = "NextLevel"; // The name of the next scene to load
    public CanvasGroup fadeCanvasGroup; // The CanvasGroup that will be used for fading
    public float fadeDuration = 12f; // Duration for the fade effect

    private int requiredPoints = 10; // The minimum points required to trigger the level end

    void Update()
    {
        if (targetRenderer.enabled && GetPoints() >= requiredPoints)
        {
            StartCoroutine(TransitionToEndLevel());
        }
    }

    private int GetPoints()
    {
        int points = 0;
        int.TryParse(pointsText.text, out points);
        return points;
    }

    private IEnumerator TransitionToEndLevel()
    {
        // Fade to black over half of the fadeDuration
        yield return StartCoroutine(Fade(1f, fadeDuration / 2));
        
        // Load the next scene over the second half of the fadeDuration
        yield return StartCoroutine(Fade(0f, fadeDuration / 2));
        
        SceneManager.LoadScene(nextSceneName);
    }

    private IEnumerator Fade(float finalAlpha, float duration)
    {
        float counter = 0f;
        float initialAlpha = fadeCanvasGroup.alpha;

        while (counter < duration)
        {
            counter += Time.deltaTime;
            float alpha = Mathf.Lerp(initialAlpha, finalAlpha, counter / duration);
            fadeCanvasGroup.alpha = alpha;
            yield return null;
        }

        fadeCanvasGroup.alpha = finalAlpha;
    }
}
