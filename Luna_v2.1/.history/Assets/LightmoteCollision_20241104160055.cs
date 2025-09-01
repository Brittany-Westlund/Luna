using UnityEngine;
using MoreMountains.Tools;  // Assuming MMProgressBar is from MoreMountains

public class LightmoteCollision : MonoBehaviour
{
    public float lightCost = 0.1f;                       // Light cost to activate the lantern
    public MMProgressBar lightBar;                       // Reference to the Lightmote's light bar
    public string litLanternChildName = "LitLantern";    // Name of the child GameObject for the lit version
    public float detectionRadius = 1.0f;                 // Radius to detect nearby lanterns
    public float checkInterval = 0.5f;                   // How often to check for nearby lanterns (in seconds)

    private float _nextCheckTime;                        // Time for the next detection check

    private void Update()
    {
        // Perform a check only at defined intervals
        if (Time.time >= _nextCheckTime)
        {
            DetectNearbyLantern();
            _nextCheckTime = Time.time + checkInterval;
        }
    }

    private void DetectNearbyLantern()
    {
        // Find all colliders within the detection radius
        Collider2D[] hitColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        foreach (var collider in hitColliders)
        {
            // Check if the collider belongs to a Lantern
            if (collider.CompareTag("Lantern"))
            {
                Debug.Log("Detected a Lantern nearby.");

                // Find the "lit" version of the lantern as a child GameObject
                Transform litLantern = collider.transform.Find(litLanternChildName);

                if (litLantern != null)
                {
                    bool isCurrentlyLit = litLantern.gameObject.activeSelf;

                    if (!isCurrentlyLit && lightBar.BarProgress >= lightCost)
                    {
                        // Activate the lit version and reduce the Lightmote's light bar
                        litLantern.gameObject.SetActive(true);
                        lightBar.SetBar01(lightBar.BarProgress - lightCost);
                        Debug.Log("Lantern activated by Lightmote.");
                    }
                    else if (isCurrentlyLit)
                    {
                        // Deactivate the lit version and replenish the Lightmote's light bar
                        litLantern.gameObject.SetActive(false);
                        lightBar.SetBar01(lightBar.BarProgress + lightCost);
                        Debug.Log("Lantern deactivated by Lightmote.");
                    }
                }
                else
                {
                    Debug.LogWarning("Lit lantern child not found on the Lantern object.");
                }

                // Stop further checks once a nearby lantern is found and processed
                break;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Draw the detection radius in the Scene view for visualization
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
