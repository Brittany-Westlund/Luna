using UnityEngine;

public class ChangeAlphaOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float alphaIncreaseStep = 0.2f; // How much the alpha should increase (20% each time)

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;
    private float originalAlpha;

    private void Start()
    {
        // Get the SpriteRenderer component for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the initial alpha value of the background
        if (spriteRenderer != null)
        {
            originalAlpha = spriteRenderer.color.a;
            Debug.Log("Initial alpha: " + originalAlpha);
        }
    }

    private void Update()
    {
        // Check if "LitFlowerB" exists and if its SpriteRenderer is enabled
        GameObject litFlowerB = GameObject.Find("LitFlowerB");

        if (litFlowerB != null)
        {
            // Get the SpriteRenderer component of LitFlowerB
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            // If LitFlowerB's renderer is enabled, adjust the background alpha
            if (litFlowerRenderer != null && litFlowerRenderer.enabled)
            {
                AdjustBackgroundAlpha();
            }
        }
    }

    // Adjust the background alpha towards full opacity (1)
    void AdjustBackgroundAlpha()
    {
        if (spriteRenderer != null)
        {
            // Get current color
            Color currentColor = spriteRenderer.color;

            // Gradually increase the alpha, ensuring it doesn't exceed 1 (fully opaque)
            float newAlpha = Mathf.Min(currentColor.a + alphaIncreaseStep, 1f);

            // Apply the new color with updated alpha
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            // Optional: Log the alpha change for debugging
            Debug.Log("Object: " + gameObject.name + " | New Alpha: " + spriteRenderer.color.a);
        }
    }
}
