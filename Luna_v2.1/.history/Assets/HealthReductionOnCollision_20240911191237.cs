using UnityEngine;

// Attach this script to the flower or hazard object
public class HealthReductionOnTrigger2D : MonoBehaviour
{
    // Tag your player as "Player" or adjust the tag name accordingly
    public string playerTag = "Player";

    // Flag to ensure the health reduction happens only once
    private bool hasReducedHealth = false;

    // Ensure the object has a trigger collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger Entered with: " + collision.gameObject.name); // Debug line to confirm trigger detection
        
        // Check if the triggering object is the player and if the health hasn't been reduced yet
        if (collision.CompareTag(playerTag) && !hasReducedHealth)
        {
            // Attempt to get the PlayerHealth component from the triggering object
            PlayerHealth playerHealth = collision.GetComponent<PlayerHealth>();

            if (playerHealth != null)
            {
                // Reduce the player's health by half
                playerHealth.currentHealth /= 2;

                // Set the flag to true to prevent further health reduction
                hasReducedHealth = true;

                // Optional: Add any feedback like sound or visual effects here
                Debug.Log("Player health reduced by half!");
            }
            else
            {
                Debug.LogError("PlayerHealth component not found on the triggering player object.");
            }
        }
    }

    // Optional: Reset health reduction (e.g., if player leaves the trigger area and re-enters)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag(playerTag))
        {
            // Reset flag when player exits, if desired
            hasReducedHealth = false;
        }
    }
}
