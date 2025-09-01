using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G; // Key to interact with spores
    public Transform holdPointPrefab; // Reference for holding spores
    public Vector3 holdPointOffset = Vector3.zero; // Offset for hold point
    public float dropOffsetX = 0.5f; // Horizontal offset for drop position
    public float dropOffsetY = 0.0f; // Vertical offset for drop position

    private GameObject _player;
    private Rigidbody2D _rigidbody;
    private Transform holdPoint;
    private bool isHeld = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _rigidbody.isKinematic = true;

        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError("Player not found! Ensure Player has the correct tag.");
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogError("Collider not found on object! Add a Collider2D component.");
        }

        holdPoint = Instantiate(holdPointPrefab, _player.transform);
        holdPoint.localPosition = holdPointOffset;
    }

    private void Update()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);

        // Check if tagged as "Spore", within range, and interact key is pressed
        if (CompareTag("Spore") && Input.GetKeyDown(interactKey) && distanceToPlayer <= 1.0f)
        {
            Debug.Log("Interact key pressed within range and tag matches 'Spore'.");

            if (!isHeld)
            {
                Pickup();
            }
            else
            {
                Drop();
            }
        }
    }

    private void Pickup()
    {
        isHeld = true;

        // Attach the spore to the hold point and disable physics while held
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;

        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = true;
            _rigidbody.velocity = Vector2.zero; // Stop any movement
        }

        Debug.Log("Picked up spore. Hold point active.");
    }

    private void Drop()
    {
        isHeld = false;

        // Detach the spore from the hold point
        transform.SetParent(null);

        // Position the spore based on the player's position and offsets
        Vector3 dropPosition = new Vector3(
            _player.transform.position.x + dropOffsetX * Mathf.Sign(_player.transform.localScale.x), // Adjust based on player facing direction
            _player.transform.position.y + dropOffsetY,
            transform.position.z
        );

        transform.position = dropPosition;

        // Re-enable physics for the dropped spore
        if (_rigidbody != null)
        {
            _rigidbody.isKinematic = false;
        }

        Debug.Log("Dropped spore at position: " + transform.position);
    }
}

// Perfect