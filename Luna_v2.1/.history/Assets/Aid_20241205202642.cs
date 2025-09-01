using UnityEngine;
using System.Collections;

public class Aid : MonoBehaviour
{
    public float aidDistance = 3f;      // Distance the spore moves when aiding
    public float moveSpeed = 5f;       // Speed of the spore's movement
    public float detectionDelay = 0.1f; // Delay to allow spore to move before checking collision

    private PresentSpore presentSpore; // Reference to the PresentSpore script
    private Transform sporeHoldPoint; // Reference to the spore's attach point

    private GameObject currentSpore;  // Tracks the active spore

    void Start()
    {
        presentSpore = GetComponent<PresentSpore>();
        sporeHoldPoint = presentSpore.attachPoint; // Reference to the spore's parent point
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A) && presentSpore.HasSporeAttached && !presentSpore.IsSliding)
        {
            GameObject attachedSpore = presentSpore.GetAttachedSpore();
            if (attachedSpore != null)
            {
                currentSpore = attachedSpore;

                // Determine the direction based on Luna's sprite renderer flip state
                SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
                Vector3 direction = spriteRenderer.flipX ? Vector3.left : Vector3.right;

                Debug.Log($"Aid triggered. Moving spore in direction: {direction}");

                // Start moving the spore and handle interaction
                StartCoroutine(SlideSporeForward(attachedSpore, direction));
            }
        }
    }

    private IEnumerator SlideSporeForward(GameObject spore, Vector3 direction)
    {
        if (spore == null)
        {
            Debug.LogError("No spore found to move!");
            yield break;
        }

        // Calculate the target position
        Vector3 startPosition = spore.transform.position;
        Vector3 targetPosition = startPosition + direction * aidDistance;

        Debug.Log($"Spore starting at {startPosition}, moving to {targetPosition}");

        // Detach from the parent to move independently
        spore.transform.SetParent(null);

        // Move the spore toward the target position
        while (Vector3.Distance(spore.transform.position, targetPosition) > 0.01f)
        {
            spore.transform.position = Vector3.MoveTowards(
                spore.transform.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );
            yield return null; // Wait until the next frame
        }

        Debug.Log("Spore reached the target.");

        // Allow collision detection to process
        yield return new WaitForSeconds(detectionDelay);

        if (spore != null)
        {
            Debug.Log("Spore interaction completed.");
            Destroy(spore); // Clean up the spore
            presentSpore.ResetSporeState(); // Reset PresentSpore
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the moving spore interacts with a valid NPC
        if (currentSpore != null && other.gameObject == currentSpore)
        {
            Debug.Log($"Spore interacted with: {other.name}, Tag: {other.tag}");

            if (other.CompareTag("Butterfly"))
            {
                AidButterfly(other.gameObject);
            }
            else if (other.CompareTag("Flower"))
            {
                AidFlower();
            }
        }
    }

    private void AidFlower()
    {
        Debug.Log("Aided a flower!");
        // Add specific behavior for flowers, e.g., bloom animation
    }

    private void AidButterfly(GameObject butterfly)
    {
        Debug.Log("Aided a butterfly!");

        // Find the ButterflyFlyHandler script on the butterfly GameObject
        ButterflyFlyHandler butterflyFlyHandler = butterfly.GetComponent<ButterflyFlyHandler>();
        if (butterflyFlyHandler != null)
        {
            Debug.Log("ButterflyFlyHandler found. Triggering aid.");
            butterflyFlyHandler.AidButterfly();
        }
        else
        {
            Debug.LogError("ButterflyFlyHandler not found on the detected butterfly!");
        }
    }
}
