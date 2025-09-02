using UnityEngine;

public class UpperFlightLimiter : MonoBehaviour
{
    public Transform butterfly;
    public float maxY = 10f; // Set this to your ceiling height

    void Update()
    {
        if (butterfly == null) return;

        Vector3 pos = butterfly.position;
        if (pos.y > maxY)
        {
            pos.y = maxY;
            butterfly.position = pos;
        }
    }
}
