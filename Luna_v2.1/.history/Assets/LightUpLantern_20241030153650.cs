using UnityEngine;
using MoreMountains.Tools; // Assuming LightBar uses this namespace

public class LanternLight : MonoBehaviour
{
    public SpriteRenderer LitLanternSprite;    // The lit version of the lantern sprite
    public KeyCode activationKey = KeyCode.L;  // Public field to set the activation key
    public MMProgressBar lightBar;             // Public field to assign the light bar in the Inspector
    private float lightCost = 0.1f;            // Light cost to light up the lantern
    private bool isLit = false;                // Track whether the lantern is currently lit
    private bool playerInRange = false;        // Track if the player is in range for activation

    void Update()
    {
        // Check for player input to toggle lantern light
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;  // Get the current progress (0 to 1 scale)

            if (!isLit && currentProgress >= lightCost)
            {
                // Light up the lantern
                lightBar.SetBar01(currentProgress - lightCost);  // Reduce progress by lightCost
                if (LitLanternSprite != null)
                {
                    LitLanternSprite.enabled = true;  // Enable the lit sprite
                }
                isLit = true;
            }
            else if (isLit)
            {
                // Retrieve light from the lantern
                lightBar.SetBar01(currentProgress + lightCost);  // Increase progress by lightCost
                if (LitLanternSprite != null)
                {
                    LitLanternSprite.enabled = false;  // Disable the lit sprite
                }
                isLit = false;
            }
        }
    }

    // Detect when the player enters the trigger area
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) // Ensure the player has the tag "Player"
        {
            playerInRange = true;
        }
    }

    // Detect when the player exits the trigger area
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
