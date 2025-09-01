using UnityEngine;

public class CatchPinObject : MonoBehaviour
{
    public Vector3 offsetPosition = new Vector3(0, 0, -0.1f); // Offset position for the pinned object relative to the net

    private bool isPinned = false;   // Track if the object is pinned
    private Transform netTransform;  // Reference to the net's transform for pinning

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the colliding object is the net and the object isn't already pinned
        if (other.CompareTag("Net") && !isPinned)
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

        Debug.Log("Object pinned to net at position: " + transform.position);
    }

    private void Update()
    {
        // Optional: Add behavior for the object when it's pinned, such as visual effects
        if (isPinned)
        {
            // Additional pinned behavior can go here
        }
    }
}
