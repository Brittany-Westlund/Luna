using UnityEngine;

public class LeverController : MonoBehaviour
{
    public WellVineMovement wellVineMovement; // Assign this in the Inspector

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered lever collider.");
            wellVineMovement.StartMovingDown();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited lever collider.");
            // Optionally add behavior when Luna steps off the lever
        }
    }
}
