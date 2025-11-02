using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class BackgroundLevelTransition : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveDistance = 100f;         // How far down to move
    public float moveDuration = 1.5f;         // Time to move
    public float holdDuration = 2f;           // How long to hold before transition
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Crossfade Settings")]
    public Image fadeOverlay;                 // A full-screen UI Image (black or white) over the canvas
    public float fadeDuration = 1.5f;         // Fade in/out duration
    public Color fadeColor = Color.black;     // Fade color

    [Header("Next Level Settings")]
    public string nextSceneName;

    private RectTransform rectTransform;
    private Vector2 initialPosition;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;

        // Ensure fade overlay starts transparent
        if (fadeOverlay != null)
        {
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, 0f);
        }
    }

    void OnEnable()
    {
        StopAllCoroutines();
        StartCoroutine(MoveAndTransition());
    }

    private IEnumerator MoveAndTransition()
    {
        // Step 1: Move down with easing
        Vector2 targetPosition = initialPosition - new Vector2(0, moveDistance);
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float t = moveCurve.Evaluate(elapsed / moveDuration);
            rectTransform.anchoredPosition = Vector2.Lerp(initialPosition, targetPosition, t);
            yield return null;
        }

        // Step 2: Hold
        yield return new WaitForSeconds(holdDuration);

        // Step 3: Fade out, then load next level
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeOverlay(0f, 1f));  // Fade out
        }

        if (!string.IsNullOrEmpty(nextSceneName))
        {
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(nextSceneName);
            while (!asyncLoad.isDone)
                yield return null;
        }

        // Step 4: Optional fade back in after scene load
        if (fadeOverlay != null)
        {
            yield return StartCoroutine(FadeOverlay(1f, 0f));  // Fade in
        }
    }

    private IEnumerator FadeOverlay(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;
        Color color = fadeOverlay.color;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeOverlay.color = new Color(fadeColor.r, fadeColor.g, fadeColor.b, a);
            yield return null;
        }
    }
}
