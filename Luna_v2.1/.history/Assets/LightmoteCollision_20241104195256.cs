using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    public GameObject lightNet;  // Reference to the LightNet GameObject

    private void Start()
    {
        // Ensure the LightNet reference is assigned
        if (lightNet == null)
        {
            Debug.LogError("LightNet reference is missing. Please assign LightNet in the Inspector.");
        }
    }

    private void Update()
    {
        // Check if LightNet already has a LightMote attached as a child
        if (lightNet.transform.childCount > 0 && transform.IsChildOf(lightNet.transform) == false)
        {
            // If LightNet already has a LightMote, ignore further attempts to collect this one
            Debug.Log("LightNet already has a LightMote. Ignoring additional LightMote.");
            return; // Do nothing if LightNet already has a LightMote
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is tagged "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with a Lantern: " + other.gameObject.name);

            // Look for a SpriteRenderer on the Lantern object
            SpriteRenderer spriteRenderer = other.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true; // Activate the Lantern's sprite
                Debug.Log("Lantern SpriteRenderer activated.");

                // Destroy this LightMote after activating the Lantern
                Destroy(gameObject);
                Debug.Log("LightMote destroyed after lighting the Lantern.");
            }
            else
            {
                Debug.LogWarning("No SpriteRenderer found on the Lantern.");
            }
        }
    }
}
