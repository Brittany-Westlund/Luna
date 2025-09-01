using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.1f; // Step for color change towards white
    public Color targetColor = Color.white; // Target color (white by default)

    private SpriteRenderer spriteRenderer; // Reference to the SpriteRenderer of the background
    private SpriteRenderer litFlowerRenderer; // Reference to LitFlowerB's SpriteRenderer

    private void Start()
    {
        // Cache the SpriteRenderer for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Find the "LitFlowerB" and cache its SpriteRenderer
        GameObject litFlowerB = GameObject.Find("LitFlowerB");
        if (litFlowerB != null)
        {
            litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();
        }

        // Log an error if either SpriteRenderer is missing
        if (spriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer is missing on DarkenedBackground.");
        }
        if (litFlowerRenderer == null)
        {
            Debug.LogError("SpriteRenderer is missing on LitFlowerB.");
        }
    }

    private void Update()
    {
        // Only adjust the background color if LitFlowerB's renderer is enabled
        if (litFlowerRenderer != null && litFlowerRenderer.enabled)
        {
            AdjustBackgroundColor();
        }
    }

    // Gradually adjust the background color towards white
    private void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, colorIncreaseStep);
            Debug.Log("Background color adjusted: " + spriteRenderer.color);
        }
    }
}
