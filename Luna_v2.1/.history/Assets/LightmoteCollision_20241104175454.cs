using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    private LightNetCollector lightNetCollector;

    private void Start()
    {
        lightNetCollector = GetComponent<LightNetCollector>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with a Lantern: " + other.gameObject.name);

            // Check if LightNet currently has a LightMote to use
            if (transform.Find("LightMote") == null)
            {
                Debug.Log("No LightMote available to light the Lantern.");
                return;
            }

            Transform litLantern = other.transform.Find("LitLantern");
            if (litLantern != null && litLantern.CompareTag("Lantern"))
            {
                SpriteRenderer spriteRenderer = litLantern.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null)
                {
                    spriteRenderer.enabled = true;
                    Debug.Log("LitLantern SpriteRenderer activated.");

                    // Release LightMote after lighting the Lantern
                    lightNetCollector.ReleaseLightMote();
                }
                else
                {
                    Debug.LogWarning("No SpriteRenderer found on LitLantern.");
                }
            }
            else
            {
                Debug.LogWarning("No LitLantern child found or it is not tagged correctly.");
            }
        }
    }
}
