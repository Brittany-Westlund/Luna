using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.01f; // Small step for gradual transition
    public Color targetColor = Color.white; // The color we want to approach (white by default)

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private int lastActivatedCount = 0; // Track the previous count of activated LitFlowerBs

    private void Start()
    {
        // Get the SpriteRenderer component for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the initial color of the background
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    private void Update()
    {
        // Find all objects with the tag "LitFlowerB"
        GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");

        int currentActivatedCount = 0;

        // Count how many LitFlowerB objects are enabled
        foreach (GameObject litFlowerB in litFlowerBObjects)
        {
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            if (litFlowerRenderer != null && litFlowerRenderer.enabled)
            {
                currentActivatedCount++;
            }
        }

        // Check if new LitFlowerBs were activated since the last frame
        if (currentActivatedCount > lastActivatedCount)
        {
            // Increase the background color incrementally based on the number of new activations
            for (int i = 0; i < (currentActivatedCount - lastActivatedCount); i++)
            {
                AdjustBackgroundColor();
            }

            // Update the lastActivatedCount
            lastActivatedCount = currentActivatedCount;
        }
    }

    // Adjust the background color towards the target color (white)
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Gradually change the color to the target color (white by default)
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, colorIncreaseStep);

            // Log the color change for debugging
            Debug.Log("Object: " + gameObject.name + " | New Color: " + spriteRenderer.color);
        }
    }
}
