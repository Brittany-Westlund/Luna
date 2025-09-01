using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class MovingPlatformController : MonoBehaviour
{
    // Reference to the MovingPlatform script
    public MovingPlatform MovingPlatform;

    // Time to wait before resuming movement after the player leaves
    public float DelayBeforeContinuing = 3f;

    // Should the platform resume moving after the player leaves?
    public bool ContinueAfterPlayerLeaves = true;

    // Tracks if the player is currently on the platform
    private bool _playerOnPlatform = false;

    private void Start()
    {
        // Ensure the MovingPlatform reference is set
        if (MovingPlatform == null)
        {
            MovingPlatform = GetComponent<MovingPlatform>();
        }
    }

    /// <summary>
    /// Called when the player steps onto the platform
    /// </summary>
    public void PlayerEntered()
    {
        _playerOnPlatform = true;

        // Optional: Cancel any existing coroutine to continue moving
        StopAllCoroutines();
    }

    /// <summary>
    /// Called when the player leaves the platform
    /// </summary>
    public void PlayerExited()
    {
        _playerOnPlatform = false;

        // If configured to continue moving, start the delay coroutine
        if (ContinueAfterPlayerLeaves)
        {
            StartCoroutine(ResumeAfterDelay());
        }
    }

    /// <summary>
    /// Coroutine to resume platform movement after a delay
    /// </summary>
    private IEnumerator ResumeAfterDelay()
    {
        yield return new WaitForSeconds(DelayBeforeContinuing);

        // If the player hasn't returned, allow the platform to continue moving
        if (!_playerOnPlatform)
        {
            MovingPlatform.AuthorizeMovement();
        }
    }
}
