using UnityEngine;
using MoreMountains.Tools; // Ensure this is included to access MMProgressBar.

public class BackgroundLightAdjuster : MonoBehaviour
{
    [Header("Settings")]
    public MMProgressBar lightBar; // Reference to the MMProgressBar.
    public float lightChangeThreshold = 0.1f; // Threshold to trigger a color change.
    public float colorIncreaseStep = 0.1f; // How much the color should move towards white.
    public Color targetColor = Color.white; // The color we want to approach (white by default)

    // Reference to the SpriteRenderer component
    private SpriteRenderer spriteRenderer;
    private Color originalColor;
    private float previousLightValue;

    private void Start()
    {
        // Get the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the original color of the sprite
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color; // Store the starting color
        }

        // Initialize the previous light bar value
        previousLightValue = lightBar.BarProgress;
    }

    private void Update()
    {
        // Check if the light bar has decreased by the threshold
        if (previousLightValue - lightBar.BarProgress >= lightChangeThreshold)
        {
            // Adjust the color of this object
            AdjustBackgroundColor();

            // Update the previous light bar value
            previousLightValue = lightBar.BarProgress;
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
