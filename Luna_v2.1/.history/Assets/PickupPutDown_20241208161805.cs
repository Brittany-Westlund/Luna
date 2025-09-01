using UnityEngine;

public class NetPickupPutDown : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G;
    public Transform holdPointPrefab;
    public Vector3 holdPointOffset = Vector3.zero;
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

        // Check if tagged as "Pickupable", within range, and interact key is pressed
        if (CompareTag("Pickupable") && Input.GetKeyDown(interactKey) && distanceToPlayer <= 1.0f)
        {
            Debug.Log("Interact key pressed within range and tag matches 'Pickupable'.");

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
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero;
        Debug.Log("Picked up object. Hold point active.");
    }

    private void Drop()
    {
        isHeld = false;
        transform.SetParent(null);

        // Drop position with offsets relative to the player's position
        Vector3 dropPosition = new Vector3(
            _player.transform.position.x + dropOffsetX * _player.transform.localScale.x,
            _player.transform.position.y + dropOffsetY,
            transform.position.z
        );

        transform.position = dropPosition;

        Debug.Log("Dropped object at position: " + transform.position);
    }
}
