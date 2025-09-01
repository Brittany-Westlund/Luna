using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
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
        // Find all objects with the tag "LitFlowerB"
        GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");

        foreach (GameObject litFlowerB in litFlowerBObjects)
        {
            // Get the SpriteRenderer component of LitFlowerB
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            // If LitFlowerB's renderer is enabled, adjust the background color
            if (litFlowerRenderer != null && litFlowerRenderer.enabled)
            {
                AdjustBackgroundColor();
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
