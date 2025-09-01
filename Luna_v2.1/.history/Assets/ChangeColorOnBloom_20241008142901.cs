using UnityEngine;

public class AdjustBackgroundAlpha : MonoBehaviour
{
    [Header("Settings")]
    public float alphaIncreaseStep = 0.2f; // How much the alpha should increase (20% each time)

    private SpriteRenderer spriteRenderer; // Reference to the background's SpriteRenderer
    private bool alphaAdjusted = false; // To track if alpha has been adjusted after any flower enablement

    private string[] flowerTags = { "LitFlowerA", "LitFlowerB", "LitFlowerC", "LitFlowerD", "LitFlowerE" }; // Tags for each flower type

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
        // Check if any of the LitFlowerA, LitFlowerB, LitFlowerC, LitFlowerD, or LitFlowerE objects are enabled
        bool anyLitFlowerEnabled = false;

        foreach (string tag in flowerTags)
        {
            GameObject[] litFlowerObjects = GameObject.FindGameObjectsWithTag(tag);

            // Check if any of the objects with this tag have their SpriteRenderer enabled
            foreach (GameObject litFlower in litFlowerObjects)
            {
                SpriteRenderer litFlowerRenderer = litFlower.GetComponent<SpriteRenderer>();

                if (litFlowerRenderer != null && litFlowerRenderer.enabled)
                {
                    anyLitFlowerEnabled = true;
                    break; // If one is enabled, proceed to adjust alpha
                }
            }

            // If we found an enabled flower, no need to continue checking other tags
            if (anyLitFlowerEnabled)
            {
                break;
            }
        }

        // Adjust the alpha only if one flower is enabled and alpha hasn't been adjusted yet
        if (anyLitFlowerEnabled && !alphaAdjusted)
        {
            AdjustAlpha();
            alphaAdjusted = true; // Ensure it only adjusts once per enablement phase
        }
        else if (!anyLitFlowerEnabled)
        {
            // Reset the flag to allow future adjustments when flowers are re-enabled
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
