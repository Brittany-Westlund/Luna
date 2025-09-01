using UnityEngine;

public class PinToNet : MonoBehaviour
{
    public Transform lightNet;          // Assign LightNet directly in the Inspector
    public Vector3 offsetPosition = new Vector3(0, 0.2f, 0);  // Offset for visibility

    private void LateUpdate()
    {
        if (lightNet != null)
        {
            // Update position to follow LightNet
            transform.position = lightNet.position + offsetPosition;
        }
    }
}
