using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.2f; // Lighten by 20% each time
    public string backgroundTag = "Background"; // Tag for background objects
    public string playerTag = "Player"; // Tag for Luna (player)
    public string litFlowerBTag = "LitFlowerB"; // Tag for LitFlowerB objects

    private SpriteRenderer[] backgroundRenderers; // Array for all background SpriteRenderers
    private SpriteRenderer[] litFlowerRenderers; // Array for all LitFlowerB SpriteRenderers
    private bool playerCollidingWithBackground = false; // Player collision status

    private void Start()
    {
        // Cache all background SpriteRenderers based on the background tag
        GameObject[] backgroundObjects = GameObject.FindGameObjectsWithTag(backgroundTag);
        backgroundRenderers = new SpriteRenderer[backgroundObjects.Length];

        for (int i = 0; i < backgroundObjects.Length; i++)
        {
            backgroundRenderers[i] = backgroundObjects[i].GetComponent<SpriteRenderer>();
            if (backgroundRenderers[i] != null)
            {
                Debug.Log("Found background object: " + backgroundObjects[i].name + " with color: " + backgroundRenderers[i].color);
            }
            else
            {
                Debug.LogError("No SpriteRenderer found on background object: " + backgroundObjects[i].name);
            }
        }

        // Cache all LitFlowerB SpriteRenderers based on the litFlowerBTag
        GameObject[] litFlowerBObjects = GameObject.FindGameObjectsWithTag(litFlowerBTag);
        litFlowerRenderers = new SpriteRenderer[litFlowerBObjects.Length];

        for (int i = 0; i < litFlowerBObjects.Length; i++)
        {
            litFlowerRenderers[i] = litFlowerBObjects[i].GetComponent<SpriteRenderer>();
            if (litFlowerRenderers[i] != null)
            {
                Debug.Log("Found LitFlowerB object: " + litFlowerBObjects[i].name + " with color: " + litFlowerRenderers[i].color);
            }
            else
            {
                Debug.LogError("No SpriteRenderer found on LitFlowerB object: " + litFlowerBObjects[i].name);
            }
        }
    }

    private void Update()
    {
        // Check if any LitFlowerB is enabled and player is colliding with a background object
        bool isLitFlowerBEnabled = false;
        foreach (var litFlowerRenderer in litFlowerRenderers)
        {
            if (litFlowerRenderer.enabled)
            {
                isLitFlowerBEnabled = true;
                break; // No need to check further once we find an enabled LitFlowerB
            }
        }

        // Log if player is colliding and LitFlowerB's state
        Debug.Log("Player colliding with background: " + playerCollidingWithBackground);
        Debug.Log("Any LitFlowerB enabled: " + isLitFlowerBEnabled);

        // Only adjust the background colors if any LitFlowerB is enabled and player is colliding
        if (isLitFlowerBEnabled && playerCollidingWithBackground)
        {
            Debug.Log("Adjusting background colors...");
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
