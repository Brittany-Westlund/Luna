using UnityEngine;
using MoreMountains.Tools;  // Assuming MMProgressBar is from MoreMountains

public class LightmoteCollision : MonoBehaviour
{
    public float lightCost = 0.1f;               // Light cost to activate the lantern
    public MMProgressBar lightBar;               // Reference to the Lightmote's light bar, if it has one

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            // Toggle the lantern's visual state, assuming it has a SpriteRenderer
            SpriteRenderer lanternSprite = other.GetComponent<SpriteRenderer>();
            if (lanternSprite != null)
            {
                bool isCurrentlyLit = lanternSprite.enabled;
                
                // Check if enough light is available to toggle on
                if (!isCurrentlyLit && lightBar.BarProgress >= lightCost)
                {
                    lanternSprite.enabled = true;
                    lightBar.SetBar01(lightBar.BarProgress - lightCost); // Reduce light bar by the cost
                    Debug.Log("Lantern activated by Lightmote.");
                }
                else if (isCurrentlyLit)
                {
                    lanternSprite.enabled = false;
                    lightBar.SetBar01(lightBar.BarProgress + lightCost); // Refill light bar by the cost
                    Debug.Log("Lantern deactivated by Lightmote.");
                }
            }
            else
            {
                Debug.LogWarning("No SpriteRenderer found on Lantern object.");
            }

            // Optional: Toggle any additional effects, like enabling/disabling other GameObjects
            GameObject targetObject = other.transform.Find("TargetObjectName")?.gameObject;
            if (targetObject != null)
            {
                targetObject.SetActive(!targetObject.activeSelf); // Toggle its active state
            }
        }
    }
}
