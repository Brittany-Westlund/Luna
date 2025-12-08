using UnityEngine;

public class ElevatorIgnoreOneWayWhileMoving : MonoBehaviour
{
    [Header("Elevator")]
    public Collider2D elevatorCollider;

    [Header("Detection")]
    public string playerTag = "Player";
    public LayerMask oneWayPlatformLayer;

    private Collider2D riderCollider;

    private Vector3 lastPosition;
    private bool isIgnoring = false;

    private void Awake()
    {
        if (elevatorCollider == null)
            elevatorCollider = GetComponent<Collider2D>();

        lastPosition = transform.position;
    }

    private void Update()
    {
        // Auto-detect Luna if she enters the scene later
        if (riderCollider == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag(playerTag);
            if (player != null)
                riderCollider = player.GetComponent<Collider2D>();
        }

        bool isMoving = Vector3.Distance(transform.position, lastPosition) > 0.001f;

        if (isMoving && !isIgnoring)
        {
            EnablePassThrough();
        }
        else if (!isMoving && isIgnoring)
        {
            DisablePassThrough();
        }

        lastPosition = transform.position;
    }

    private void EnablePassThrough()
    {
        isIgnoring = true;

        Collider2D[] platforms = Physics2D.OverlapBoxAll(
            elevatorCollider.bounds.center,
            elevatorCollider.bounds.size,
            0f,
            oneWayPlatformLayer
        );

        foreach (Collider2D platform in platforms)
        {
            // Ignore for elevator
            Physics2D.IgnoreCollision(elevatorCollider, platform, true);

            // Ignore for Luna if present
            if (riderCollider != null)
                Physics2D.IgnoreCollision(riderCollider, platform, true);
        }
    }

    private void DisablePassThrough()
    {
        isIgnoring = false;

        Collider2D[] platforms = Physics2D.OverlapBoxAll(
            elevatorCollider.bounds.center,
            elevatorCollider.bounds.size,
            0f,
            oneWayPlatformLayer
        );

        foreach (Collider2D platform in platforms)
        {
            // Restore for elevator
            Physics2D.IgnoreCollision(elevatorCollider, platform, false);

            // Restore for Luna if present
            if (riderCollider != null)
                Physics2D.IgnoreCollision(riderCollider, platform, false);
        }
    }
}
