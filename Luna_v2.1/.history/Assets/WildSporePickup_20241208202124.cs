using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.G; // Key to pick up a spore
    public Transform sporeHoldPoint; // Transform for holding the spore
    public float detectionRadius = 1.0f; // Radius for detecting spores nearby
    public LayerMask sporeLayer; // Layer mask for detecting spores

    private GameObject _player; // Reference to Luna's player GameObject
    private PresentSpore presentSpore; // Reference to PresentSpore component

    // Property to check if Luna is already holding a spore (either from PresentSpore or this script)
    public bool IsHoldingSpore
    {
        get => presentSpore != null && presentSpore.HasSporeAttached;
    }

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
            HandlePickup();
        }
    }

    private void HandlePickup()
    {
        // Prevent picking up a WildSpore if Luna is already holding a spore
        if (IsHoldingSpore) // No parentheses since it's a property
        {
            Debug.Log("Cannot pick up a WildSpore while a spore is already attached.");
            return;
        }

        Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius, sporeLayer);
        if (nearbyCollider != null)
        {
            GameObject detectedSpore = nearbyCollider.gameObject;
            PickupWildSpore(detectedSpore);
        }
        else
        {
            Debug.Log("No spore nearby to pick up.");
        }
    }

    private void PickupWildSpore(GameObject wildSpore)
    {
        Debug.Log($"Picking up WildSpore: {wildSpore.name}");
        Destroy(wildSpore); // Destroy the wild spore object

        // Use PresentSpore's CreateSpore method to attach a new spore
        if (presentSpore != null)
        {
            presentSpore.CreateSpore();
        }
        else
        {
            Debug.LogError("PresentSpore component not found on Luna!");
        }
    }
}
