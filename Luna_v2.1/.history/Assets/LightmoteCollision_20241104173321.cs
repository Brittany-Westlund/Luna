using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure only one LightMote exists as a child of LightNet
        if (transform.Find("LightMote") != null)
        {
            Debug.Log("LightNet already has a LightMote. Cannot add another.");
            return; // Exit if LightMote is already present
        }

        // Check if the collided object is tagged "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision with object tagged 'Lantern' detected.");

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

                    // Destroy the LightMote after activating the lantern
                    Transform lightMote = transform.Find("LightMote");
                    if (lightMote != null)
                    {
                        Destroy(lightMote.gameObject);
                        Debug.Log("LightMote destroyed after lighting up the Lantern.");
                    }
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
    }
}
