using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.G; // Key to pick up or drop a spore
    public Transform sporeHoldPoint; // Transform for holding the spore
    public float detectionRadius = 1.0f; // Radius for detecting spores nearby
    public LayerMask sporeLayer; // Layer mask for detecting spores

    private GameObject currentSpore; // Reference to the held spore
    private GameObject _player; // Reference to Luna's player GameObject
    private PresentSpore presentSpore; // Reference to PresentSpore component

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
            if (presentSpore.HasSporeAttached)
            {
                Debug.Log("Cannot pick up a WildSpore while an attached spore is active.");
                return;
            }

            if (currentSpore == null)
            {
                HandlePickup();
            }
            else
            {
                DropSpore();
            }
        }
    }

    private void HandlePickup()
    {
        Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, sporeLayer);
        if (nearbyCollider != null)
        {
            GameObject detectedSpore = nearbyCollider.gameObject;
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
            currentSpore = null;
            Debug.Log("Spore dropped.");
        }
    }

    public bool IsHoldingSpore()
    {
        return currentSpore != null;
    }
}
