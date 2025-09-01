using UnityEngine;
using System.Linq; // For LINQ methods like FirstOrDefault

public class WildSporePickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G; // Key to pick up or detach
    public KeyCode aidKey = KeyCode.A; // Key to assist with a spore
    public Transform attachPoint; // The point where the spore attaches to Luna
    public float detectionRadius = 0.8f; // Radius for detecting spores
    public Vector3 dropOffset = new Vector3(0.5f, 0, 0); // Offset for dropping spores

    private GameObject _player;
    private GameObject attachedSpore; // Reference to the currently attached spore
    private Rigidbody2D sporeRb; // Reference to the spore's Rigidbody2D

    private void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player not found! Ensure Player has the correct tag.");
        }
    }

    private void Update()
    {
        if (_player == null) return;

        // Handle pickup or detach
        if (Input.GetKeyDown(interactKey))
        {
            if (attachedSpore == null)
            {
                HandlePickup();
            }
            else
            {
                DetachSpore();
            }
        }

        // Handle aiding sprouts
        if (Input.GetKeyDown(aidKey) && attachedSpore != null)
        {
            AidSprout();
        }
    }

    private void HandlePickup()
    {
        // Detect nearby objects
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);

        // Check for the first valid spore
        Collider2D sporeCollider = nearbyColliders.FirstOrDefault(c => c.CompareTag("Spore"));

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
        if (attachedSpore != null)
        {
            Debug.LogWarning("Luna is already holding a spore.");
            return;
        }

        attachedSpore = spore;

        // Disable physics while held
        sporeRb = spore.GetComponent<Rigidbody2D>();
        if (sporeRb != null)
        {
            sporeRb.isKinematic = true;
        }

        // Attach the spore to the player
        spore.transform.SetParent(attachPoint);
        spore.transform.localPosition = Vector3.zero;

        Debug.Log("Spore attached.");
    }

    private void DetachSpore()
    {
        if (attachedSpore == null) return;

        // Detach the spore
        attachedSpore.transform.SetParent(null);

        // Enable physics for the spore
        if (sporeRb != null)
        {
            sporeRb.isKinematic = false;
        }

        // Set drop position
        attachedSpore.transform.position = _player.transform.position + dropOffset;

        Debug.Log($"Spore detached: {attachedSpore.name}");
        attachedSpore = null;
        sporeRb = null;
    }

    private void AidSprout()
    {
        if (attachedSpore == null)
        {
            Debug.Log("No spore attached to assist.");
            return;
        }

        // Check for nearby sprout
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
        if (sproutCollider != null && sproutCollider.CompareTag("Sprout"))
        {
            Debug.Log($"Aiding sprout: {sproutCollider.name}");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger sprout growth
                Destroy(attachedSpore); // Consume the spore
                attachedSpore = null;
            }
        }
        else
        {
            Debug.Log("No sprout nearby to aid.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
