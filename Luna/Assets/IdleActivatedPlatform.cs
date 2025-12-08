using UnityEngine;
using MoreMountains.CorgiEngine;
using System.Collections;

public class ElevatorIdleDelayController : MonoBehaviour
{
    public float idleDelay = 1.5f;

    private MovingPlatform platform;
    private bool playerOnPlatform = false;
    private bool isWaiting = false;
    private bool isLocked = true;

    private Coroutine waitRoutine;

    void Start()
    {
        platform = GetComponent<MovingPlatform>();

        // ✅ Always start locked
        platform.ForbidMovement();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerOnPlatform = true;

        // ✅ Only start timer if we're currently locked
        if (isLocked && !isWaiting)
        {
            waitRoutine = StartCoroutine(IdleThenMove());
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerOnPlatform = false;

        // ✅ Cancel pending movement if they leave early
        if (waitRoutine != null)
        {
            StopCoroutine(waitRoutine);
            waitRoutine = null;
        }

        // ✅ Re-arm the platform ONLY after they exit
        isLocked = true;
    }

    IEnumerator IdleThenMove()
    {
        isWaiting = true;

        yield return new WaitForSeconds(idleDelay);

        if (playerOnPlatform)
        {
            isLocked = false;
            platform.AuthorizeMovement(); // ✅ MOVE ONCE
        }

        isWaiting = false;
    }

    // ✅ This is called by the platform when it reaches a point
    public void ForceStopAtPoint()
    {
        platform.ForbidMovement();  // ✅ STOP
        isLocked = true;            // ✅ REQUIRE EXIT + REENTER
    }
}
