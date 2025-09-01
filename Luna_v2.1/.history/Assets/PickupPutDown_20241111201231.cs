using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public KeyCode interactKey = KeyCode.G;
    public Transform holdPointPrefab;
    public Vector3 holdPointOffset = Vector3.zero;
    private GameObject _player;
    private Rigidbody2D _rigidbody;
    private Transform holdPoint;
    private bool isHeld = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();

        _rigidbody.isKinematic = true;
        Debug.Log("Rigidbody set to kinematic.");

        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null) Debug.LogError("Player not found! Ensure Player has the correct tag.");

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
            Debug.Log("Collider set to trigger.");
        }
        else
        {
            Debug.LogError("Collider not found on object! Add a Collider2D component.");
        }

        holdPoint = Instantiate(holdPointPrefab, _player.transform);
        holdPoint.localPosition = holdPointOffset;
        Debug.Log("Hold point initialized at: " + holdPoint.position);
    }

    private void Update()
    {
        if (_player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);
        Debug.Log($"Distance to player: {distanceToPlayer}, isHeld: {isHeld}, Tag: {gameObject.tag}");

        // Check if tagged as "Pickupable", within range, and interact key is pressed
        if (CompareTag("Pickupable") && Input.GetKeyDown(interactKey) && distanceToPlayer <= 10.0f)
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
        Debug.Log("Dropped object at position: " + transform.position);
    }
}
