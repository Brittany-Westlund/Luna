using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public LayerMask sporeLayer; // Assign this in the Inspector for spore detection
    public LayerMask netLayer; // Assign this in the Inspector for net detection
    public Transform sporeHoldPoint; // Reference for holding spores
    public Transform pickupHoldPoint; // Reference for holding general pickupables

    public float detectionRadius = 0.8f; // Adjustable detection radius for pickups

    private GameObject _player;
    private GameObject currentHeldObject; // Reference to the currently held object

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player not found! Ensure Player has the correct tag.");
        }

        if (sporeHoldPoint == null || pickupHoldPoint == null)
        {
            Debug.LogError("Hold points have not been assigned in the Inspector!");
        }
    }

    private void Update()
    {
        if (_player == null) return;

        // Handle picking up objects with the Player1_Grab input
        if (Input.GetButtonDown("Player1_Grab")) // Grab/Drops objects with "Player1_Grab"
        {
            HandlePickupOrDrop();
        }

        // Handle aiding sprouts with the Aid input
        if (Input.GetButtonDown("Aid") && currentHeldObject != null && currentHeldObject.CompareTag("Spore")) // Assists with "Aid"
        {
            AidSprout();
        }
    }

    private void HandlePickupOrDrop()
    {
        if (currentHeldObject == null) // Not holding anything, try to pick up
        {
            // Check for objects in the detection radius
            Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
            if (nearbyCollider != null)
            {
                // Debugging the detected object
                Debug.Log($"Detected object: {nearbyCollider.name}, Tag: {nearbyCollider.tag}, Layer: {LayerMask.LayerToName(nearbyCollider.gameObject.layer)}");

                // Get the root object to ensure we pick up the parent (e.g., Net)
                GameObject rootObject = nearbyCollider.transform.root.gameObject;

                // Check if the object is interactable
                if (rootObject.CompareTag("Pickupable") || rootObject.CompareTag("Spore"))
                {
                    Pickup(rootObject);
                }
                else
                {
                    Debug.Log($"Nearby object '{rootObject.name}' is not interactable.");
                }
            }
            else
            {
                Debug.Log("No nearby object to pick up.");
            }
        }
        else // Already holding something, drop it
        {
            Drop();
        }
    }

    private void Pickup(GameObject targetObject)
    {
        if (currentHeldObject != null)
        {
            Debug.LogWarning("Luna is already holding an object. Cannot pick up another!");
            return; // Prevent picking up multiple objects
        }

        currentHeldObject = targetObject;

        if (targetObject.CompareTag("Spore"))
        {
            // Use the spore hold point
            currentHeldObject.transform.SetParent(sporeHoldPoint);
        }
        else // Handle general pickupables
        {
            // Use the general pickup hold point
            currentHeldObject.transform.SetParent(pickupHoldPoint);
        }

        currentHeldObject.transform.localPosition = Vector3.zero;
        currentHeldObject.GetComponent<Rigidbody2D>().isKinematic = true; // Disable physics while held
        Debug.Log($"Picked up object: {currentHeldObject.name}");
    }

    private void Drop()
    {
        if (currentHeldObject != null)
        {
            // Detach the object
            currentHeldObject.transform.SetParent(null);

            // Reset its position slightly in front of Luna
            currentHeldObject.transform.position = new Vector3(
                _player.transform.position.x + 0.5f * _player.transform.localScale.x, // Offset based on facing direction
                _player.transform.position.y,
                currentHeldObject.transform.position.z
            );

            // Re-enable physics if applicable
            Rigidbody2D rb = currentHeldObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false; // Allow physics interactions
            }

            Debug.Log($"Dropped object: {currentHeldObject.name}");
            currentHeldObject = null; // Clear the reference
        }
    }

    private void AidSprout()
    {
        // Check for nearby sprouts using LayerMask
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, sporeLayer);
        if (sproutCollider != null)
        {
            Debug.Log("Spore is near a sprout, and aid key was pressed!");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function on the sprout
                Destroy(currentHeldObject); // Destroy the spore after assisting
                currentHeldObject = null; // Clear the reference
            }
        }
        else
        {
            Debug.Log("No sprout nearby to assist.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the OverlapCircle in the Scene view for debugging
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
