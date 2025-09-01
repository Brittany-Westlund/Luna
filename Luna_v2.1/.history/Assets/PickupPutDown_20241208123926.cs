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
    private Transform holdPoint;
    private bool isHeld = false;
    private bool isHoldingSpore = false; // Tracks if Luna is holding a spore
    private GameObject currentSpore; // Reference to the held spore


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

        // Check if the object is interactable by tag (backup logic)
        if ((CompareTag("Pickupable") || CompareTag("Spore")) && Input.GetKeyDown(interactKey) && distanceToPlayer <= 1.0f)
        {
            Debug.Log("Interact key pressed within range and tag matches.");

            if (!isHeld)
            {
                Pickup();
            }
            else
            {
                Drop();
            }
        }

        // Check for sprout interaction when A key is pressed
        if (isHeld && CompareTag("Spore") && Input.GetKeyDown(aidKey))
        {
            AidSprout();
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
                Drop(); // Optionally drop the spore after assisting
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the OverlapCircle in the Scene view for debugging
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 1.0f);
    }
}
