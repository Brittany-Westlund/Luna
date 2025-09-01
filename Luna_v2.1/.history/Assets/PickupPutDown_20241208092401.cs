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

    // Check if tagged as "Pickupable" or "Spore", within range, and interact key is pressed
    if ((CompareTag("Pickupable") || CompareTag("Spore")) && Input.GetKeyDown(interactKey) && distanceToPlayer <= 10.0f)
    {
        Debug.Log("Interact key pressed within range and tag matches 'Pickupable' or 'Spore'.");

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
    if (isHeld && CompareTag("Spore") && Input.GetKeyDown(KeyCode.A))
    {
        AidSprout();
    }
}

private void AidSprout()
    {
        // Use a proximity check for the sprout
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, 1.0f, LayerMask.GetMask("Sprout")); // Adjust radius as needed
        if (sproutCollider != null)
        {
            Debug.Log("Spore is near a sprout, and A was pressed!");
            AidingSprouts sproutScript = sproutCollider.GetComponent<AidingSprouts>();
            if (sproutScript != null)
            {
                sproutScript.Grow(); // Trigger the growth function on the sprout
                Drop(); // Optionally drop the spore after assisting
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
