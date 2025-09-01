using UnityEngine;

public class ChangeColorOnBloomForLitFlowerB : MonoBehaviour
{
    [Header("Settings")]
    public float colorIncreaseStep = 0.1f; // Step for color change towards white
    public Color targetColor = Color.white; // Target color (white by default)
    public string backgroundTag = "Background"; // Tag to identify background objects

    private SpriteRenderer[] backgroundRenderers; // Array to hold all background SpriteRenderers
    private SpriteRenderer litFlowerRenderer; // Reference to LitFlowerB's SpriteRenderer

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

        // Log an error if any background objects are missing their SpriteRenderer
        for (int i = 0; i < backgroundRenderers.Length; i++)
        {
            if (backgroundRenderers[i] == null)
            {
                Debug.LogError("SpriteRenderer is missing on a background object tagged as " + backgroundTag);
            }
        }
    }

    private void Update()
    {
        // Only adjust the background colors if LitFlowerB's renderer is enabled
        if (litFlowerRenderer != null && litFlowerRenderer.enabled)
        {
            AdjustBackgroundColors();
        }
    }

    // Gradually adjust all background colors towards white
    private void AdjustBackgroundColors()
    {
        foreach (SpriteRenderer renderer in backgroundRenderers)
        {
            if (renderer != null)
            {
                renderer.color = Color.Lerp(renderer.color, targetColor, colorIncreaseStep);
                Debug.Log("Background color adjusted: " + renderer.color);
            }
        }
    }
}
