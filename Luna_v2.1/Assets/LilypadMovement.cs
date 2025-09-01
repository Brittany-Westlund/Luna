using System.Collections;
using UnityEngine;

public class LilypadMovement : MonoBehaviour
{
    public enum MovementMode { Mode1, Mode2, Mode3, Mode4 }
    public MovementMode currentMode;

    private SpriteRenderer spriteRenderer;
    private Collider2D lilypadCollider;
    private Vector3 originalPosition;
    private Vector3 originalScale;
    private const float loopDuration = 4.0f;
    private const float fadeDuration = 1f; // Shorter fade duration
    private const float opaqueDuration = 1.5f; // Duration to stay fully opaque
    private const float scaleMultiplier = 1.1f;
    private const float colliderDisableTransparency = 0.15f;
    private const float sinkAmount = 0.05f; // Amount to sink when fading

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        lilypadCollider = GetComponent<Collider2D>();
        originalPosition = transform.position;
        originalScale = transform.localScale;

        float startDelay = ((int)currentMode) * 1.0f;
        StartCoroutine(ModeBehavior(startDelay));
    }

    IEnumerator ModeBehavior(float delay)
    {
        yield return new WaitForSeconds(delay);

        while (true)
        {
            float elapsedTime = (Time.time - delay) % loopDuration;
            float fadePhase;

            if (elapsedTime < fadeDuration)
            {
                // Fade in
                fadePhase = elapsedTime / fadeDuration;
            }
            else if (elapsedTime < fadeDuration + opaqueDuration)
            {
                // Hold fully opaque
                fadePhase = 1.0f;
            }
            else
            {
                // Fade out
                fadePhase = 1.0f - (elapsedTime - fadeDuration - opaqueDuration) / fadeDuration;
            }
            
            fadePhase = Mathf.Clamp01(fadePhase);

            // Update transparency
            float transparency = fadePhase;
            spriteRenderer.color = new Color(1f, 1f, 1f, transparency);

            // Update scale
            float scale = Mathf.Lerp(1f, scaleMultiplier, fadePhase);
            transform.localScale = originalScale * scale;

            // Update position to simulate sinking
            float sink = Mathf.Lerp(0f, sinkAmount, 1 - transparency); // Sink more as the transparency decreases
            transform.position = new Vector3(originalPosition.x, originalPosition.y - sink, originalPosition.z);

            // Disable collider when transparency is lower than 0.2
            lilypadCollider.enabled = transparency >= colliderDisableTransparency;

            yield return null;
        }
    }
}
