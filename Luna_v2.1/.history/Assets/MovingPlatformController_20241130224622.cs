using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class MovingPlatformController : MonoBehaviour
{
    [Header("Platform Settings")]
    public MovingPlatform MovingPlatform; // Reference to the MovingPlatform script
    public float DelayBeforeContinuing = 3f; // Time to wait before resuming movement after the player leaves
    public bool ContinueAfterPlayerLeaves = true; // Should the platform continue after the player leaves?

    private void Start()
    {
        if (MovingPlatform == null)
        {
            MovingPlatform = GetComponent<MovingPlatform>();
            Debug.Log("MovingPlatform reference set from the same GameObject.");
        }

        if (MovingPlatform == null)
        {
            Debug.LogError("No MovingPlatform component found on this GameObject!");
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
            MovingPlatform.ScriptActivated = true; // Explicitly activate the script
            MovingPlatform.AuthorizeMovement();   // Authorize movement
            MovingPlatform.ResetEndReached();     // Reset end status
            Debug.Log("Platform movement authorized and end reset.");
        }
        else
        {
            Debug.LogError("MovingPlatform reference is null!");
        }
    }
}
