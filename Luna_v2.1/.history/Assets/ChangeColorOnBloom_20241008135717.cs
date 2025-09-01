using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.2f; // Lighten by 20% each time
    public string backgroundTag = "Background"; // Tag to identify background objects
    public string playerTag = "Player"; // Tag to identify Luna (player)

    private SpriteRenderer[] backgroundRenderers; // Array to hold all background SpriteRenderers
    private SpriteRenderer litFlowerRenderer; // Reference to LitFlowerB's SpriteRenderer
    private bool playerCollidingWithBackground = false; // Check if player is colliding with background

    private void Start()
    {
        // Cache all background SpriteRenderers based on the background tag
        GameObject[] backgroundObjects = GameObject.FindGameObjectsWithTag(backgroundTag);
        backgroundRenderers = new SpriteRenderer[backgroundObjects.Length];

        for (int i = 0; i < backgroundObjects.Length; i++)
        {
            backgroundRenderers[i] = backgroundObjects[i].GetComponent<SpriteRenderer>();
        }

        // Find the "LitFlowerB" and cache its SpriteRenderer
        GameObject litFlowerB = GameObject.Find("LitFlowerB");
        if (litFlowerB != null)
        {
            litFlowerRenderer = litFlowerB.GetComponent<SpriteRenderer>();
        }

        // Log an error if LitFlowerB's SpriteRenderer is missing
        if (litFlowerRenderer == null)
        {
            Debug.LogError("SpriteRenderer is missing on LitFlowerB.");
        }
    }

    private void Update()
    {
        // Only adjust the background colors if LitFlowerB's renderer is enabled and player is colliding
        if (litFlowerRenderer != null && litFlowerRenderer.enabled && playerCollidingWithBackground)
        {
            AdjustBackgroundColors();
        }
    }

    // Adjust all background colors by 20% toward white
    private void AdjustBackgroundColors()
    {
        foreach (SpriteRenderer renderer in backgroundRenderers)
        {
            if (renderer != null)
            {
                // Calculate the new RGB values, moving 20% towards white
                float newR = Mathf.Min(renderer.color.r + colorIncreaseStep, 1f);
                float newG = Mathf.Min(renderer.color.g + colorIncreaseStep, 1f);
                float newB = Mathf.Min(renderer.color.b + colorIncreaseStep, 1f);

                // Apply the new color
                renderer.color = new Color(newR, newG, newB, renderer.color.a);
                Debug.Log("Background color adjusted to: " + renderer.color);
            }
        }
    }

    // Detect when the player starts colliding with a background object
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerCollidingWithBackground = true;
            Debug.Log("Player started colliding with background.");
        }
    }

    // Detect when the player stops colliding with a background object
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerCollidingWithBackground = false;
            Debug.Log("Player stopped colliding with background.");
        }
    }
}
