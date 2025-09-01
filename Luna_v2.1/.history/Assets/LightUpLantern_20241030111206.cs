using UnityEngine;
using MoreMountains.Tools;  // Import the namespace for MMProgressBar

public class LanternLight : MonoBehaviour
{
    public SpriteRenderer LitLanternSprite;   // The lit version of the lantern sprite
    public Transform PlayerTransform;         // Reference to the player's transform
    public float ActivationRange = 3f;        // Range within which the lantern can be activated
    public MMProgressBar lightBar;            // Reference to the MMProgressBar component
    private float lightCost = 0.1f;           // Light cost to light up the lantern
    private bool isLit = false;               // Track whether the lantern is currently lit

    void Update()
    {
        // Check for player input to toggle lantern light
        if (Input.GetButtonDown("Player1_LightActivation") && IsPlayerInRange())
        {
            if (!isLit && lightBar.BarProgress >= lightCost)
            {
                // Light up the lantern
                lightBar.UpdateBar01(lightBar.BarProgress - lightCost);
                LitLanternSprite.enabled = true;
                isLit = true;
            }
            else if (isLit)
            {
                // Retrieve light from the lantern
                lightBar.UpdateBar01(lightBar.BarProgress + lightCost);
                LitLanternSprite.enabled = false;
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
