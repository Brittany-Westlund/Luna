using UnityEngine;
using System.Collections;

public class PressurePlate : MonoBehaviour
{
    public float delay = 1f; // Delay before responding to player's presence
    public float downDistance = 0.1f; // Distance the plate moves down
    public float detectionOffset = 0.1f; // Offset for the detection area
    public LayerMask playerLayer; // Layer on which the player is placed to filter the checks

    private Vector3 originalPosition;
    private Vector3 downPosition;
    private bool isPressed = false;

    void Start()
    {
        originalPosition = transform.position;
        downPosition = originalPosition - Vector3.up * downDistance;
    }

    void Update()
    {
        // Calculate the position of the detection area with the offset
        Vector2 detectionPosition = new Vector2(transform.position.x, transform.position.y + detectionOffset);

        // Check if any player is within a certain distance of the pressure plate
        bool playerNearby = Physics2D.OverlapCircle(detectionPosition, 0.1f, playerLayer);

        if (playerNearby && !isPressed)
        {
            StartCoroutine(TriggerPlate());
        }
    }

    IEnumerator TriggerPlate()
    {
        isPressed = true;
        transform.position = downPosition; // Move the plate down

        yield return new WaitForSeconds(delay);

        // Calculate the position of the detection area with the offset
        Vector2 detectionPosition = new Vector2(transform.position.x, transform.position.y + detectionOffset);

        // Check if the player is still nearby after the delay
        bool playerStillNearby = Physics2D.OverlapCircle(detectionPosition, 0.1f, playerLayer);
        
        // If the player is no longer nearby, move the plate back up
        if (!playerStillNearby)
        {
            transform.position = originalPosition;
            isPressed = false;
        }
        else
        {
            // If the player is still nearby, restart the coroutine
            StartCoroutine(TriggerPlate());
        }
    }

    void OnDrawGizmos()
    {
        // Optionally draw a small detection radius in the editor for visualization
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(new Vector3(transform.position.x, transform.position.y + detectionOffset, transform.position.z), 0.1f);
    }
}
