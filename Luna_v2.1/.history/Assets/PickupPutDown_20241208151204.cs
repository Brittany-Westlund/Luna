using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public LayerMask sporeLayer; // Assign this in the Inspector for spore detection
    public LayerMask netLayer; // Assign this in the Inspector for net detection
    public Transform sporeHoldPoint; // Reference for holding spores
    public Transform pickupHoldPoint; // Reference for holding general pickupables

    public float netDetectionRadius = 0.8f; // Adjustable detection radius for nets
    public float sporeDetectionRadius = 1.0f; // Adjustable detection radius for spores

    private GameObject _player;
    private GameObject currentHeldObject; // Reference to the currently held object
    private bool isHoldingSpore = false; // Tracks if Luna is holding a spore

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
        if (Input.GetButtonDown("Aid") && isHoldingSpore) // Assists with "Aid"
        {
            AidSprout();
        }
    }

    private void HandlePickupOrDrop()
    {
        if (currentHeldObject == null) // Not holding anything, try to pick up
        {
            // Check for objects in the NetLayer
            Collider2D nearbyNet = Physics2D.OverlapCircle(transform.position, netDetectionRadius, netLayer);
            if (nearbyNet != null)
            {
                // Get the root object to ensure the parent (Net) is picked up
                GameObject rootObject = nearbyNet.transform.root.gameObject;

                // Check if the root object is interactable
                if (rootObject.CompareTag("Pickupable") || rootObject.CompareTag("Spore"))
                {
                    Pickup(rootObject);
                    return;
                }
            }

            // Check for objects in the SporeLayer
            Collider2D nearbySpore = Physics2D.OverlapCircle(transform.position, sporeDetectionRadius, sporeLayer);
            if (nearbySpore != null)
            {
                GameObject rootObject = nearbySpore.transform.root.gameObject;

                // Check if the root object is interactable
                if (rootObject.CompareTag("Pickupable") || rootObject.CompareTag("Spore"))
                {
                    Pickup(rootObject);
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
        if (targetObject.CompareTag("Spore"))
        {
            if (isHoldingSpore)
            {
                Debug.Log("Luna is already holding a spore!");
                return; // Prevent picking up another spore
            }

            isHoldingSpore = true;
            currentHeldObject = targetObject;

            // Use the spore hold point
            currentHeldObject.transform.SetParent(sporeHoldPoint);
            currentHeldObject.transform.localPosition = Vector3.zero;

            Debug.Log($"Picked up spore: {currentHeldObject.name}");
        }
        else // Handle general pickupables
        {
            currentHeldObject = targetObject;

            // Use the general pickup hold point
            currentHeldObject.transform.SetParent(pickupHoldPoint);
            currentHeldObject.transform.localPosition = Vector3.zero;

            Debug.Log($"Picked up object: {currentHeldObject.name}");
        }
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

            Debug.Log($"Dropped object: {currentHeldObject.name}");

            if (isHoldingSpore)
            {
                isHoldingSpore = false;
            }

            currentHeldObject = null;
        }
    }

    private void AidSprout()
    {
        // Check for nearby sprouts using LayerMask
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, sporeDetectionRadius, sporeLayer);
        if (sproutCollider != null)
        {
            Debug.Log("Spore is near a sprout, and aid key was pressed!");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function on the sprout
                Destroy(currentHeldObject); // Destroy the spore after assisting
                currentHeldObject = null;
                isHoldingSpore = false;
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
        Gizmos.DrawWireSphere(transform.position, netDetectionRadius); // For nets
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, sporeDetectionRadius); // For spores
    }
}
