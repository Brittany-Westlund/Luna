using UnityEngine;

public class GroundProximityLimiter : MonoBehaviour
{
    [Tooltip("How close the butterfly can get to any collider tagged 'Ground'.")]
    public float minDistance = 1f;

    [Tooltip("The transform of your butterfly (the thing you want to keep away from ground).")]
    public Transform butterfly;

    void Update()
    {
        if (butterfly == null) return;

        Vector2 pos = butterfly.position;
        // find all colliders within the minDistance
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, minDistance);
        foreach (var col in hits)
        {
            if (!col.CompareTag("Ground")) continue;

            // find the closest point on that ground collider to our butterfly
            Vector2 closest = col.ClosestPoint(pos);
            Vector2 dir     = pos - closest;
            float   dist    = dir.magnitude;

            // if we're too close, push out along the normal
            if (dist > 0f && dist < minDistance)
            {
                Vector2 shift = dir.normalized * (minDistance - dist);
                butterfly.position = pos + shift;
                // update pos so if multiple ground hits we accumulate correctly
                pos = butterfly.position;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        if (butterfly != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(butterfly.position, minDistance);
        }
    }
}
