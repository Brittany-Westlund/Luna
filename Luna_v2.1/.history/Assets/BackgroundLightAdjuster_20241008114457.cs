using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools; // Include MoreMountains namespace to access MMProgressBar.

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
                    // Get the current color
                    Color currentColor = renderer.color;

                    // Move the color 10% closer to white
                    Color newColor = Color.Lerp(currentColor, Color.white, colorIncreaseStep);

                    // Apply the new color
                    renderer.color = newColor;
                }
            }
        }
    }
}
