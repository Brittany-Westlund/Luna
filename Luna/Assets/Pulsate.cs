using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class Pulsate : MonoBehaviour
{
    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;
    public float scaleAmount = 0.1f;
    public float fadeAmount = 0.5f;

    [Header("External Control")]
    public float externalScaleMultiplier = 1f;
    public float externalSpeedMultiplier = 1f;

    [Header("Optional External Base Overrides")]
    [Tooltip("If true, another script can provide the base scale via SetBaseScale().")]
    public bool useExternalBaseScale = false;

    [Tooltip("If true, another script can provide the base RGB color via SetBaseColor(). Alpha will still pulse here.")]
    public bool useExternalBaseColor = false;

    private SpriteRenderer spriteRenderer;

    private Vector3 originalScale;
    private Vector3 externalBaseScale;
    private bool hasExternalBaseScale = false;

    private Color originalColor;
    private Color externalBaseColor;
    private bool hasExternalBaseColor = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        originalColor = spriteRenderer.color;

        externalBaseScale = originalScale;
        externalBaseColor = originalColor;
    }

    public void SetBaseScale(Vector3 newBaseScale)
    {
        externalBaseScale = newBaseScale;
        hasExternalBaseScale = true;
    }

    public void ClearExternalBaseScale()
    {
        hasExternalBaseScale = false;
        externalBaseScale = originalScale;
    }

    public void SetBaseColor(Color newBaseColor)
    {
        externalBaseColor = newBaseColor;
        hasExternalBaseColor = true;
    }

    public void ClearExternalBaseColor()
    {
        hasExternalBaseColor = false;
        externalBaseColor = originalColor;
    }

    private void Update()
    {
        float speed = pulseSpeed * externalSpeedMultiplier;

        float pulse = Mathf.Sin(Time.time * speed) * scaleAmount + 1f;
        float fade = Mathf.Sin(Time.time * speed) * fadeAmount + 1f - fadeAmount;

        Vector3 baseScale = (useExternalBaseScale && hasExternalBaseScale)
            ? externalBaseScale
            : originalScale;

        Color baseColor = (useExternalBaseColor && hasExternalBaseColor)
            ? externalBaseColor
            : originalColor;

        transform.localScale = baseScale * pulse * externalScaleMultiplier;

        Color finalColor = baseColor;
        finalColor.a = baseColor.a * fade;
        spriteRenderer.color = finalColor;
    }
}