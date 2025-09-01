using UnityEngine;

public class ChangeColorOnTrigger : MonoBehaviour
{
    // Public color field to set in the Unity Editor
    public Color targetColor = new Color(1f, 0.553f, 0.867f); // Default to FF8DCF

    // Reference to the SpriteRenderer component
    private SpriteRenderer spriteRenderer;

    // Variable to store the original color
    private Color originalColor;

    private void Start()
    {
        // Get the SpriteRenderer component attached to the GameObject
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Store the original color of the sprite
        if (spriteRenderer != null)
        {
            originalColor = spriteRenderer.color;
        }
    }

    // This is called when another object enters the trigger collider
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Change the sprite color when the trigger is entered
        if (spriteRenderer != null)
        {
            spriteRenderer.color = targetColor;
        }
    }

    // This is called when another object exits the trigger collider
    private void OnTriggerExit2D(Collider2D other)
    {
        // Revert the sprite color back to the original color
        if (spriteRenderer != null)
        {
            spriteRenderer.color = originalColor;
        }
    }
}
