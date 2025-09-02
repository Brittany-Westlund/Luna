using UnityEngine;

public class ButterflyMovement : MonoBehaviour
{
    public float patrolRange = 2f;  // Range within which the butterfly patrols along the X-axis
    public float speed = 0.5f;
    public Transform target;  // Player's transform
    public bool isTalking = false;  // Indicates if the butterfly is in conversation
    public BoxCollider2D conversationArea; // Reference to the conversation area collider

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
        if (!isTalking)
        {
            Patrol();
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

    // Call this method to start conversation
    public void StartConversation()
    {
        isTalking = true;
        // Stop movement when conversation starts
        speed = 0f;
    }

    // Call this method to end conversation
    public void EndConversation()
    {
        isTalking = false;
        // Resume movement when conversation ends
        speed = 0.5f;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // Start conversation when Luna enters the conversation area
            StartConversation();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // End conversation when Luna exits the conversation area
            EndConversation();
        }
    }
}
