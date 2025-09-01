using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("OnTriggerEnter2D triggered with object: " + other.gameObject.name);

        // Check if the collided object has the tag "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision with object tagged 'Lantern' detected: " + other.gameObject.name);

            // Check if LightNet currently has a child named LightMote
            Transform lightMote = transform.Find("LightMote");

            if (lightMote != null)
            {
                Debug.Log("LightMote is confirmed as a child of LightNet.");

                // Try to find the LitLantern child object on the collided Lantern
                Transform litLantern = other.transform.Find("LitLantern");

                // Verify that LitLantern exists and is tagged as "Lantern"
                if (litLantern != null)
                {
                    Debug.Log("LitLantern child found on Lantern object: " + litLantern.name);

                    if (litLantern.CompareTag("Lantern"))
                    {
                        Debug.Log("LitLantern is correctly tagged as 'Lantern'.");

                        // Attempt to activate the SpriteRenderer on LitLantern
                        SpriteRenderer spriteRenderer = litLantern.GetComponent<SpriteRenderer>();
                        if (spriteRenderer != null)
                        {
                            spriteRenderer.enabled = true;
                            Debug.Log("SpriteRenderer on LitLantern is now enabled.");
                        }
                        else
                        {
                            Debug.LogWarning("LitLantern does not have a SpriteRenderer component.");
                        }
                    }
                    else
                    {
                        Debug.LogWarning("LitLantern is not tagged as 'Lantern'. Check the tag.");
                    }
                }
                else
                {
                    Debug.LogWarning("No child named 'LitLantern' found on the collided Lantern.");
                }
            }
            else
            {
                Debug.Log("LightMote is not a child of LightNet. No action taken.");
            }
        }
        else
        {
            Debug.Log("Collided object is not tagged as 'Lantern'. Ignored.");
        }
    }
}
