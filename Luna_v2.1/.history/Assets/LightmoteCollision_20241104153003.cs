using UnityEngine;
using MoreMountains.Tools;  // Assuming MMProgressBar is from MoreMountains

public class LightmoteCollision : MonoBehaviour
{
    public float lightCost = 0.1f;                 // Light cost to activate the lantern
    public MMProgressBar lightBar;                 // Reference to the Lightmote's light bar
    public GameObject litLantern;                  // Reference to the lit version of the lantern

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            // Check if the Lightmote has enough light to activate the lantern
            if (lightBar != null && lightBar.BarProgress >= lightCost)
            {
                // Toggle the lit version of the lantern
                if (litLantern != null)
                {
                    bool isCurrentlyLit = litLantern.activeSelf;
                    
                    if (!isCurrentlyLit)
                    {
                        // Turn on the lit version of the lantern and reduce the Lightmote's light bar
                        litLantern.SetActive(true);
                        lightBar.SetBar01(lightBar.BarProgress - lightCost);
                        Debug.Log("Lantern activated by Lightmote.");
                    }
                    else
                    {
                        // Turn off the lit version of the lantern and replenish the Lightmote's light bar
                        litLantern.SetActive(false);
                        lightBar.SetBar01(lightBar.BarProgress + lightCost);
                        Debug.Log("Lantern deactivated by Lightmote.");
                    }
                }
                else
                {
                    Debug.LogWarning("No litLantern GameObject assigned in LightmoteCollision.");
                }
            }
            else
            {
                Debug.LogWarning("Insufficient light to activate the lantern.");
            }
        }
    }
}
