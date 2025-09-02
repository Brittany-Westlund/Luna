using UnityEngine;

public class AdjustBackgroundAlpha : MonoBehaviour
{
    [Header("Settings")]
    public float alphaIncreaseStep = 0.2f; // How much the alpha should increase (20% each time)

    private SpriteRenderer spriteRenderer; // Reference to the background's SpriteRenderer

    // Flags to track if each flower has adjusted the alpha
    private bool[] alphaAdjusted = new bool[5]; // One flag for each LitFlower (A-E)

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

        // Initialize all flags as false (no alpha adjustment has been made yet)
        for (int i = 0; i < alphaAdjusted.Length; i++)
        {
            alphaAdjusted[i] = false;
        }
    }

    private void Update()
    {
        // Loop through each flower tag (A-E)
        for (int i = 0; i < flowerTags.Length; i++)
        {
            string tag = flowerTags[i];
            GameObject[] litFlowerObjects = GameObject.FindGameObjectsWithTag(tag);

            // Check if any of the LitFlower objects with this tag have their SpriteRenderer enabled
            foreach (GameObject litFlower in litFlowerObjects)
            {
                SpriteRenderer litFlowerRenderer = litFlower.GetComponent<SpriteRenderer>();

                // If the LitFlower is enabled and hasn't adjusted the alpha yet
                if (litFlowerRenderer != null && litFlowerRenderer.enabled && !alphaAdjusted[i])
                {
                    AdjustAlpha(); // Increase the alpha
                    alphaAdjusted[i] = true; // Mark this flower type as having adjusted the alpha
                }
            }
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
