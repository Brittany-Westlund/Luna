using UnityEngine;

public class PickupObject : MonoBehaviour
{
    public Transform player; // Assign your player character's transform here
    public Vector3 pickUpOffset = new Vector3(0, 1f, 0); // Offset the position of the object when picked up
    private Vector3 originalScale; // To store the original scale

    // New field to reference another GameObject's SpriteRenderer
    public SpriteRenderer objectToActivateRenderer;

    private SpriteRenderer spriteRenderer; // SpriteRenderer of this object

    private void Awake()
    {
        originalScale = transform.localScale; // Store the original scale
        spriteRenderer = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer of this object
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform == player)
        {
            transform.SetParent(player); // Make the object a child of the player
            transform.localPosition = pickUpOffset; // Adjust position relative to the player
            transform.localScale = originalScale; // Reset the scale to the original scale
            player.GetComponent<SetDownObject>().PickUpObject(this);
        }
    }

    public void SetDown()
    {
        transform.SetParent(null); // Detach the object from the player
        transform.localScale = originalScale; // Reset the scale to the original scale

        // Turn off this object's SpriteRenderer
        if (spriteRenderer != null)
        {
            spriteRenderer.enabled = false;
        }

        // Turn on the other object's SpriteRenderer
        if (objectToActivateRenderer != null)
        {
            objectToActivateRenderer.enabled = true;
        }
    }
}
