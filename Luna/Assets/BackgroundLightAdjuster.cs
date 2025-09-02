using UnityEngine;

public class BackgroundLightAdjuster : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.1f; // How much the color should move towards white
    public Color targetColor = Color.white; // The color we want to approach (white by default)

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

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
        // Find all objects with a component called "LitFlowerRenderer"
        GameObject[] litFlowerObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in litFlowerObjects)
        {
            // Check if the object has a SpriteRenderer named "LitFlowerRenderer"
            SpriteRenderer litFlowerRenderer = obj.GetComponent<SpriteRenderer>();

            if (litFlowerRenderer != null && obj.name.Contains("LitFlowerRenderer") && litFlowerRenderer.enabled)
            {
                // If any LitFlowerRenderer is enabled, adjust the background color
                AdjustBackgroundColor();
                break; // Break after detecting the first enabled one, no need to continue
            }
        }
    }

    // Adjust the background color towards the target color (white)
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Gradually change the color to the target color (white by default)
            spriteRenderer.color = Color.Lerp(spriteRenderer.color, targetColor, colorIncreaseStep);

            // Optional: Log the color change for debugging
            Debug.Log("Object: " + gameObject.name + " | New Color: " + spriteRenderer.color);
        }
    }
}
