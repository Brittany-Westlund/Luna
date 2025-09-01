using UnityEngine;

public class SimpleBackgroundColorChange : MonoBehaviour
{
    // Reference to the SpriteRenderer component of the background
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        // Get the SpriteRenderer component for the background object
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Log the initial background color for confirmation
        if (spriteRenderer != null)
        {
            Debug.Log("Initial color of background: " + spriteRenderer.color);
        }
        else
        {
            Debug.LogError("SpriteRenderer missing on DarkenedBackground!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Log any object that triggers the collision
        Debug.Log("Collision detected with: " + other.name);

        // Check if the player collides with the DarkenedBackground
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player collided with DarkenedBackground.");

            // Find LitFlowerB objects and check if any are enabled
            GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag("LitFlowerB");

            foreach (GameObject litFlowerB in litFlowerBObjects)
            {
                SpriteRenderer litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();

                // If LitFlowerB is enabled (through the renderer), change the background color
                if (litFlowerRenderer != null && litFlowerRenderer.enabled)
                {
                    Debug.Log("LitFlowerB is enabled. Changing background color...");

                    // Change the background color to light gray
                    spriteRenderer.color = Color.gray;

                    // Log the new color for debugging
                    Debug.Log("New Background Color: " + spriteRenderer.color);
                    break; // Exit after one change
                }
                else
                {
                    Debug.LogWarning("LitFlowerB renderer is not enabled or missing.");
                }
            }
        }
    }
}
