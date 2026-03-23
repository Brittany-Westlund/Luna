using UnityEngine;

public class Pulsate : MonoBehaviour
{
    public float pulseSpeed = 2f;
    public float scaleAmount = 0.1f;
    public float fadeAmount = 0.5f;

    [Header("External Control")]
    public float externalScaleMultiplier = 1f;
    public float externalSpeedMultiplier = 1f;

    private SpriteRenderer spriteRenderer;
    private Vector3 originalScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer == null)
        {
            Debug.LogError("Pulsate script requires a SpriteRenderer component");
            enabled = false;
            return;
        }

        originalScale = transform.localScale;
    }

    void Update()
    {
        float speed = pulseSpeed * externalSpeedMultiplier;

        float pulse = Mathf.Sin(Time.time * speed) * scaleAmount + 1f;
        float fade = Mathf.Sin(Time.time * speed) * fadeAmount + 1f - fadeAmount;

        transform.localScale = originalScale * pulse * externalScaleMultiplier;

        Color current = spriteRenderer.color;
        current.a = fade;
        spriteRenderer.color = current;
    }
}