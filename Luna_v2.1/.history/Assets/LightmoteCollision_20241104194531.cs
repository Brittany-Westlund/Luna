using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private LightMoteManager lightMoteManager;

    private void Start()
    {
        lightMoteManager = GetComponent<LightMoteManager>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with a Lantern: " + other.gameObject.name);

            // Look for a LitLantern child on the Lantern object
            Transform litLantern = other.transform.Find("LitLantern");
            if (litLantern != null && litLantern.CompareTag("Lantern"))
            {
                SpriteRenderer spriteRenderer = litLantern.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    Debug.Log("LitLantern SpriteRenderer activated.");

                    // Release the LightMote after lighting the Lantern
                    lightMoteManager.ReleaseLightMote();
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
