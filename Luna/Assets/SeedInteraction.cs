using UnityEngine;

public class SeedInteraction : MonoBehaviour
{
    public GameObject pinkSeed; // Assign the pink seed GameObject
    public SpriteRenderer sproutRenderer; // Assign the sprout's Sprite Renderer
    public Collider2D lunaCollider; // Assign a collider component that detects Luna's presence
    public SpriteRenderer dryGroundRenderer; // Assign the Sprite Renderer for the dry ground
    public float sproutPositionXOffset = 0f; // Offset for the sprout's x position
    public float sproutPositionYOffset = 0f; // Offset for the sprout's y position
    public DryingOut dryingOutScript; // Reference to the drying out script

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Check if the object colliding is the pink seed, if Luna is close enough, and if DryGround's renderer is not enabled
        if (collision.gameObject == pinkSeed && lunaCollider.isActiveAndEnabled && !dryGroundRenderer.enabled)
        {
            // Calculate the position of the sprout
            Vector3 sproutPosition = transform.position;
            sproutPosition.x += sproutPositionXOffset;
            sproutPosition.y += sproutPositionYOffset;
            sproutRenderer.transform.position = sproutPosition;

            // Enable the sprout's renderer
            sproutRenderer.enabled = true;

            // Deactivate the pink seed, making it disappear
            pinkSeed.SetActive(false);

            // Start the drying out process
            if (dryingOutScript != null)
            {
                dryingOutScript.enabled = true; // Enable the drying out script
            }
        }
    }
}
