using UnityEngine;
using System.Linq;

public class PickupPutDown : MonoBehaviour
{
    [Header("Settings")]
    public KeyCode interactKey = KeyCode.S; 
    public Vector3 holdPointOffset = Vector3.zero;
    public float dropOffsetX = 0.5f;
    public float dropOffsetY = 0.0f;

    private GameObject _player;
    private Rigidbody2D _rigidbody;
    private Transform holdPoint;
    private bool isHeld = false;

    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        if (_rigidbody == null)
        {
            Debug.LogError($"{name}: Missing Rigidbody2D component!");
            return;
        }

        _rigidbody.isKinematic = true;

        // Find player
        _player = GameObject.FindGameObjectWithTag("Player");
        if (_player == null)
        {
            Debug.LogError($"{name}: Player not found! Ensure Player has the correct tag.");
            return;
        }

        // Try to locate SporeHoldPoint under Luna or anywhere in player hierarchy
        holdPoint = FindHoldPoint(_player.transform, "SporeHoldPoint");

        if (holdPoint == null)
        {
            Debug.LogWarning($"{name}: SporeHoldPoint not found under Player. Creating temporary hold point.");
            GameObject tempHold = new GameObject("TempHoldPoint");
            tempHold.transform.SetParent(_player.transform);
            tempHold.transform.localPosition = Vector3.zero;
            holdPoint = tempHold.transform;
        }

        // Ensure collider exists and acts as trigger
        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.isTrigger = true;
        }
        else
        {
            Debug.LogError($"{name}: Collider2D not found! Add a collider to use PickupPutDown.");
        }
    }

    private void Update()
    {
        if (_player == null || holdPoint == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _player.transform.position);

        if (CompareTag("Pickupable") && Input.GetKeyDown(interactKey) && distanceToPlayer <= 1.0f)
        {
            if (!isHeld)
                Pickup();
            else
                Drop();
        }
    }

    private void Pickup()
    {
        isHeld = true;
        transform.SetParent(holdPoint);
        transform.localPosition = holdPointOffset;

        _rigidbody.isKinematic = true;
        _rigidbody.velocity = Vector2.zero;

        Debug.Log($"{name}: Picked up. Attached to {holdPoint.name}.");
    }

    private void Drop()
    {
        isHeld = false;
        transform.SetParent(null);

        Vector3 dropPosition = new Vector3(
            _player.transform.position.x + dropOffsetX * Mathf.Sign(_player.transform.localScale.x),
            _player.transform.position.y + dropOffsetY,
            transform.position.z
        );

        transform.position = dropPosition;
        _rigidbody.isKinematic = false;

        Debug.Log($"{name}: Dropped at position {transform.position}.");
    }

    /// <summary>
    /// Searches for the hold point recursively under the given root transform.
    /// </summary>
    private Transform FindHoldPoint(Transform root, string targetName)
    {
        // First, look for Luna explicitly if she exists
        var luna = root.GetComponentsInChildren<Transform>(true)
                       .FirstOrDefault(t => t.name == "Luna");
        if (luna != null)
        {
            var child = luna.GetComponentsInChildren<Transform>(true)
                            .FirstOrDefault(t => t.name == targetName);
            if (child != null)
                return child;
        }

        // If not found, look anywhere under Player hierarchy
        var fallback = root.GetComponentsInChildren<Transform>(true)
                           .FirstOrDefault(t => t.name == targetName);
        return fallback;
    }
}
