using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.G; // Key to pick up or drop a spore
    public Transform sporeHoldPoint; // Transform for holding the spore
    public float detectionRadius = 1.0f; // Radius for detecting spores nearby
    public LayerMask sporeLayer; // Layer mask for detecting spores

    private GameObject currentSpore; // Reference to the held spore
    private GameObject _player; // Reference to Luna's player GameObject
    private PresentSpore presentSpore; // Reference to the PresentSpore component

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player not found! Ensure Player has the correct tag.");
        }

        presentSpore = _player.GetComponent<PresentSpore>();
        if (presentSpore == null)
        {
            Debug.LogError("PresentSpore component not found on Player!");
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(pickupKey))
        {
            if (currentSpore == null) // Not holding a spore, try to pick one up
            {
                HandlePickup();
            }
            else // Holding a spore, drop it
            {
                DropSpore();
            }
        }
    }

    private void HandlePickup()
    {
        if (presentSpore.HasSporeAttached)
        {
            Debug.Log("Luna cannot pick up a WildSpore while she has an attached spore.");
            return;
        }

        // Detect nearby spores within the specified radius
        Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, sporeLayer);
        if (nearbyCollider != null)
        {
            GameObject detectedSpore = nearbyCollider.gameObject;
            Debug.Log($"Spore detected: {detectedSpore.name}");
            AttachSpore(detectedSpore);
        }
        else
        {
            Debug.Log("No spore nearby to pick up.");
        }
    }

    private void AttachSpore(GameObject spore)
    {
        currentSpore = spore;
        currentSpore.transform.SetParent(sporeHoldPoint);
        currentSpore.transform.localPosition = Vector3.zero;

        Rigidbody2D sporeRigidbody = currentSpore.GetComponent<Rigidbody2D>();
        if (sporeRigidbody != null)
        {
            sporeRigidbody.isKinematic = true; // Disable physics while held
            sporeRigidbody.velocity = Vector2.zero;
        }

        Debug.Log("Spore attached.");
    }

    private void DropSpore()
    {
        if (currentSpore != null)
        {
            currentSpore.transform.SetParent(null);

            // Drop the spore slightly in front of Luna
            Vector3 dropPosition = new Vector3(
                _player.transform.position.x + 0.5f * Mathf.Sign(_player.transform.localScale.x), // Offset based on facing direction
                _player.transform.position.y,
                currentSpore.transform.position.z
            );

            currentSpore.transform.position = dropPosition;

            Rigidbody2D sporeRigidbody = currentSpore.GetComponent<Rigidbody2D>();
            if (sporeRigidbody != null)
            {
                sporeRigidbody.isKinematic = false; // Re-enable physics for the spore
            }

            Debug.Log($"Spore dropped: {currentSpore.name}");
            currentSpore = null;
        }
    }

    public bool IsHoldingSpore()
    {
        return currentSpore != null;
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the detection radius in the Scene view
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
