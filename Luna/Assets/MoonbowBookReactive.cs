using UnityEngine;
using System.Collections;

public class MoonbowBookReactive : MonoBehaviour
{
    [Header("Fade Targets")]
    [SerializeField] private SpriteRenderer[] spriteRenderers;

    [Header("Fade Settings")]
    [SerializeField] private float fadeDuration = 0.35f;
    [SerializeField] private float hiddenAlpha = 0f;
    [SerializeField] private float visibleAlpha = 1f;

    private Coroutine fadeRoutine;
    private bool triggeredByBook = false;

    private void Awake()
    {
        if (spriteRenderers == null || spriteRenderers.Length == 0)
        {
            spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        }

        ApplyAlpha(hiddenAlpha);
    }

    public void SetTriggeredByBook(bool isActive)
    {
        if (triggeredByBook == isActive)
            return;

        triggeredByBook = isActive;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        fadeRoutine = StartCoroutine(FadeTo(isActive ? visibleAlpha : hiddenAlpha));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        float startAlpha = GetCurrentAlpha();
        float elapsed = 0f;

        if (fadeDuration <= 0f)
        {
            ApplyAlpha(targetAlpha);
            yield break;
        }

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            ApplyAlpha(alpha);
            yield return null;
        }

        ApplyAlpha(targetAlpha);
    }

    private void ApplyAlpha(float alpha)
    {
        alpha = Mathf.Clamp01(alpha);

        if (spriteRenderers == null)
            return;

        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            if (spriteRenderers[i] == null)
                continue;

            Color c = spriteRenderers[i].color;
            c.a = alpha;
            spriteRenderers[i].color = c;
        }
    }

    private float GetCurrentAlpha()
    {
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    return spriteRenderers[i].color.a;
            }
        }

        return hiddenAlpha;
    }
}