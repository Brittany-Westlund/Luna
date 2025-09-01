using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            // Check if LightNet has a child called LightMote
            Transform lightMote = transform.Find("LightMote");

            if (lightMote != null)
            {
                Debug.Log("LightMote is present as a child of LightNet.");

                // Find the LitLantern child on the collided Lantern object
                Transform litLantern = other.transform.Find("LitLantern");

                // Verify LitLantern is present and tagged as Lantern
                if (litLantern != null && litLantern.CompareTag("Lantern"))
                {
                    // Activate the SpriteRenderer on LitLantern
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
                    Debug.LogWarning("LitLantern not found or incorrectly tagged on the Lantern object.");
                }
            }
            else
            {
                Debug.Log("LightMote is not a child of LightNet. No action taken.");
            }
        }
    }
}
