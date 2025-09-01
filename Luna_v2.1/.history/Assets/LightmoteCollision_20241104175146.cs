using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ensure that the collision is with a Lantern object
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with a Lantern: " + other.gameObject.name);

            // Find the LitLantern child object on the Lantern
            Transform litLantern = other.transform.Find("LitLantern");
            if (litLantern != null && litLantern.CompareTag("Lantern"))
            {
                // Enable the SpriteRenderer on LitLantern
                SpriteRenderer spriteRenderer = litLantern.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    Debug.Log("LitLantern SpriteRenderer activated.");

                    // Destroy the LightMote after successful activation
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
                Debug.LogWarning("No LitLantern child found or incorrectly tagged.");
            }
        }
    }
}
