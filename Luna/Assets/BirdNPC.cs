using System.Collections;
using UnityEngine;

public class BirdNPC : MonoBehaviour
{
    [Header("Bird Visuals")]
    [SerializeField] private GameObject birdUpright;
    [SerializeField] private GameObject birdFlying;

    [Header("Flight Settings")]
    [SerializeField] private float flyDistance = 3f;
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private float waitBeforeReturn = 1.5f;
    [SerializeField] private int flyDirection = 1; // 1 = move right first, -1 = move left first

    [Header("Collision")]
    [SerializeField] private string lunaTag = "Player";

    private Vector3 startPosition;
    private Vector3 flyingStartScale;
    private bool isBusy = false;

    private void Start()
    {
        startPosition = transform.position;

        if (birdFlying != null)
        {
            flyingStartScale = birdFlying.transform.localScale;
        }

        SetUprightState();
        ResetFlyingScale();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBusy)
            return;

        if (!other.CompareTag(lunaTag))
            return;

        StartCoroutine(FlyAwayAndReturn());
    }

    private IEnumerator FlyAwayAndReturn()
    {
        isBusy = true;

        // Switch to flying
        SetFlyingState();

        // Face in the initial travel direction relative to whatever scale was already set in scene
        SetFlyingFacing(true);

        // Move away
        Vector3 awayTarget = startPosition + new Vector3(flyDistance * flyDirection, 0f, 0f);
        yield return StartCoroutine(MoveToPosition(awayTarget));

        // Wait
        yield return new WaitForSeconds(waitBeforeReturn);

        // Turn around visually
        SetFlyingFacing(false);

        // Return
        yield return StartCoroutine(MoveToPosition(startPosition));

        // Switch back to upright and restore original scale
        SetUprightState();
        ResetFlyingScale();

        isBusy = false;
    }

    private IEnumerator MoveToPosition(Vector3 targetPosition)
    {
        while (Vector3.Distance(transform.position, targetPosition) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                targetPosition,
                flySpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = targetPosition;
    }

    private void SetUprightState()
    {
        if (birdUpright != null)
            birdUpright.SetActive(true);

        if (birdFlying != null)
            birdFlying.SetActive(false);
    }

    private void SetFlyingState()
    {
        if (birdUpright != null)
            birdUpright.SetActive(false);

        if (birdFlying != null)
            birdFlying.SetActive(true);
    }

    private void SetFlyingFacing(bool outward)
    {
        if (birdFlying == null)
            return;

        Vector3 scale = flyingStartScale;

        // Preserve the original sign you set in the scene.
        // outward = original facing
        // return = opposite of original facing
        if (outward)
        {
            scale.x = flyingStartScale.x;
        }
        else
        {
            scale.x = -flyingStartScale.x;
        }

        birdFlying.transform.localScale = scale;
    }

    private void ResetFlyingScale()
    {
        if (birdFlying == null)
            return;

        birdFlying.transform.localScale = flyingStartScale;
    }
}