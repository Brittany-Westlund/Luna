using UnityEngine;
using UnityEngine.UI;

public class ImageFlicker : MonoBehaviour
{
    [Header("Flicker")]
    [Range(0f, 1f)] public float minimumAlpha = 0.8f;
    [Range(0f, 1f)] public float maximumAlpha = 1f;
    [Min(0.01f)] public float cycleDuration = 1.5f;

    [Header("Optional Movement")]
    public bool animateScale = false;
    [Range(0.1f, 1f)] public float minimumScale = 0.94f;

    public bool animateDrop = false;
    [Min(0f)] public float dropDistance = 3f;

    private Graphic graphic;
    private SpriteRenderer spriteRenderer;
    private RectTransform rectTransform;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Vector2 originalAnchoredPosition;
    private float originalAlpha;

    private void Awake()
    {
        graphic = GetComponent<Graphic>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rectTransform = transform as RectTransform;

        originalScale = transform.localScale;
        originalPosition = transform.localPosition;

        if (rectTransform != null)
            originalAnchoredPosition = rectTransform.anchoredPosition;

        if (graphic != null)
            originalAlpha = graphic.color.a;
        else if (spriteRenderer != null)
            originalAlpha = spriteRenderer.color.a;
        else
        {
            Debug.LogWarning(
                "ImageFlicker requires an Image or SpriteRenderer.",
                this);
            enabled = false;
        }
    }

    private void Update()
    {
        float wave = 0.5f + 0.5f * Mathf.Sin(
            Time.unscaledTime / Mathf.Max(0.01f, cycleDuration)
            * Mathf.PI * 2f);

        float alpha = originalAlpha *
            Mathf.Lerp(minimumAlpha, maximumAlpha, wave);

        if (graphic != null)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
        else if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        if (animateScale)
        {
            float scale = Mathf.Lerp(minimumScale, 1f, wave);
            transform.localScale = originalScale * scale;
        }

        if (animateDrop)
        {
            float drop = (1f - wave) * dropDistance;

            if (rectTransform != null)
                rectTransform.anchoredPosition =
                    originalAnchoredPosition + Vector2.down * drop;
            else
                transform.localPosition =
                    originalPosition + Vector3.down * drop;
        }
    }
} 