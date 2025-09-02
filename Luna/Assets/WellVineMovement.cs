using UnityEngine;

public class WellVineMovement : MonoBehaviour
{
    public float bottomYPosition; // Y position when the well vine is at the bottom
    public float topYPosition;    // Y position when the well vine is at the top
    public SpriteRenderer waterRenderer; // Water sprite renderer to toggle

    public float moveSpeed = 2f;     // Speed at which the well vine moves
    public float delayAtBottom = 2f; // Delay at the bottom position before moving back up

    private bool isMoving = false;   // Flag to track if the well vine is currently moving
    private bool isFilling = false;  // Flag to track if the vine is currently filling with water

    void Start()
    {
        // Ensure the water renderer is disabled at start
        waterRenderer.enabled = false;
    }

    void Update()
    {
        if (isMoving)
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, bottomYPosition, transform.position.z), moveSpeed * Time.deltaTime);
            if (transform.position.y <= bottomYPosition)
            {
                // Enable water renderer only if not already filling
                if (!isFilling)
                {
                    waterRenderer.enabled = true;
                    isFilling = true;
                }
                Invoke("MoveBackUp", delayAtBottom);
            }
        }
        else
        {
            transform.position = Vector3.MoveTowards(transform.position, new Vector3(transform.position.x, topYPosition, transform.position.z), moveSpeed * Time.deltaTime);
            // Do not disable the water renderer when moving back up if it is filling
        }
    }

    void MoveBackUp()
    {
        isMoving = false;
    }

    public void StartMovingDown()
    {
        isMoving = true; // Start moving the well vine down
    }

    public void TurnOffWater()
    {
        waterRenderer.enabled = false;
        isFilling = false; // Reset the filling status
    }
}
