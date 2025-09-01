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
        // Check if LightNet has more than one child
        if (lightNet.transform.childCount > 1)
        {
            // Destroy this LightMote if there's already one attached
            Destroy(gameObject);
            Debug.Log("LightNet can only have one child LightMote. Extra LightMote destroyed.");
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
            }
            else
            {
                Debug.LogWarning("No SpriteRenderer found on the Lantern.");
            }
        }
    }
}
