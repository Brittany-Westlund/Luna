using UnityEngine;
using System.Collections;

public class GardenSparkleToggle : MonoBehaviour
{
    [Header("References")]
    public GameObject sparkles; // parent Sparkles object

    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    private SpriteRenderer[] sparkleRenderers;
    private float[] initialAlphas;
    private Coroutine fadeRoutine;
    private GardenGrowth growth; // reference to GardenGrowth for isGrown

    void Awake()
    {
        if (sparkles != null)
        {
            sparkleRenderers = sparkles.GetComponentsInChildren<SpriteRenderer>(true);
            initialAlphas = new float[sparkleRenderers.Length];
            for (int i = 0; i < sparkleRenderers.Length; i++)
                initialAlphas[i] = sparkleRenderers[i].color.a;
        }

        growth = GetComponent<GardenGrowth>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (growth != null && growthGrassHasGrown()) return;

        if (other.CompareTag("Player"))
        {
            StartFade(1f); // fade in to full alpha
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (growth != null && growthGrassHasGrown()) return;

        if (other.CompareTag("Player"))
        {
            StartFadeToInitial(); // fade back to initial alpha
        }
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeAllTo(targetAlpha));
    }

    private void StartFadeToInitial()
    {
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeAllToInitial());
    }

    private IEnumerator FadeAllTo(float targetAlpha)
    {
        if (sparkleRenderers == null) yield break;

        float elapsed = 0f;
        Color[] startColors = new Color[sparkleRenderers.Length];
        for (int i = 0; i < sparkleRenderers.Length; i++)
            startColors[i] = sparkleRenderers[i].color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            for (int i = 0; i < sparkleRenderers.Length; i++)
            {
                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, targetAlpha, t);
                sparkleRenderers[i].color = c;
            }
            yield return null;
        }

        // lock final
        for (int i = 0; i < sparkleRenderers.Length; i++)
        {
            Color c = sparkleRenderers[i].color;
            c.a = targetAlpha;
            sparkleRenderers[i].color = c;
        }
    }

    private IEnumerator FadeAllToInitial()
    {
        if (sparkleRenderers == null) yield break;

        float elapsed = 0f;
        Color[] startColors = new Color[sparkleRenderers.Length];
        for (int i = 0; i < sparkleRenderers.Length; i++)
            startColors[i] = sparkleRenderers[i].color;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            for (int i = 0; i < sparkleRenderers.Length; i++)
            {
                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, initialAlphas[i], t);
                sparkleRenderers[i].color = c;
            }
            yield return null;
        }

        // lock back to initial alphas
        for (int i = 0; i < sparkleRenderers.Length; i++)
        {
            Color c = sparkleRenderers[i].color;
            c.a = initialAlphas[i];
            sparkleRenderers[i].color = c;
        }
    }

    private bool growthGrassHasGrown()
    {
        return growth != null && (bool)growth.GetType()
            .GetField("isGrown", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            .GetValue(growth);
    }
}
