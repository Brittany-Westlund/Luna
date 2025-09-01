using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if LightNet already has a LightMote
        Transform lightMote = transform.Find("LightMote");

        if (lightMote == null)
        {
            Debug.Log("No LightMote attached to LightNet. Skipping Lantern lighting.");
            return; // Exit if there is no LightMote
        }

        // Proceed only if the collided object is tagged "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with a Lantern: " + other.gameObject.name);

            // Look for a child called LitLantern on the collided Lantern object
            Transform litLantern = other.transform.Find("LitLantern");

            // Ensure LitLantern exists and is tagged as "Lantern"
            if (litLantern != null && litLantern.CompareTag("Lantern"))
            {
                Debug.Log("LitLantern child found and verified on the Lantern object.");

                // Enable the SpriteRenderer on LitLantern
                SpriteRenderer spriteRenderer = litLantern.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    Debug.Log("SpriteRenderer on LitLantern is now enabled.");

                    // Destroy the LightMote after lighting up the lantern
                    Destroy(lightMote.gameObject);
                    Debug.Log("LightMote destroyed after lighting up the Lantern.");
                }
                else
                {
                    Debug.LogWarning("SpriteRenderer not found on LitLantern.");
                }
            }
            else
            {
                Debug.LogWarning("No LitLantern child found or it is not tagged correctly.");
            }
        }
        else
        {
            Debug.Log("Collided object is not tagged as 'Lantern'. Ignored.");
        }
    }
}
