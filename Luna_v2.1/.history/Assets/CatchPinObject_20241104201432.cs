using UnityEngine;

public class CatchPinObject : MonoBehaviour
{
    public Vector3 offsetPosition = new Vector3(0, 0, 0.1f); // Offset position for the pinned object relative to the net

    private bool isPinned = false;   // Track if the object is pinned
    private Transform netTransform;  // Reference to the net's transform for pinning

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
        // If the object is pinned, update its position to follow the net with the offset
        if (isPinned && netTransform != null)
        {
            transform.position = netTransform.position + offsetPosition;
        }
    }
}
