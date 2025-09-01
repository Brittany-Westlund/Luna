using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class MovingPlatformController : MonoBehaviour
{
    public MovingPlatform MovingPlatform; // Reference to the platform
    public float DelayBeforeContinuing = 3f; // Delay in seconds
    public bool ContinueAfterPlayerLeaves = true; // Toggle for behavior

    private bool _playerOnPlatform = false;

    private void Start()
    {
        // Ensure MovingPlatform reference is set
        if (MovingPlatform == null)
        {
            MovingPlatform = GetComponent<MovingPlatform>();
        }

        if (MovingPlatform == null)
        {
            Debug.LogError("MovingPlatformController: No MovingPlatform component found!");
        }
    }

    public void PlayerEntered()
    {
        Debug.Log("Player entered the platform.");
        _playerOnPlatform = true;
        StopAllCoroutines(); // Stop any ongoing coroutine
    }

    public void PlayerExited()
    {
        Debug.Log("Player exited the platform.");
        _playerOnPlatform = false;

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

        if (!_playerOnPlatform)
        {
            Debug.Log("Player hasn't returned. Resuming platform movement.");
            if (MovingPlatform != null)
            {
                MovingPlatform.AuthorizeMovement();
            }
            else
            {
                Debug.LogError("MovingPlatformController: MovingPlatform reference is missing!");
            }
        }
    }
}
