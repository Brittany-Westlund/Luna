using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is tagged "Lantern"
        if (other.CompareTag("Lantern"))
        {
            // Check if LightNet currently has a child named LightMote
            Transform lightMote = transform.Find("LightMote");

            // If LightMote exists as a child of LightNet, proceed
            if (lightMote != null)
            {
                // Look for a child called LitLantern on the collided Lantern object
                Transform litLantern = other.transform.Find("LitLantern");

                // Ensure LitLantern exists and is tagged as "Lantern"
                if (litLantern != null && litLantern.CompareTag("Lantern"))
                {
                    // Enable the SpriteRenderer on LitLantern
                    SpriteRenderer spriteRenderer = litLantern.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null)
                    {
                        spriteRenderer.enabled = true;
                        Debug.Log("LitLantern SpriteRenderer activated.");
                    }
                    else
                    {
                        Debug.LogWarning("No SpriteRenderer found on LitLantern.");
                    }
                }
                else
                {
                    Debug.LogWarning("LitLantern child not found or incorrectly tagged on the Lantern object.");
                }
            }
            else
            {
                Debug.Log("LightMote is not a child of LightNet. Collision ignored.");
            }
        }
    }
}
