using UnityEngine;

public class FadingSprout : MonoBehaviour
{
    public float fadeSpeed = 1f; // Speed of the fading
    public AidingSprouts wateringScript; // Reference to the Watering script
    public float maxScale = 0.2215f; // Maximum scale of the sprout

    private SpriteRenderer sproutRenderer; // Sprite Renderer of the sprout

    void Start()
    {
        sproutRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Check if the sprout has reached its maximum scale
        if (transform.localScale.x >= maxScale)
        {
            // If it has, ensure the sprout is fully visible
            sproutRenderer.color = new Color(sproutRenderer.color.r, sproutRenderer.color.g, sproutRenderer.color.b, 1);
            return; // Exit the method early to skip fading logic
        }

        // Check if the cooldown has finished and it's time to fade
        if (wateringScript != null && Time.time >= wateringScript.lastSporeTime + wateringScript.waterCooldown)
        {
            // Calculate new alpha using a cosine wave for smooth fade in and fade out
            float newAlpha = (Mathf.Cos(Time.time * fadeSpeed) + 1f) * 0.5f; // Normalized to 0-1
            sproutRenderer.color = new Color(sproutRenderer.color.r, sproutRenderer.color.g, sproutRenderer.color.b, newAlpha);
        }
        else
        {
            // If not fading, ensure the sprout is fully visible
            sproutRenderer.color = new Color(sproutRenderer.color.r, sproutRenderer.color.g, sproutRenderer.color.b, 1);
        }
    }
}
