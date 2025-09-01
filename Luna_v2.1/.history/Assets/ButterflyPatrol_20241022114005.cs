using UnityEngine;
using System.Collections; // Required for IEnumerator

public class ButterflyPatrol : MonoBehaviour
{
    public float patrolRange = 2f;  // Range within which the butterfly patrols along the X-axis
    public float speed = 0.5f;
    public float pauseDuration = 1f; // Pause duration before flipping direction

    private Vector3 initialPosition;
    private Vector3 patrolStartPosition;
    private Vector3 patrolEndPosition;
    private bool patrollingRight = true;
    private bool isPausing = false; // Indicates if the butterfly is currently pausing
    private Transform luna; // Reference to Luna

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

        // If Luna is on the butterfly, keep her position updated
        if (luna != null)
        {
            luna.position = transform.position; // Update Luna's position to match the butterfly
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
        // Flip the sprite along the X-axis based on the current patrol direction
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
            luna.SetParent(transform); // Make Luna a child of the butterfly
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            luna.SetParent(null); // Detach Luna from the butterfly
            luna = null; // Clear the reference
        }
    }
}
