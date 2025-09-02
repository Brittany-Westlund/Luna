using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.G;  // Key to pick up/drop spores
    public KeyCode aidKey = KeyCode.A;     // Key to aid sprouts
    public float detectionRadius = 1.0f;   // Radius to detect spores/sprouts
    public Transform sporeHoldPoint;       // Hold point for the spore

    private GameObject currentSpore;       // Currently held spore
    private bool isHoldingSpore = false;   // Whether Luna is holding a spore

    void Update()
    {
        // Handle picking up or dropping spores
        if (Input.GetKeyDown(pickupKey))
        {
            if (isHoldingSpore)
            {
                DetachSpore();
            }
            else
            {
                HandlePickup();
            }
        }

        // Handle aiding sprouts
        if (Input.GetKeyDown(aidKey) && isHoldingSpore)
        {
            AidSprout();
        }
    }

    private void HandlePickup()
    {
        // Detect nearby spores
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Collider2D sporeCollider = System.Array.Find(nearbyColliders, c => c.CompareTag("Spore"));

        if (sporeCollider != null)
        {
            Debug.Log($"Spore detected: {sporeCollider.name}");
            AttachSpore(sporeCollider.gameObject);
        }
        else
        {
            Debug.Log("No spore nearby to pick up.");
        }
    }

    private void AttachSpore(GameObject spore)
    {
        currentSpore = spore;
        isHoldingSpore = true;

        // Attach the spore to the hold point
        currentSpore.transform.SetParent(sporeHoldPoint);
        currentSpore.transform.localPosition = Vector3.zero;

        Rigidbody2D sporeRb = currentSpore.GetComponent<Rigidbody2D>();
        if (sporeRb != null)
        {
            sporeRb.isKinematic = true; // Disable physics while held
        }

        Debug.Log("Spore attached.");
    }

    private void DetachSpore()
    {
        if (currentSpore == null) return;

        // Detach the spore
        currentSpore.transform.SetParent(null);

        Rigidbody2D sporeRb = currentSpore.GetComponent<Rigidbody2D>();
        if (sporeRb != null)
        {
            sporeRb.isKinematic = false; // Re-enable physics
        }

        Debug.Log($"Spore detached: {currentSpore.name}");
        currentSpore = null;
        isHoldingSpore = false;
    }

    private void AidSprout()
    {
        // Detect nearby sprouts
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
        Collider2D sproutCollider = System.Array.Find(nearbyColliders, c => c.CompareTag("Sprout"));

        if (sproutCollider != null)
        {
            Debug.Log($"Sprout detected: {sproutCollider.name}");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();

            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function on the sprout
                Destroy(currentSpore); // Destroy the held spore after aiding
                currentSpore = null;
                isHoldingSpore = false;

                Debug.Log("Spore used to aid sprout.");
            }
        }
        else
        {
            Debug.Log("No sprout nearby to assist.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize detection radius in the editor
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}

// Picks up and aids making sprout larger