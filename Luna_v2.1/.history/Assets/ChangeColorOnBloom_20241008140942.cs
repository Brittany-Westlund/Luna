using UnityEngine;

public class ChangeAlphaOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float alphaIncreaseStep = 0.2f; // Increase alpha by 20% each time
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
                Debug.Log("Found background object: " + backgroundObjects[i].name + " with alpha: " + backgroundRenderers[i].color.a);
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
                Debug.Log("Found LitFlowerB object: " + litFlowerBObjects[i].name + " with alpha: " + litFlowerRenderers[i].color.a);
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

        // Only adjust the background alpha if any LitFlowerB is enabled and player is colliding
        if (isLitFlowerBEnabled && playerCollidingWithBackground)
        {
            Debug.Log("Adjusting background alpha...");
            AdjustBackgroundAlpha();
        }
    }

    // Adjust the alpha of the background objects by 20%
    private void AdjustBackgroundAlpha()
    {
        foreach (SpriteRenderer renderer in backgroundRenderers)
        {
            if (renderer != null)
            {
                // Increase alpha by 20%, ensuring it doesn't exceed 1 (fully opaque)
                float newAlpha = Mathf.Min(renderer.color.a + alphaIncreaseStep, 1f);

                // Apply the new color with updated alpha
                renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, newAlpha);
                Debug.Log("Background alpha adjusted to: " + renderer.color.a);
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
