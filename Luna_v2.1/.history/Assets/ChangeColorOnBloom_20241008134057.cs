using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.2f; // Lighten by 20% each time
    public Color targetColor = Color.white; // The color we want to approach (white by default)

    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Get the SpriteRenderer component for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure the initial color is set to something darker than white
        if (spriteRenderer != null)
        {
            Debug.Log("Initial color of background set to: " + spriteRenderer.color);
        }
        else
        {
            Debug.LogError("SpriteRenderer missing on DarkenedBackground!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log when any collision happens
        Debug.Log("Collision detected with: " + other.name);  // Log any object that triggers the collision
        
        // Check if the player collides with the DarkenedBackground
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with DarkenedBackground.");

            // Find LitFlowerB objects and check if they are enabled
            GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");
            Debug.Log("Number of LitFlowerB objects found: " + litFlowerBObjects.Length);

            foreach (GameObject litFlowerB in litFlowerBObjects)
            {
                Debug.Log("Checking LitFlowerB object: " + litFlowerB.name);

                SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

                // If LitFlowerB is enabled (through the renderer), lighten the background
                if (litFlowerRenderer != null)
                {
                    Debug.Log("LitFlowerB Renderer found for: " + litFlowerB.name);

                    if (litFlowerRenderer.enabled)
                    {
                        Debug.Log("LitFlowerB is enabled. Lightening background...");
                        AdjustBackgroundColor();
                        break; // Lighten only once per collision
                    }
                    else
                    {
                        Debug.LogWarning("LitFlowerB renderer is not enabled: " + litFlowerB.name);
                    }
                }
                else
                {
                    Debug.LogError("LitFlowerB SpriteRenderer is missing on: " + litFlowerB.name);
                }
            }
        }
        else
        {
            Debug.Log("Collision detected with object other than Player: " + other.name);
        }
    }

    // Manually adjust the background color towards white by increasing the RGB values
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Get the current background color
            Color currentColor = spriteRenderer.color;
            Debug.Log("Current background color before adjustment: " + currentColor);

            // Calculate the new RGB values, lightening each channel by 20%
            float newR = Mathf.Min(currentColor.r + colorIncreaseStep, 1f); // Ensure it doesn't exceed 1 (white)
            float newG = Mathf.Min(currentColor.g + colorIncreaseStep, 1f);
            float newB = Mathf.Min(currentColor.b + colorIncreaseStep, 1f);

            // Apply the new color to the background
            spriteRenderer.color = new Color(newR, newG, newB, currentColor.a); // Keep the same alpha value

            // Log the new color for debugging
            Debug.Log("Background Color after adjustment: " + spriteRenderer.color);
        }
        else
        {
            Debug.LogError("SpriteRenderer is missing on DarkenedBackground, cannot adjust color.");
        }
    }
}
