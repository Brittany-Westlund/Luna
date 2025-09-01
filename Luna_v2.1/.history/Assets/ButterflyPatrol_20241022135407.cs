using UnityEngine;
using System.Collections;

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
    private bool isLunaAttached = false; // Flag to check if Luna is currently "attached"

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

        // If Luna is "attached", move her manually along with the butterfly
        if (isLunaAttached && luna != null)
        {
            luna.position = new Vector3(transform.position.x, luna.position.y, luna.position.z); // Move only on X axis

            // If Luna moves, detach her
            if (!IsLunaIdle() || Input.GetButtonDown("Jump")) // Replace "Jump" with your specific jump input if needed
            {
                DetachLuna();
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
        SpriteRenderer spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.flipX = !patrollingRight;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            luna = other.transform; // Reference to Luna
            lastLunaPosition = luna.position; // Initialize last position for movement tracking
            isLunaAttached = true; // Luna is now attached to the butterfly
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            DetachLuna();
        }
    }

    void DetachLuna()
    {
        if (luna != null)
        {
            isLunaAttached = false; // Luna is no longer attached
            luna = null; // Clear the reference to Luna
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
