using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G; // Key to pick up/detach wild spores
    public KeyCode sporeCreateKey = KeyCode.S; // Key to create cap spores
    public Transform sporeHoldPoint; // Hold point for the spore
    public GameObject sporePrefab; // Reference to the WildSpore prefab

    private GameObject currentSpore; // Reference to the currently held spore
    private bool isHoldingSpore = false; // Tracks if Luna is holding a spore

    private void Update()
    {
        // Handle picking up and detaching WildSpores
        if (Input.GetKeyDown(interactKey))
        {
            HandlePickupOrDetach();
        }

        // Disable cap spore creation when holding a spore
        if (Input.GetKeyDown(sporeCreateKey) && !isHoldingSpore)
        {
            CreateCapSpore();
        }
    }

    private void HandlePickupOrDetach()
    {
        if (currentSpore == null) // Try to pick up a spore
        {
            Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, 1.0f);
            if (nearbyCollider != null && nearbyCollider.CompareTag("Spore"))
            {
                PickupSpore(nearbyCollider.gameObject);
            }
            else
            {
                Debug.Log("No spore nearby to pick up.");
            }
        }
        else // Detach the currently held spore
        {
            DetachSpore();
        }
    }

    private void PickupSpore(GameObject spore)
    {
        if (isHoldingSpore)
        {
            Debug.LogWarning("Luna is already holding a spore!");
            return;
        }

        currentSpore = spore;
        isHoldingSpore = true;

        // Attach the spore to the hold point
        spore.transform.SetParent(sporeHoldPoint);
        spore.transform.localPosition = Vector3.zero;

        // Disable physics for the spore while held
        Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }

        Debug.Log($"Picked up spore: {spore.name}");
    }

    private void DetachSpore()
    {
        if (currentSpore != null)
        {
            // Detach the spore from the hold point
            currentSpore.transform.SetParent(null);

            // Re-enable physics for the spore
            Rigidbody2D rb = currentSpore.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // Drop the spore slightly in front of Luna
            currentSpore.transform.position = new Vector3(
                transform.position.x + 0.5f * transform.localScale.x,
                transform.position.y,
                currentSpore.transform.position.z
            );

            Debug.Log($"Detached spore: {currentSpore.name}");
            currentSpore = null;
            isHoldingSpore = false;
        }
    }

    private void CreateCapSpore()
    {
        if (sporePrefab == null)
        {
            Debug.LogError("Spore prefab is not assigned!");
            return;
        }

        // Instantiate a new spore and set it to the hold point
        GameObject newSpore = Instantiate(sporePrefab, sporeHoldPoint.position, Quaternion.identity);
        PickupSpore(newSpore); // Automatically pick up the created spore

        Debug.Log("Created and picked up a cap spore.");
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}
