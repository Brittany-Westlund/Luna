using UnityEngine;


public class WildSporeInteraction : MonoBehaviour
{
    public LayerMask sporeLayer; // Assign this in the Inspector
    private bool isSporeNearby = false; // Tracks if a spore is near
    private GameObject nearbySpore; // Reference to the nearby spore
    private SpriteRenderer sproutRenderer;
    public float growthIncrement = 0.1f;
    public float yPositionIncrement = 0.05f;
    public float maxScale = 0.2215f; // Maximum scale allowed for the sprout
    public float maxHeight = -1.098f; // Maximum height the sprout can move to
    public float lastSporeTime = -30f; // Track the last spore interaction time
    public float sporeCooldown = 30f; // Cooldown in seconds


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (((1 << other.gameObject.layer) & sporeLayer) != 0) // Check if in SporeLayer
        {
            Debug.Log("Wild spore collided with a sprout!");

            AidingSprouts sproutScript = other.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function
                Destroy(gameObject); // Destroy wild spore after interaction
            }
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Spore")) // Detect spore leaving range
        {
            Debug.Log("Spore left the range.");
            isSporeNearby = false;
            nearbySpore = null;
        }
    }

    public void Grow()
    {
        Debug.Log("Sprout is growing!");

        // Ensure the sprout renderer is enabled
        if (!sproutRenderer.enabled)
        {
            sproutRenderer.enabled = true;
        }

        // Scale logic
        if (transform.localScale.x + growthIncrement <= maxScale &&
            transform.localScale.y + growthIncrement <= maxScale)
        {
            transform.localScale += new Vector3(growthIncrement, growthIncrement, 0);
        }
        else
        {
            transform.localScale = new Vector3(maxScale, maxScale, 1);
        }

        // Position adjustment logic with a height limit
        if (transform.position.y + yPositionIncrement <= maxHeight)
        {
            transform.position += new Vector3(0, yPositionIncrement, 0);
        }
        else
        {
            transform.position = new Vector3(transform.position.x, maxHeight, transform.position.z);
        }

        // Update the last spore time to track cooldown
        lastSporeTime = Time.time;
    }

}

