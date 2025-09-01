using UnityEngine;

public class CatchPinObject : MonoBehaviour
{
    public Vector3 offsetPosition = new Vector3(0, 0, -0.1f); // Offset position for the pinned object relative to the net

    private bool isPinned = false;   // Track if the object is pinned
    private Transform netTransform;  // Reference to the net's transform for pinning

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the net and the object isn't already pinned
        if (other.CompareTag("Pickupable") && !isPinned)
        {
            netTransform = other.transform;  // Reference to the net's transform
            PinToNet();
        }
    }

    private void PinToNet()
    {
        // Set this object as a child of the net and apply the offset
        transform.SetParent(netTransform);
        transform.localPosition = offsetPosition;  // Position it with the specified offset behind the net
        isPinned = true;  // Mark the object as pinned

        // Ensure the object remains enabled and visible
        gameObject.SetActive(true);

        Debug.Log("Object pinned to net at position: " + transform.position);
    }
}
