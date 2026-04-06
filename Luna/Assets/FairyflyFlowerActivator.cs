using UnityEngine;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
[RequireComponent(typeof(FairyflyMovement))]
public class FairyflyCoordinator : MonoBehaviour
{
    [Header("Flower Activation")]
    public float checkInterval = 0.5f;
    public float pauseAtFlower = 0.25f;
    public float minimumMoveDuration = 0.6f;
    public float movementSpeedDivisor = 1.0f;
    public bool debugLogs = false;

    [Header("Events")]
    public UnityEvent onFlowerLit;

    private FairyflyMovement fairyflyMovement;
    private bool isMoving = false;

    private void Start()
    {
        fairyflyMovement = GetComponent<FairyflyMovement>();

        if (fairyflyMovement == null)
        {
            Debug.LogWarning("FairyflyCoordinator: FairyflyMovement not found on " + gameObject.name);
            enabled = false;
            return;
        }

        StartCoroutine(MonitorFlowers());
    }

    private IEnumerator MonitorFlowers()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);

            if (isMoving)
            {
                continue;
            }

            SproutAndLightManager[] flowers = FindObjectsOfType<SproutAndLightManager>();

            for (int i = 0; i < flowers.Length; i++)
            {
                SproutAndLightManager flower = flowers[i];

                if (flower == null)
                {
                    continue;
                }

                if (!flower.IsFullyGrown)
                {
                    continue;
                }

                if (IsFlowerAlreadyLit(flower))
                {
                    continue;
                }

                if (debugLogs)
                {
                    Debug.Log("FairyflyCoordinator: Found unlit grown flower: " + flower.name);
                }

                yield return StartCoroutine(FlyToFlowerAndContinue(flower));
                break;
            }
        }
    }

    private bool IsFlowerAlreadyLit(SproutAndLightManager flower)
    {
        if (flower == null)
        {
            return true;
        }

        if (flower.litFlowerRenderer != null)
        {
            return flower.litFlowerRenderer.enabled;
        }

        Transform lit = flower.transform.Find("LitFlowerB");
        if (lit != null)
        {
            SpriteRenderer litRenderer = lit.GetComponent<SpriteRenderer>();
            if (litRenderer != null)
            {
                return litRenderer.enabled;
            }
        }

        return false;
    }

    private IEnumerator FlyToFlowerAndContinue(SproutAndLightManager flower)
    {
        if (isMoving)
        {
            yield break;
        }

        if (flower == null)
        {
            yield break;
        }

        isMoving = true;

        Vector3 startPos = transform.position;
        Vector3 targetPos = flower.transform.position;

        float distance = Vector3.Distance(startPos, targetPos);
        float moveDuration = Mathf.Max(distance / Mathf.Max(0.01f, movementSpeedDivisor), minimumMoveDuration);

        float t = 0f;

        if (debugLogs)
        {
            Debug.Log("FairyflyCoordinator: Flying to flower " + flower.name);
        }

        while (t < 1f)
        {
            if (flower == null)
            {
                if (debugLogs)
                {
                    Debug.LogWarning("FairyflyCoordinator: Target flower disappeared mid-flight.");
                }

                isMoving = false;
                yield break;
            }

            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        // Turn on lit visuals
        if (flower.litFlowerRenderer != null)
        {
            flower.litFlowerRenderer.enabled = true;
        }

        Transform lit = flower.transform.Find("LitFlowerB");
        if (lit != null)
        {
            SpriteRenderer litRenderer = lit.GetComponent<SpriteRenderer>();
            if (litRenderer != null)
            {
                litRenderer.enabled = true;
            }

            LitFlowerBForceOn forceOn = lit.GetComponent<LitFlowerBForceOn>();
            if (forceOn != null)
            {
                forceOn.enabled = true;
            }
        }

        // Core logic
        flower.HideLightHint();
        flower.GiveLight();

        // ✅ NEW EVENT
        onFlowerLit?.Invoke();

        if (debugLogs)
        {
            Debug.Log("FairyflyCoordinator: Lit flower " + flower.name);
        }

        yield return new WaitForSeconds(pauseAtFlower);

        if (fairyflyMovement != null)
        {
            fairyflyMovement.skipWaitOnce = true;

            if (!fairyflyMovement.isMoving)
            {
                StartCoroutine(fairyflyMovement.MoveSequencePublic());
            }
        }

        isMoving = false;
    }
}