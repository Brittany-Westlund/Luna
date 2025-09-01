using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class MovingPlatformController : MonoBehaviour
{
    [Header("Platform Settings")]
    public MovingPlatform MovingPlatform; // Reference to the MovingPlatform script
    public float DelayBeforeContinuing = 3f; // Time to wait before resuming movement after the player leaves
    public bool ContinueAfterPlayerLeaves = true; // Should the platform continue after the player leaves?

    private bool _playerOnPlatform = false; // Tracks if the player is on the platform

    private void Start()
    {
        // Ensure the MovingPlatform reference is set
        if (MovingPlatform == null)
        {
            MovingPlatform = GetComponent<MovingPlatform>();
        }

        if (MovingPlatform == null)
        {
            Debug.LogError("MovingPlatformController: No MovingPlatform component found on this GameObject!");
        }
    }

    /// <summary>
    /// Called when the player enters the platform
    /// </summary>
    public void PlayerEntered()
    {
        Debug.Log("Player entered the platform.");
        _playerOnPlatform = true;

        // Stop any existing coroutine to ensure it doesn't interfere
        StopAllCoroutines();
    }

    /// <summary>
    /// Called when the player exits the platform
    /// </summary>
    public void PlayerExited()
    {
        Debug.Log("Player exited the platform.");
        _playerOnPlatform = false;

        // If the platform should continue, start the delay coroutine
        if (ContinueAfterPlayerLeaves)
        {
            Debug.Log("Starting coroutine to resume movement.");
            StartCoroutine(ResumeAfterDelay());
        }
    }

    /// <summary>
    /// Coroutine to resume platform movement after a delay
    /// </summary>
    private IEnumerator ResumeAfterDelay()
    {
        Debug.Log($"Waiting for {DelayBeforeContinuing} seconds before resuming.");
        yield return new WaitForSeconds(DelayBeforeContinuing);

        // If the player hasn't returned, resume the platform's movement
        if (!_playerOnPlatform && MovingPlatform != null)
        {
            Debug.Log("Player hasn't returned. Resuming platform movement.");

            // Authorize movement and reset any "end reached" state
            MovingPlatform.AuthorizeMovement();
            MovingPlatform.ResetEndReached();

            Debug.Log("Platform movement authorized and end reset.");
        }
    }
}
