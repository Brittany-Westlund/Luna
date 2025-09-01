using UnityEngine;

public class ButterflyPatrol : MonoBehaviour
{
    public float patrolRange = 2f;  // Range within which the butterfly patrols along the X-axis
    public float speed = 0.5f;

    private Vector3 initialPosition;
    private Vector3 patrolStartPosition;
    private Vector3 patrolEndPosition;
    private bool patrollingRight = true;

    void Start()
    {
        initialPosition = transform.position;
        patrolStartPosition = initialPosition - Vector3.right * patrolRange;
        patrolEndPosition = initialPosition + Vector3.right * patrolRange;

        // Initialize facing direction
        UpdateFacingDirection();
    }

    void Update()
    {
        Patrol();
    }

    void Patrol()
    {
        // Determine the target position based on patrol direction
        Vector3 targetPosition = patrollingRight ? patrolEndPosition : patrolStartPosition;

        // Move towards the target position
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // Check if reached the target position
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // Switch patrol direction
            patrollingRight = !patrollingRight;
            UpdateFacingDirection();
        }
    }

    void UpdateFacingDirection()
    {
        // Flip the sprite along the X-axis based on the current patrol direction
        if (patrollingRight)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }
}
