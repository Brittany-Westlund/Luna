using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G;      // Key to interact with the object
    public Transform holdPointPrefab;            // The prefab for the hold point
    public Vector3 holdPointOffset = Vector3.zero; // Offset for the hold point position
    private GameObject _player;                  // Reference to the player GameObject
    private Rigidbody2D _rigidbody;
    private Transform holdPoint;                 // Persistent hold point for holding the object
    private bool isHeld = false;                 // Track if the object is currently held

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        // Set isKinematic to true initially so the object doesn't fall due to gravity
        _rigidbody.isKinematic = true;

        // Find the player GameObject (tagged as "Player" in this example)
        _player = GameObject.FindGameObjectWithTag("Player");

        // Set the object's collider to always be a trigger to avoid physical collisions
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }

        // Create a single hold point as a child of the player to reuse for every pickup
        holdPoint = Instantiate(holdPointPrefab, _player.transform);
        holdPoint.localPosition = holdPointOffset;  // Set it to the player's position with offset
        Debug.Log("Initialized persistent holdPoint with offset.");
    }

    private void Update()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        Debug.Log("Distance to player: " + distanceToPlayer + ", isHeld: " + isHeld);

        // Check if the player is within range and presses the interact key
        if (Input.GetKeyDown(interactKey) && distanceToPlayer <= 4.0f) // 4.0f is the interaction range
        {
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

        // Set this object as a child of the persistent hold point
        transform.SetParent(holdPoint);
        transform.localPosition = Vector3.zero; // Center the object on the hold point

        Debug.Log("Object picked up at position: " + transform.position + ", holdPoint active with offset.");
    }

    private void Drop()
    {
        isHeld = false;

        // Remove this object from the hold point to "drop" it in place
        transform.SetParent(null);

        Debug.Log("Object dropped at position: " + transform.position + ", isHeld: " + isHeld);
    }
}
