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

    private void Start()
    {
        sproutRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for spore via layer or tag
        if (((1 << other.gameObject.layer) & sporeLayer) != 0 || other.CompareTag("Spore"))
        {
            Debug.Log("Spore entered range!");

            // Track the spore
            isSporeNearby = true;
            nearbySpore = other.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Check for spore via layer or tag
        if (((1 << other.gameObject.layer) & sporeLayer) != 0 || other.CompareTag("Spore"))
        {
            Debug.Log("Spore left the range.");

            // Clear the spore reference
            isSporeNearby = false;
            nearbySpore = null;
        }
    }

    private void Update()
    {
        // Check if the player presses A and a spore is nearby
        if (isSporeNearby && Input.GetKeyDown(KeyCode.A))
        {
            Debug.Log("Player pressed 'A' to assist!");

            // Check cooldown
            if (Time.time - lastSporeTime >= sporeCooldown)
            {
                Grow(); // Trigger sprout growth
                Destroy(nearbySpore); // Remove the spore after assisting
                isSporeNearby = false; // Reset spore detection
            }
            else
            {
                Debug.Log("Cooldown not met yet.");
            }
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
