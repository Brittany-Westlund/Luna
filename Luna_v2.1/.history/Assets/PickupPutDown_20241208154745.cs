using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public LayerMask sporeLayer; // For detecting spores
    public LayerMask sproutLayer; // For detecting sprouts
    public Transform sporeHoldPoint; // Reference for holding spores
    public Transform NetLayer; // Reference for holding nets
    public Transform pickupHoldPoint; // Reference for holding other pickupables

    public float detectionRadius = 0.8f; // Radius for detecting pickupables and sprouts

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

        // Handle picking up and dropping objects with the Grab input
        if (Input.GetButtonDown("Player1_Grab"))
        {
            HandlePickupOrDrop();
        }

        // Handle aiding sprouts with the Aid input
        if (Input.GetButtonDown("Aid") && currentHeldObject != null && currentHeldObject.CompareTag("Spore"))
        {
            AidSprout();
        }
    }

    private void HandlePickupOrDrop()
    {
        if (currentHeldObject == null) // Not holding anything, try to pick up
        {
            Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
            if (nearbyCollider != null)
            {
                Debug.Log($"Detected object: {nearbyCollider.name}, Tag: {nearbyCollider.tag}, Layer: {LayerMask.LayerToName(nearbyCollider.gameObject.layer)}");

                GameObject targetObject = nearbyCollider.transform.root.gameObject;

                if (targetObject.CompareTag("Pickupable") || targetObject.CompareTag("Spore"))
                {
                    Pickup(targetObject);
                }
                else
                {
                    Debug.Log($"Object '{targetObject.name}' is not interactable.");
                }
            }
            else
            {
                Debug.Log("No nearby objects detected.");
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
            Debug.LogWarning("Luna is already holding an object.");
            return;
        }

        currentHeldObject = targetObject;

        if (targetObject.CompareTag("Spore"))
        {
            currentHeldObject.transform.SetParent(sporeHoldPoint);
        }
        else // Handle general pickupables
        {
            currentHeldObject.transform.SetParent(pickupHoldPoint);
        }

        currentHeldObject.transform.localPosition = Vector3.zero;

        Rigidbody2D rb = currentHeldObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true; // Disable physics while held
        }

        Debug.Log($"Picked up object: {currentHeldObject.name}");
    }

    private void Drop()
    {
        if (currentHeldObject != null)
        {
            currentHeldObject.transform.SetParent(null);

            Vector3 dropPosition = new Vector3(
                _player.transform.position.x + 0.5f * _player.transform.localScale.x,
                _player.transform.position.y,
                currentHeldObject.transform.position.z
            );

            currentHeldObject.transform.position = dropPosition;

            Rigidbody2D rb = currentHeldObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false; // Re-enable physics on drop
            }

            Debug.Log($"Dropped object: {currentHeldObject.name}");
            currentHeldObject = null;
        }
    }

    private void AidSprout()
    {
        if (currentHeldObject != null && currentHeldObject.CompareTag("Spore"))
        {
            Collider2D nearbySprout = Physics2D.OverlapCircle(transform.position, detectionRadius, sproutLayer);

            if (nearbySprout != null)
            {
                AidingSprouts sproutScript = nearbySprout.GetComponent<AidingSprouts>();
                if (sproutScript != null)
                {
                    sproutScript.Grow(); // Trigger sprout growth
                    Destroy(currentHeldObject); // Destroy the spore after aiding
                    currentHeldObject = null;
                }
            }
            else
            {
                Debug.Log("No nearby sprout to assist.");
            }
        }
        else
        {
            Debug.Log("Luna is not holding a spore.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
