using UnityEngine;

public class FlightBoundaryLimiter : MonoBehaviour
{
    public Transform butterfly;

    [Header("Bounds")]
    public float minX = -10f;
    public float maxX =  10f;
    public float minY = -1f;
    public float maxY =  8f;

    void Update()
    {
        if (butterfly == null) return;

        Vector3 pos = butterfly.position;

        // Clamp the butterfly’s position to stay within bounds
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.y = Mathf.Clamp(pos.y, minY, maxY);

        butterfly.position = pos;
    }

    void OnDrawGizmosSelected()
    {
        // Draw the boundary box in the Scene view for visualization
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(new Vector3(minX, minY), new Vector3(maxX, minY));
        Gizmos.DrawLine(new Vector3(minX, maxY), new Vector3(maxX, maxY));
        Gizmos.DrawLine(new Vector3(minX, minY), new Vector3(minX, maxY));
        Gizmos.DrawLine(new Vector3(maxX, minY), new Vector3(maxX, maxY));
    }
}
