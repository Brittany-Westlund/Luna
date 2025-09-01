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
            Debug.Log("Initial color set to: " + spriteRenderer.color);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the player collides with the DarkenedBackground and there is a LitFlowerB enabled
        if (other.CompareTag("Player"))
        {
            GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");

            foreach (GameObject litFlowerB in litFlowerBObjects)
            {
                SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

                // If LitFlowerB is enabled (assumed through the renderer), lighten the background
                if (litFlowerRenderer != null && litFlowerRenderer.enabled)
                {
                    AdjustBackgroundColor();
                    break; // Lighten only once per collision
                }
            }
        }
    }

    // Adjust the background color towards the target color (white) by 20%
    void AdjustBackgroundColor()
    {
        if (spriteRenderer != null)
        {
            // Calculate the new color, lightening by 20%
            Color newColor = Color.Lerp(spriteRenderer.color, targetColor, colorIncreaseStep);

            // Apply the new color
            spriteRenderer.color = newColor;

            // Optional: Log the color change for debugging
            Debug.Log("Background Color after adjustment: " + spriteRenderer.color);
        }
    }
}
