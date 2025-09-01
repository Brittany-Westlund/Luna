using UnityEngine;

public class CatchPinObject : MonoBehaviour
{
    public Transform netTransform;   // Direct reference to the net object
    public Vector3 offsetPosition = new Vector3(0, 0.23f, 0); // Static offset

    private bool isPinned = false;   // Track if the object is pinned

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the net and the object isn't already pinned
        if (other.CompareTag("Pickupable") && !isPinned)
        {
            netTransform = other.transform;  // Reference to the net's transform
            isPinned = true;  // Mark the object as pinned
            Debug.Log("Object pinned to net.");
        }
    }

    private void Update()
    {
        // If the object is pinned, set its position to follow the net with a static offset
        if (isPinned && netTransform != null)
        {
            transform.position = netTransform.position + offsetPosition;
        }
    }
}
