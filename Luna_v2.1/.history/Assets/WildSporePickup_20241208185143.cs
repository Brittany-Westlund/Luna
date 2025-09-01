using UnityEngine;
using System.Collections;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G; // Key to pick up or detach
    public KeyCode aidKey = KeyCode.A; // Key to assist with a spore
    public Transform attachPoint; // The point where the spore attaches to Luna
    public float slideSpeed = 1.5f; // Speed of the slide animation
    public float slideOffset = 0.1f; // Offset when sliding into position

    private GameObject _player;
    private bool isAttached = false; // Tracks if the spore is attached
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

        if (Input.GetKeyDown(interactKey))
        {
            if (!isAttached)
            {
                HandlePickupOrAttach();
            }
            else
            {
                DetachSpore();
            }
        }

        if (Input.GetKeyDown(aidKey) && isAttached)
        {
            AidSprout();
        }
    }

    private void HandlePickupOrAttach()
    {
        float detectionRadius = 0.8f;
        Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
        if (nearbyCollider != null && nearbyCollider.CompareTag("Spore"))
        {
            Debug.Log($"Picked up spore: {nearbyCollider.name}");
            AttachSpore(nearbyCollider.gameObject);
        }
        else
        {
            Debug.Log("No spore nearby to pick up.");
        }
    }

    private void AttachSpore(GameObject spore)
    {
        isAttached = true;

        // Attach the spore to the player’s attach point
        spore.transform.SetParent(attachPoint);
        spore.transform.localPosition = Vector3.up * slideOffset;

        // Slide the spore into position
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
        if (!isAttached) return;

        Transform spore = attachPoint.GetChild(0); // Get the attached spore
        if (spore != null)
        {
            spore.SetParent(null); // Detach the spore
            spore.position += Vector3.right * 0.5f; // Drop it slightly offset
            Debug.Log($"Spore detached: {spore.name}");
        }

        isAttached = false;
    }

    private void AidSprout()
    {
        float detectionRadius = 1.0f;
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, detectionRadius);
        if (sproutCollider != null && sproutCollider.CompareTag("Sprout"))
        {
            Debug.Log($"Aiding sprout: {sproutCollider.name}");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow();
                DetachSpore(); // Use the spore to aid the sprout
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
        Gizmos.DrawWireSphere(transform.position, 0.8f); // Pickup detection radius
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.0f); // Aid detection radius
    }
}
