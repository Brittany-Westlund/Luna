using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class SimpleSceneFader : MonoBehaviour
{
    public float fadeDuration = 0.5f;

    private Image fadeImage;

    void Awake()
    {
        fadeImage = GetComponent<Image>();
        if (fadeImage == null)
        {
            Debug.LogError("SimpleSceneFader: No Image component found on this GameObject!");
            enabled = false;
        }
        else
        {
            // Make sure the image starts black and fully opaque
            Color c = fadeImage.color;
            c.a = 1f;
            fadeImage.color = c;
            fadeImage.raycastTarget = true; // Block clicks during fade
        }
    }

    void Start()
    {
        StartCoroutine(FadeOutRoutine());
    }

    IEnumerator FadeOutRoutine()
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        c.a = 1f;
        fadeImage.color = c;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        // Make sure we end fully transparent and pass clicks through
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.raycastTarget = false;
    }

    // (Optional) Call this to manually fade in again
    public void FadeIn(float duration)
    {
        StopAllCoroutines();
        StartCoroutine(FadeInRoutine(duration));
    }

    IEnumerator FadeInRoutine(float duration)
    {
        float elapsed = 0f;
        Color c = fadeImage.color;
        c.a = 0f;
        fadeImage.color = c;
        fadeImage.raycastTarget = true;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            c.a = alpha;
            fadeImage.color = c;
            yield return null;
        }
        c.a = 1f;
        fadeImage.color = c;
    }
}
