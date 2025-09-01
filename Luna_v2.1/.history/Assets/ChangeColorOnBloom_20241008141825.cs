using UnityEngine;

public class AdjustBackgroundAlpha : MonoBehaviour
{
    [Header("Settings")]
    public float alphaIncreaseStep = 0.2f; // How much the alpha should increase (20% each time)

    private SpriteRenderer spriteRenderer; // Reference to the background's SpriteRenderer
    private bool alphaAdjusted = false; // To track if alpha has been adjusted on this enablement

    private void Start()
    {
        // Cache the SpriteRenderer component of the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the SpriteRenderer exists
        if (spriteRenderer == null)
        {
            Debug.LogError("No SpriteRenderer found on: " + gameObject.name);
        }
    }

    private void Update()
    {
        // Check if "LitFlowerB" exists and if its SpriteRenderer is enabled
        GameObject litFlowerB = GameObject.FindWithTag("LitFlowerB");

        if (litFlowerB != null && litFlowerB.GetComponent<SpriteRenderer>().enabled && !alphaAdjusted)
        {
            // Adjust the background alpha only once per enablement
            AdjustAlpha();
            alphaAdjusted = true; // Prevent further adjustments after the first one
        }
        else if (litFlowerB != null && !litFlowerB.GetComponent<SpriteRenderer>().enabled)
        {
            // Reset the flag when LitFlowerB is disabled to allow future adjustments
            alphaAdjusted = false;
        }
    }

    // Method to adjust only the alpha of the background
    private void AdjustAlpha()
    {
        if (spriteRenderer != null)
        {
            // Get the current color of the background
            Color currentColor = spriteRenderer.color;

            // Increase the alpha value by the defined step, clamping it at 1 (fully opaque)
            float newAlpha = Mathf.Min(currentColor.a + alphaIncreaseStep, 1f);

            // Apply the new color with updated alpha, keeping RGB values unchanged
            spriteRenderer.color = new Color(currentColor.r, currentColor.g, currentColor.b, newAlpha);

            // Debug log to confirm alpha adjustment
            Debug.Log("Background alpha adjusted to: " + newAlpha);
        }
    }
}
