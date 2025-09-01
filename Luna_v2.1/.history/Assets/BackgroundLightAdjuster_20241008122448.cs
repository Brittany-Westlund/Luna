using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // Ensure this is included to access MMProgressBar.

public class BackgroundLightAdjuster : MonoBehaviour
{
    [Header("References")]
    public MMProgressBar lightBar; // Reference to the MMProgressBar.
    public float lightChangeThreshold = 0.1f; // The percentage threshold to trigger a color change.
    public float colorIncreaseStep = 0.1f; // The percentage of color change towards white per trigger.
    
    [Header("Background Color Settings")]
    public Color initialBackgroundColor = new Color(112f / 255f, 112f / 255f, 112f / 255f, 1f); // Default initial color for the background
    
    private float previousLightValue;

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial background color for each object on the "Background" layer
        SetInitialBackgroundColors();

        // Initialize the previous light bar value
        previousLightValue = lightBar.BarProgress; 
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the light bar has decreased by the threshold
        if (previousLightValue - lightBar.BarProgress >= lightChangeThreshold)
        {
            // Trigger the background color adjustment
            AdjustBackgroundColor();

            // Update the previous light bar value to the new value
            previousLightValue = lightBar.BarProgress;
        }
    }

    void SetInitialBackgroundColors()
    {
        // Find all objects on the Background layer
        GameObject[] backgroundObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in backgroundObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Background"))
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    // Apply the initial color to the background object
                    renderer.color = initialBackgroundColor;

                    // Log the initial color for debugging
                    Debug.Log("Initial color set for: " + obj.name + " | Color: " + renderer.color);
                }
            }
        }
    }

    void AdjustBackgroundColor()
    {
        // Find all objects on the Background layer
        GameObject[] backgroundObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in backgroundObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Background"))
            {
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    // Get the current color of the background object
                    Color currentColor = renderer.color;

                    // Move the color closer to white based on the step
                    Color newColor = Color.Lerp(currentColor, Color.white, colorIncreaseStep);

                    // Log the color transition for debugging
                    Debug.Log("Current color: " + currentColor + " | New color: " + newColor);

                    // Apply the new color to the background object
                    renderer.color = newColor;
                }
                else
                {
                    // Log if the object doesn't have a SpriteRenderer
                    Debug.Log("Object " + obj.name + " does not have a SpriteRenderer.");
                }
            }
        }
    }
}
