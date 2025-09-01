using UnityEngine;
using System.Collections; // Required for IEnumerator

public class ButterflyPatrol : MonoBehaviour
{
    public float patrolRange = 2f;  // Range within which the butterfly patrols along the X-axis
    public float speed = 0.5f;
    public float pauseDuration = 1f; // Pause duration before flipping direction
    public float idleThreshold = 0.1f; // Threshold to detect idle state (no movement)

    private Vector3 initialPosition;
    private Vector3 patrolStartPosition;
    private Vector3 patrolEndPosition;
    private bool patrollingRight = true;
    private bool isPausing = false; // Indicates if the butterfly is currently pausing
    private Transform luna; // Reference to Luna
    private Vector3 lastLunaPosition; // To track Luna's position and detect movement
    private Vector3 originalLunaScale; // To store Luna's original scale

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
        if (!isPausing) // Only patrol if not pausing
        {
            Patrol();
        }

        // If Luna is on the butterfly, check for movement
        if (luna != null)
        {
            // Check if Luna has moved
            if (IsLunaIdle() && luna.parent == null)
            {
                // If idle and not already a child of the butterfly, parent her
                originalLunaScale = luna.localScale; // Save Luna's original scale before parenting
                luna.SetParent(transform);
            }
            else if (!IsLunaIdle() && luna.parent != null)
            {
                // If not idle and still a child of the butterfly, unparent her
                JumpOff();
            }

            // Check for jump input
            if (Input.GetButtonDown("Jump")) // Replace "Jump" with your specific jump input if needed
            {
                JumpOff(); // Detach and jump off
            }
        }
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
            // Start pausing before switching patrol direction
            StartCoroutine(PauseAndFlip());
        }
    }

    private IEnumerator PauseAndFlip()
    {
        isPausing = true; // Set pausing state to true
        yield return new WaitForSeconds(pauseDuration); // Wait for the specified duration

        // Switch patrol direction
        patrollingRight = !patrollingRight;
        UpdateFacingDirection();
        isPausing = false; // Reset pausing state
    }

    void UpdateFacingDirection()
    {
        // Flip the butterfly's sprite along the X-axis based on the current patrol direction
        if (patrollingRight)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            luna = other.transform; // Reference to Luna
            lastLunaPosition = luna.position; // Initialize last position for movement tracking
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            luna.SetParent(null); // Detach Luna from the butterfly
            luna.localScale = originalLunaScale; // Restore Luna's original scale when unparenting
            luna = null; // Clear the reference
        }
    }

    void JumpOff()
    {
        if (luna != null)
        {
            // Detach Luna from the butterfly
            luna.SetParent(null);

            // Restore Luna's original scale to avoid flipping movement controls
            luna.localScale = originalLunaScale;

            // Apply a jump force. You can adjust the force based on your needs.
            Rigidbody2D lunaRigidbody = luna.GetComponent<Rigidbody2D>();
            if (lunaRigidbody != null)
            {
                lunaRigidbody.velocity = new Vector2(lunaRigidbody.velocity.x, 5f); // Set vertical velocity for jumping
            }

            luna = null; // Clear the reference
        }
    }

    // Method to check if Luna is idle (not moving)
    bool IsLunaIdle()
    {
        if (luna == null) return false;

        // Check if Luna has moved beyond the idle threshold
        float movementDistance = Vector3.Distance(luna.position, lastLunaPosition);
        bool isIdle = movementDistance < idleThreshold;

        // Update the last known position
        lastLunaPosition = luna.position;

        return isIdle;
    }
}
