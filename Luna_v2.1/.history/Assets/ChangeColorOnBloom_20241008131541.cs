using System.Collections.Generic;
using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.1f; // How much the color should move towards white
    public Color targetColor = Color.white; // The color we want to approach (white by default)

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    // A dictionary to track which LitFlowerB objects have already triggered a color change
    private Dictionary<GameObject, bool> litFlowerBStates = new Dictionary<GameObject, bool>();

    private void Start()
    {
        // Get the SpriteRenderer component for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the initial color of the background
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }

        // Find all LitFlowerB objects and initialize them as not activated
        GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");
        foreach (GameObject litFlowerB in litFlowerBObjects)
        {
            litFlowerBStates.Add(litFlowerB, false); // Initially, none have triggered the color change
        }
    }

    private void Update()
    {
        // Loop through all tracked LitFlowerB objects
        foreach (KeyValuePair<GameObject, bool> flowerState in litFlowerBStates)
        {
            GameObject litFlowerB = flowerState.Key;
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            // If LitFlowerB's renderer is enabled and it hasn't triggered the color change yet
            if (litFlowerRenderer != null && litFlowerRenderer.enabled && !flowerState.Value)
            {
                // Trigger the color adjustment
                AdjustBackgroundColor();

                // Mark this LitFlowerB as having triggered the color change
                litFlowerBStates[litFlowerB] = true;
            }
        }
    }

    // Adjust the background color towards the target color (white)
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Gradually change the color by the step size, moving towards white
            Color newColor = Color.Lerp(spriteRenderer.color, targetColor, colorIncreaseStep);

            // Apply the new color
            spriteRenderer.color = newColor;

            // Optional: Log the color change for debugging
            Debug.Log("Object: " + gameObject.name + " | New Color: " + spriteRenderer.color);
        }
    }
}
