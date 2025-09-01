using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.01f; // Step for how much each color channel increases per frame
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
            // Increase the background color manually based on new activations
            for (int i = 0; i < (currentActivatedCount - lastActivatedCount); i++)
            {
                IncrementBackgroundColor();
            }

            // Update the lastActivatedCount
            lastActivatedCount = currentActivatedCount;
        }
    }

    // Manually adjust the background color towards the target color
    void IncrementBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Get the current color of the sprite
            Color currentColor = spriteRenderer.color;

            // Increment each color channel by the defined step, clamping it to the target color
            float newR = Mathf.MoveTowards(currentColor.r, targetColor.r, colorIncreaseStep);
            float newG = Mathf.MoveTowards(currentColor.g, targetColor.g, colorIncreaseStep);
            float newB = Mathf.MoveTowards(currentColor.b, targetColor.b, colorIncreaseStep);

            // Set the new color
            spriteRenderer.color = new Color(newR, newG, newB, currentColor.a);

            // Optional: Log the color change for debugging
            Debug.Log("Object: " + gameObject.name + " | New Color: " + spriteRenderer.color);
        }
    }
}
