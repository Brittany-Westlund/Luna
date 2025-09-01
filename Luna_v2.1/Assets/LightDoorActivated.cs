using System.Collections;
using UnityEngine;

public class LightDoorActivated : MonoBehaviour
{
    public GameObject vine; // Attach your vine GameObject here in the Inspector
    public SpriteRenderer LightDoor; // Attach LightDoor's SpriteRenderer here in the Inspector
    public Vector3 targetPosition; // Set the target position for the vine in the Inspector
    public float speed = 1f; // Set the speed of the vine's movement

    private bool isMoving = false; // Flag to check if vine is already moving

    // Method to be called when the X button is pressed
    public void ToggleLightDoor()
    {
        // Check if the LightDoor is active (meaning the sprite is visible)
        if (LightDoor.enabled)
        {
            // If the LightDoor is active, we turn it off
            LightDoor.enabled = false;

            // If the vine is not already moving, start moving it
            if (!isMoving)
            {
                StartCoroutine(MoveVine());
            }
        }
        else
        {
            // If the LightDoor is not active, we turn it on
            LightDoor.enabled = true;

            // Here you can stop the vine's movement if needed, or reverse its movement
            // StopAllCoroutines(); // This line would stop the vine from moving if uncommented
        }
    }

    private IEnumerator MoveVine()
    {
        isMoving = true; // Set the flag to indicate that the vine is moving
        Debug.Log("Starting to move vine towards " + targetPosition);

        // Continue moving the vine until it reaches close to the target position
        while (Vector3.Distance(vine.transform.position, targetPosition) > 0.01f)
        {
            vine.transform.position = Vector3.MoveTowards(vine.transform.position, targetPosition, speed * Time.deltaTime);
            yield return null;
        }

        // Snap the vine position to the target position
        vine.transform.position = targetPosition;
        Debug.Log("Vine reached the target position");
        isMoving = false; // Reset the moving flag
    }
}
