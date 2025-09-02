// Does not work

using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class MovingPlatformController : MonoBehaviour
{
    public MovingPlatform MovingPlatform;

    public float DelayBeforeContinuing = 3f;
    public bool ContinueAfterPlayerLeaves = true;

    private void Start()
    {
        if (MovingPlatform == null)
        {
            MovingPlatform = GetComponent<MovingPlatform>();
        }

        if (MovingPlatform == null)
        {
            Debug.LogError("No MovingPlatform component found!");
        }
        else
        {
            Debug.Log("MovingPlatform component successfully assigned.");
        }
    }

    public void PlayerEntered()
    {
        Debug.Log("Player entered the platform.");
    }

    public void PlayerExited()
    {
        Debug.Log("Player exited the platform.");

        if (MovingPlatform != null)
        {
            // Clear the colliding controller to bypass CanMove restrictions
            SetCollidingController(null);
        }

        if (ContinueAfterPlayerLeaves)
        {
            Debug.Log("Starting coroutine to resume movement.");
            StartCoroutine(ResumeAfterDelay());
        }
    }

    private IEnumerator ResumeAfterDelay()
    {
        Debug.Log($"Waiting for {DelayBeforeContinuing} seconds before resuming.");
        yield return new WaitForSeconds(DelayBeforeContinuing);

        if (MovingPlatform != null)
        {
            Debug.Log("Forcing platform to move.");

            // Manipulate MovingPlatform state to allow movement
            MovingPlatform.ScriptActivated = true;  // Ensure ScriptActivated is enabled
            MovingPlatform.AuthorizeMovement();    // Allow movement via the public method

            // Reset any "end reached" state
            MovingPlatform.ResetEndReached();

            // Force movement by calling MoveAlongThePath()
            Debug.Log("Directly calling MoveAlongThePath.");
            MovingPlatform.MoveAlongThePath();
        }
        else
        {
            Debug.LogError("MovingPlatform reference is null!");
        }
    }

    private void SetCollidingController(CorgiController controller)
    {
        var field = typeof(MovingPlatform).GetField("_collidingController", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (field != null)
        {
            field.SetValue(MovingPlatform, controller);
            Debug.Log("Successfully set _collidingController.");
        }
        else
        {
            Debug.LogError("Failed to set _collidingController: Field not found.");
        }
    }
}
