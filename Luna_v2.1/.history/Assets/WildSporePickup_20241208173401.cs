using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G; // Key to pick up/detach wild spores
    public Transform sporeHoldPoint; // Hold point for the spore
    public PresentSpore presentSporeScript; // Reference to the PresentSpore script
    public float detectionRadius = 0.8f; // Radius for detecting pickupables

    private GameObject currentSpore; // Reference to the currently held spore
    private bool isHoldingWildSpore = false; // Tracks if Luna is holding a WildSpore

    private void Start()
    {
        if (presentSporeScript == null)
        {
            Debug.LogError("PresentSpore script is not assigned!");
        }
    }

    private void Update()
    {
        // Handle picking up and detaching WildSpores
        if (Input.GetButtonDown("Player1_Grab"))
        {
            HandlePickupOrDetach();
        }

        // Disable cap spore creation when holding a WildSpore
        if (Input.GetKeyDown(KeyCode.S) && !isHoldingWildSpore)
        {
            presentSporeScript.CreateSpore();
        }
    }

    private void HandlePickupOrDetach()
    {
        if (currentSpore == null) // Try to pick up a WildSpore
        {
            Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
            if (nearbyCollider != null && nearbyCollider.CompareTag("Spore"))
            {
                PickupSpore(nearbyCollider.gameObject);
            }
            else
            {
                Debug.Log("No spore nearby to pick up.");
            }
        }
        else // Detach the currently held WildSpore
        {
            DetachSpore();
        }
    }

    private void PickupSpore(GameObject spore)
    {
        if (isHoldingWildSpore || presentSporeScript.HasSporeAttached)
        {
            Debug.LogWarning("Luna is already holding a spore!");
            return;
        }

        currentSpore = spore;
        isHoldingWildSpore = true;

        // Attach the spore to the hold point
        spore.transform.SetParent(sporeHoldPoint);
        spore.transform.localPosition = Vector3.zero;

        // Disable physics for the spore while held
        Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero; // Stop any movement
        }

        Debug.Log($"Picked up WildSpore: {spore.name}");
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

            Debug.Log($"Detached WildSpore: {currentSpore.name}");
            currentSpore = null;
            isHoldingWildSpore = false;
        }
    }
}
