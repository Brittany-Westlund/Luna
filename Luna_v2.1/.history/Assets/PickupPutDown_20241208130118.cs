using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G;
    public KeyCode aidKey = KeyCode.A;
    public LayerMask sporeLayer; // Assign this in the Inspector for spore detection
    public Transform holdPointPrefab;
    public Vector3 holdPointOffset = Vector3.zero;

    private GameObject _player;
    private Rigidbody2D _rigidbody;
    private Transform SporeHoldPoint;

    private bool isHoldingSpore = false; // Tracks if Luna is holding a spore
    private GameObject currentHeldObject; // Reference to the currently held object

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.isKinematic = true;

        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player not found! Ensure Player has the correct tag.");
        }

        SporeHoldPoint = Instantiate(holdPointPrefab, _player.transform);
        SporeHoldPoint.localPosition = holdPointOffset;
    }

    private void Update()
    {
        if (_player == null) return;

        // Handle picking up objects with interact key
        if (Input.GetKeyDown(interactKey))
        {
            HandlePickupOrDrop();
        }

        // Handle aiding sprouts with the aid key
        if (Input.GetKeyDown(aidKey) && isHoldingSpore)
        {
            AidSprout();
        }
    }

    private void HandlePickupOrDrop()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);

        if (currentHeldObject == null) // Not holding anything, try to pick up
        {
            Collider2D nearbyCollider = Physics2D.OverlapCircle(transform.position, 1.0f);
            if (nearbyCollider != null && (nearbyCollider.CompareTag("Pickupable") || nearbyCollider.CompareTag("Spore")))
            {
                Pickup(nearbyCollider.gameObject);
            }
        }
        else // Already holding something, drop it
        {
            Drop();
        }
    }

    private void Pickup(GameObject targetObject)
    {
        if (targetObject.CompareTag("Spore"))
        {
            if (isHoldingSpore)
            {
                Debug.Log("Luna is already holding a spore!");
                return; // Prevent picking up another spore
            }

            isHoldingSpore = true;
        }

        // Set the object as held
        currentHeldObject = targetObject;
        currentHeldObject.transform.SetParent(SporeHoldPoint);
        currentHeldObject.transform.localPosition = Vector3.zero;

        Debug.Log($"Picked up object: {currentHeldObject.name}");
    }

    private void Drop()
    {
        if (currentHeldObject != null)
        {
            // Detach the object
            currentHeldObject.transform.SetParent(null);

            // Reset its position slightly in front of Luna
            currentHeldObject.transform.position = new Vector3(
                _player.transform.position.x + 0.5f * _player.transform.localScale.x, // Offset based on facing direction
                _player.transform.position.y,
                currentHeldObject.transform.position.z
            );

            Debug.Log($"Dropped object: {currentHeldObject.name}");

            // If it’s a spore, keep it in the scene but reset the holding state
            if (isHoldingSpore)
            {
                isHoldingSpore = false;
            }

            currentHeldObject = null;
        }
    }

    private void AidSprout()
    {
        // Check for nearby sprouts using LayerMask
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, 1.0f, sporeLayer);
        if (sproutCollider != null)
        {
            Debug.Log("Spore is near a sprout, and aid key was pressed!");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function on the sprout
                Destroy(currentHeldObject); // Destroy the spore after assisting
                currentHeldObject = null;
                isHoldingSpore = false;
            }
        }
        else
        {
            Debug.Log("No sprout nearby to assist.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the OverlapCircle in the Scene view for debugging
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}
