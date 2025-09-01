using UnityEngine;
using MoreMountains.Tools;  // Assuming MMProgressBar is from MoreMountains

public class LightmoteCollision : MonoBehaviour
{
    public float lightCost = 0.1f;                 // Light cost to activate the lantern
    public MMProgressBar lightBar;                 // Reference to the Lightmote's light bar
    public string litLanternChildName = "LitLantern"; // Name of the child GameObject for the lit version

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with Lantern.");

            // Check if the Lightmote has enough light to activate the lantern
            if (lightBar != null && lightBar.BarProgress >= lightCost)
            {
                // Search through all children to find the "lit" version of the lantern
                Transform litLantern = FindChildByName(other.transform, litLanternChildName);

                if (litLantern != null)
                {
                    bool isCurrentlyLit = litLantern.gameObject.activeSelf;
                    Debug.Log("Lit lantern child found. Current state: " + isCurrentlyLit);

                    if (!isCurrentlyLit)
                    {
                        // Activate the lit version and reduce the Lightmote's light bar
                        litLantern.gameObject.SetActive(true);
                        lightBar.SetBar01(lightBar.BarProgress - lightCost);
                        Debug.Log("Lantern activated by Lightmote.");
                    }
                    else
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
            }
            else
            {
                Debug.LogWarning("Insufficient light to activate the lantern.");
            }
        }
    }

    // Helper method to search all children recursively for a matching name
    private Transform FindChildByName(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
            {
                return child;
            }
            // Recursively search within each child
            Transform found = FindChildByName(child, childName);
            if (found != null) return found;
        }
        return null;
    }
}
