using UnityEngine;
using System.Collections;
using TMPro;

[RequireComponent(typeof(Collider2D))]
public class PromptFadeOnProximity : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;
    public float fadeOutDelay = 0.2f;
    public string playerTag = "Player";

    [Header("References")]
    public SpriteRenderer spriteRenderer;
    public TextMeshPro textMesh;
    public MeshRenderer meshRenderer;

    private Coroutine fadeRoutine;
    private bool isInRange = false;

    void Start()
    {
        // Ensure everything starts invisible
        SetAlpha(0f);

        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (textMesh == null) textMesh = GetComponent<TextMeshPro>();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInRange = true;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        StartSafeCoroutine(FadeTo(1f));
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        isInRange = false;
        if (fadeRoutine != null) StopCoroutine(fadeRoutine);
        StartSafeCoroutine(FadeOutAfterDelay());
    }

    private IEnumerator FadeOutAfterDelay()
    {
        if (!isActiveAndEnabled) yield break;
        yield return new WaitForSeconds(fadeOutDelay);
        if (isInRange || !isActiveAndEnabled) yield break;
        StartSafeCoroutine(FadeTo(0f));
    }

    private IEnumerator FadeTo(float targetAlpha)
    {
        if (!isActiveAndEnabled) yield break;

        float startAlpha = GetCurrentAlpha();
        float time = 0f;

        while (time < fadeDuration)
        {
            if (!isActiveAndEnabled) yield break;
            time += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, time / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
    }

    // ✅ Safe Coroutine Wrapper (prevents errors if object is inactive)
    private Coroutine StartSafeCoroutine(IEnumerator routine)
    {
        if (this != null && gameObject.activeInHierarchy && isActiveAndEnabled)
            return StartCoroutine(routine);
        return null;
    }

    private void SetAlpha(float alpha)
    {
        if (spriteRenderer != null)
        {
            Color c = spriteRenderer.color;
            c.a = alpha;
            spriteRenderer.color = c;
        }

        if (textMesh != null)
        {
            Color c = textMesh.color;
            c.a = alpha;
            textMesh.color = c;
        }

        if (meshRenderer != null && meshRenderer.material.HasProperty("_Color"))
        {
            Color c = meshRenderer.material.color;
            c.a = alpha;
            meshRenderer.material.color = c;
        }
    }

    private float GetCurrentAlpha()
    {
        if (spriteRenderer != null) return spriteRenderer.color.a;
        if (textMesh != null) return textMesh.color.a;
        if (meshRenderer != null && meshRenderer.material.HasProperty("_Color"))
            return meshRenderer.material.color.a;

        return 1f;
    }
}
