using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // Ensure this is included to access MMProgressBar.

public class BackgroundLightAdjuster : MonoBehaviour
{
    [Header("Settings")]
    public MMProgressBar lightBar; // Reference to the MMProgressBar.
    public float lightChangeThreshold = 0.1f; // Threshold to trigger a color change.
    public float colorIncreaseStep = 0.1f; // How much the color should move towards white.
    public Color initialBackgroundColor = new Color(0.44f, 0.44f, 0.44f, 1f); // Default starting color.

    private SpriteRenderer spriteRenderer;
    private float previousLightValue;

    // Start is called before the first frame update
    void Start()
    {
        // Cache the SpriteRenderer component
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            // Set the initial color for the object
            spriteRenderer.color = initialBackgroundColor;
        }

        // Initialize the previous light bar value
        previousLightValue = lightBar.BarProgress;
    }

    // Update is called once per frame
    void Update()
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

    // Adjust the background color towards white
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Get the current color of the object
            Color currentColor = spriteRenderer.color;

            // Move the color towards white
            Color newColor = Color.Lerp(currentColor, Color.white, colorIncreaseStep);

            // Apply the new color
            spriteRenderer.color = newColor;

            // Optional: Log for debugging
            Debug.Log("Object: " + gameObject.name + " | New Color: " + newColor);
        }
    }
}
