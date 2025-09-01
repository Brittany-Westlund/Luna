using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.2f; // Hard-coded 20% increase
    public string backgroundTag = "Background"; // Tag for background objects
    public string playerTag = "Player"; // Tag for Luna (player)

    private SpriteRenderer[] backgroundRenderers; // Array for all background SpriteRenderers
    private SpriteRenderer litFlowerRenderer; // Reference to LitFlowerB's SpriteRenderer
    private bool playerCollidingWithBackground = false; // Player collision status

    private void Start()
    {
        // Cache all background SpriteRenderers based on the background tag
        GameObject[] backgroundObjects = GameObject.FindGameObjectsWithTag(backgroundTag);
        backgroundRenderers = new SpriteRenderer[backgroundObjects.Length];

        for (int i = 0; i < backgroundObjects.Length; i++)
        {
            backgroundRenderers[i] = backgroundObjects[i].GetComponent<SpriteRenderer>();
        }

        // Find and cache the SpriteRenderer for LitFlowerB
        GameObject litFlowerB = GameObject.Find("LitFlowerB");
        if (litFlowerB != null)
        {
            litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();
        }
    }

    private void Update()
    {
        // Check if LitFlowerB is enabled and player is colliding with a background object
        if (litFlowerRenderer != null && litFlowerRenderer.enabled && playerCollidingWithBackground)
        {
            AdjustBackgroundColors();
        }
    }

    // Adjust the background colors by 20% towards white
    private void AdjustBackgroundColors()
    {
        foreach (SpriteRenderer renderer in backgroundRenderers)
        {
            if (renderer != null)
            {
                // Increase RGB values by 20%, ensuring they don't exceed 1 (white)
                float newR = Mathf.Min(renderer.color.r + colorIncreaseStep, 1f);
                float newG = Mathf.Min(renderer.color.g + colorIncreaseStep, 1f);
                float newB = Mathf.Min(renderer.color.b + colorIncreaseStep, 1f);

                // Apply the new color to the background object
                renderer.color = new Color(newR, newG, newB, renderer.color.a);

                // Log the color change for debugging
                Debug.Log("Background color adjusted to: " + renderer.color);
            }
        }

        // After adjusting, reset the condition to prevent repeated triggering
        playerCollidingWithBackground = false;
    }

    // Detect when the player collides with a background object
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerCollidingWithBackground = true;
            Debug.Log("Player started colliding with background.");
        }
    }

    // Reset collision status when the player leaves the background
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerCollidingWithBackground = false;
            Debug.Log("Player stopped colliding with background.");
        }
    }
}
