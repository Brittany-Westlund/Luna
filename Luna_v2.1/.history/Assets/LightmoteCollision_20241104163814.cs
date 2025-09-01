using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision with Lantern detected.");

            // Check if LightNet currently has a child named LightMote
            Transform lightMote = transform.Find("LightMote");

            if (lightMote != null)
            {
                Debug.Log("LightMote is attached to LightNet.");

                // Try to find the LitLantern child object on the collided Lantern
                Transform litLantern = other.transform.Find("LitLantern");

                // Verify that LitLantern exists and is tagged as "Lantern"
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
                        Debug.LogWarning("LitLantern does not have a SpriteRenderer component.");
                    }
                }
                else
                {
                    Debug.LogWarning("LitLantern child not found or incorrectly tagged on the Lantern object.");
                }
            }
            else
            {
                Debug.Log("LightMote is not attached to LightNet. Collision ignored.");
            }
        }
    }
}
