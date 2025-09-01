using UnityEngine;
using System.Collections;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G; // Key to pick up or detach
    public KeyCode aidKey = KeyCode.A; // Key to assist with a spore
    public Transform attachPoint; // The point where the spore attaches to Luna
    public float slideSpeed = 1.5f; // Speed of the slide animation
    public float detectionRadius = 0.8f; // Radius for spore detection
    public float dropOffsetX = 0.5f; // Horizontal offset for dropping spores
    public float dropOffsetY = 0.0f; // Vertical offset for dropping spores

    private GameObject _player;
    private GameObject attachedSpore; // Reference to the currently attached spore
    private Coroutine slideCoroutine; // Coroutine for sliding logic

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
        // Detect nearby spore
        Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
        if (nearbyCollider != null && nearbyCollider.CompareTag("Spore"))
        {
            AttachSpore(nearbyCollider.gameObject);
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

        // Adjust Rigidbody2D for pickup
        Rigidbody2D rb = spore.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.velocity = Vector2.zero; // Stop any motion
        }

        // Attach the spore to the player
        spore.transform.SetParent(attachPoint);
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace(spore));
    }

    private IEnumerator SlideSporeIntoPlace(GameObject spore)
    {
        while (Vector3.Distance(spore.transform.position, attachPoint.position) > 0.01f)
        {
            spore.transform.position = Vector3.MoveTowards(
                spore.transform.position,
                attachPoint.position,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        spore.transform.position = attachPoint.position;
        Debug.Log("Spore attached.");
    }

    private void DetachSpore()
    {
        if (attachedSpore == null) return;

        // Detach the spore
        attachedSpore.transform.SetParent(null);

        // Adjust Rigidbody2D for drop
        Rigidbody2D rb = attachedSpore.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.isKinematic = false;
        }

        // Set drop position based on player's facing direction
        Vector3 dropPosition = new Vector3(
            _player.transform.position.x + dropOffsetX * Mathf.Sign(_player.transform.localScale.x),
            _player.transform.position.y + dropOffsetY,
            attachedSpore.transform.position.z
        );

        attachedSpore.transform.position = dropPosition;

        Debug.Log($"Spore detached: {attachedSpore.name}");
        attachedSpore = null;
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
