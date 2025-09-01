using System.Collections.Generic;
using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.3f; // Larger step for more visible transition
    public Color targetColor = Color.white; // The color we want to approach (white by default)
    public Color initialColor = Color.black; // Start with black for visible transition

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;

    // A dictionary to track which LitFlowerB objects have already triggered a color change
    private Dictionary<GameObject, bool> litFlowerBStates = new Dictionary<GameObject, bool>();
    private List<GameObject> activatedFlowers = new List<GameObject>(); // Track activated flowers separately

    private void Start()
    {
        // Get the SpriteRenderer component for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Set the initial background color to black to see the change
        if (spriteRenderer != null)
        {
            spriteRenderer.color = initialColor;
            Debug.Log("Initial color set to: " + spriteRenderer.color);
        }

        // Find all LitFlowerB objects and check if any are already enabled
        GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");
        foreach (GameObject litFlowerB in litFlowerBObjects)
        {
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            // Add the flower to the dictionary and check if it's already enabled
            litFlowerBStates.Add(litFlowerB, false);

            if (litFlowerRenderer != null && litFlowerRenderer.enabled)
            {
                // If it's already enabled at the start, process it
                AdjustBackgroundColor();
                litFlowerBStates[litFlowerB] = true; // Mark it as processed
            }
        }
    }

    private void Update()
    {
        activatedFlowers.Clear(); // Reset the list of flowers that will trigger the color change

        foreach (var flowerState in litFlowerBStates)
        {
            GameObject litFlowerB = flowerState.Key;
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            // If LitFlowerB's renderer is enabled and it hasn't triggered the color change yet
            if (litFlowerRenderer != null && litFlowerRenderer.enabled && !flowerState.Value)
            {
                activatedFlowers.Add(litFlowerB); // Mark this LitFlowerB to trigger the color change
            }
        }

        // Apply the color change for each flower that has been activated
        foreach (GameObject litFlowerB in activatedFlowers)
        {
            AdjustBackgroundColor();
            litFlowerBStates[litFlowerB] = true; // Mark the flower as having triggered the color change
        }
    }

    // Adjust the background color towards the target color (white)
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Gradually change the color by the step size, moving towards white
            Color newColor = Color.Lerp(spriteRenderer.color, targetColor, colorIncreaseStep);

            // Apply the new color and log it
            spriteRenderer.color = newColor;
            Debug.Log("Adjusted Color: " + spriteRenderer.color);
        }
    }
}
