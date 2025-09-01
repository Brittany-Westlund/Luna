using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode pickupKey = KeyCode.G;
    public Transform sporeHoldPoint;
    public float detectionRadius = 1.0f;
    public LayerMask sporeLayer;

    private GameObject _player;
    private PresentSpore presentSpore;

    public bool IsHoldingSpore => presentSpore != null && presentSpore.HasSporeAttached; // Correct usage of property

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
        if (IsHoldingSpore)
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
        Destroy(wildSpore);

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
