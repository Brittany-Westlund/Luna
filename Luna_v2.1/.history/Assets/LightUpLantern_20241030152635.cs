using UnityEngine;
using MoreMountains.Tools; // Assuming LightBar uses this namespace

public class LanternLight : MonoBehaviour
{
    public SpriteRenderer LitLanternSprite;   // The lit version of the lantern sprite
    public Transform PlayerTransform;         // Reference to the player's transform
    public float ActivationRange = 3f;        // Range within which the lantern can be activated
    public KeyCode activationKey = KeyCode.L; // Public field to set the activation key
    public MMProgressBar lightBar;            // Public field to assign the light bar in the Inspector
    private float lightCost = 0.1f;           // Light cost to light up the lantern
    private bool isLit = false;               // Track whether the lantern is currently lit

    void Update()
    {
        // Check for player input to toggle lantern light
        if (Input.GetKeyDown(activationKey) && IsPlayerInRange())
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

    // Check if the player is within the activation range
    private bool IsPlayerInRange()
    {
        float squaredDistanceToPlayer = (PlayerTransform.position - transform.position).sqrMagnitude;
        return squaredDistanceToPlayer <= ActivationRange * ActivationRange;
    }
}
