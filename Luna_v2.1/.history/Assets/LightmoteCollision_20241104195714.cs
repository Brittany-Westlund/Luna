using UnityEngine;

public class LightmoteCollision : MonoBehaviour
{
    public GameObject lightNet;  // Reference to the LightNet GameObject

    private void Start()
    {
        // Ensure the LightNet reference is assigned in the Inspector
        if (lightNet == null)
        {
            Debug.LogError("LightNet reference is missing. Please assign LightNet in the Inspector.");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collided object is tagged "Lantern"
        if (other.CompareTag("Lantern"))
        {
            Debug.Log("Collision detected with a Lantern: " + other.gameObject.name);

            // Activate the Lantern's SpriteRenderer, if available
            SpriteRenderer spriteRenderer = other.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.enabled = true;
                Debug.Log("Lantern SpriteRenderer activated.");
            }
            else
            {
                Debug.LogWarning("No SpriteRenderer found on the Lantern.");
            }

            // Destroy this specific LightMote after lighting the Lantern
            Destroy(gameObject);
            Debug.Log("LightMote destroyed after lighting the Lantern.");
        }
    }

    public void AttachToLightNet()
    {
        // Only attach this LightMote if LightNet has no other children tagged "LightMote"
        if (!HasLightMoteAttached())
        {
            // Set this LightMote as a child of LightNet
            transform.SetParent(lightNet.transform);
            transform.localPosition = Vector3.zero;
            Debug.Log("LightMote attached to LightNet.");
        }
        else
        {
            Debug.Log("LightNet already has a LightMote attached; ignoring this LightMote.");
        }
    }

    private bool HasLightMoteAttached()
    {
        // Check if there is any child of LightNet tagged as "LightMote"
        foreach (Transform child in lightNet.transform)
        {
            if (child.CompareTag("LightMote"))
            {
                return true; // A LightMote is already attached
            }
        }
        return false; // No LightMote attached
    }
}
