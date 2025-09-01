using UnityEngine;

public class PickupPutDown : MonoBehaviour
{
    public Transform sporeHoldPoint;
    public Transform pickupHoldPoint;

    public float detectionRadius = 0.8f;

    private GameObject _player;
    private GameObject currentHeldObject;

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
        if (Input.GetKeyDown(KeyCode.G))
        {
            HandlePickupOrDrop();
        }
    }

    private void HandlePickupOrDrop()
    {
        if (currentHeldObject == null)
        {
            Collider2D detectedObject = Physics2D.OverlapCircle(transform.position, detectionRadius);
            if (detectedObject != null)
            {
                Debug.Log($"Detected object: {detectedObject.name}, Tag: {detectedObject.tag}");
                if (detectedObject.CompareTag("Pickupable") || detectedObject.CompareTag("Spore"))
                {
                    Pickup(detectedObject.gameObject);
                }
            }
            else
            {
                Debug.Log("No objects detected.");
            }
        }
        else
        {
            Drop();
        }
    }

    private void Pickup(GameObject targetObject)
    {
        currentHeldObject = targetObject;

        if (targetObject.CompareTag("Spore"))
        {
            targetObject.transform.SetParent(sporeHoldPoint);
        }
        else
        {
            targetObject.transform.SetParent(pickupHoldPoint);
        }

        targetObject.transform.localPosition = Vector3.zero;
        Debug.Log($"Picked up object: {currentHeldObject.name}");
    }

    private void Drop()
    {
        if (currentHeldObject != null)
        {
            currentHeldObject.transform.SetParent(null);
            currentHeldObject.transform.position = transform.position + new Vector3(0.5f, 0, 0); // Offset position
            Debug.Log($"Dropped object: {currentHeldObject.name}");
            currentHeldObject = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
