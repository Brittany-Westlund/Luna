using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // Ensure this is included to access MMProgressBar.

public class BackgroundLightAdjuster : MonoBehaviour
{
    [Header("References")]
    public MMProgressBar lightBar; // Reference to the MMProgressBar.
    public float lightChangeThreshold = 0.1f; // How much the light bar needs to increase to trigger a color change.
    public float colorIncreaseStep = 0.1f; // The percentage of color change towards white per trigger.

    private float previousLightValue;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the previous light bar value
        previousLightValue = lightBar.BarProgress; // Use BarProgress for the current fill amount of the progress bar
    }

    // Update is called once per frame
    void Update()
    {
        // Check if the light bar has increased by the threshold
        if (lightBar.BarProgress > previousLightValue + lightChangeThreshold)
        {
            // Trigger the background color adjustment
            AdjustBackgroundColor();

            // Update the previous light bar value to the new value
            previousLightValue = lightBar.BarProgress;
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
                // Get the object's renderer to modify its color
                SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();

                if (renderer != null)
                {
                    // Log the object name and its layer for debugging
                    Debug.Log("Adjusting color of: " + obj.name + " | Layer: " + LayerMask.LayerToName(obj.layer));

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
