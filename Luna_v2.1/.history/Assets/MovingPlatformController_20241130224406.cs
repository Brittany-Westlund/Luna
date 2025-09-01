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
            MovingPlatform.ScriptActivated = true;
            MovingPlatform.AuthorizeMovement();
            MovingPlatform.ResetEndReached();
        }
    }
}
