using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.02f; // Lower the step for smoother transition
    public Color targetColor = Color.white; // The color we want to approach (white by default)

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private int litFlowerBCount = 0; // Track how many LitFlowerBs are enabled

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
        // Find all objects with the name "LitFlowerB"
        GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");

        int enabledCount = 0;

        // Loop through each LitFlowerB object and check if it's enabled
        foreach (GameObject litFlowerB in litFlowerBObjects)
        {
            SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

            if (litFlowerRenderer != null && litFlowerRenderer.enabled)
            {
                enabledCount++;
            }
        }

        // If more LitFlowerBs are enabled than before, increase the background color
        if (enabledCount > litFlowerBCount)
        {
            litFlowerBCount = enabledCount;
            AdjustBackgroundColor();
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
