using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer))]
public class FairyflyLocalFlowerPatrol : MonoBehaviour
{
    [Header("Assigned Flowers (only these will be checked)")]
    [SerializeField] private List<SproutAndLightManager> assignedFlowers = new List<SproutAndLightManager>();

    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 0.6f;
    [SerializeField] private float patrolSpeed = 1.25f;
    [SerializeField] private float patrolPauseAtEnds = 0.15f;
    [SerializeField] private bool useStartPositionAsCenter = true;
    [SerializeField] private Transform patrolCenterOverride;

    [Header("Flower Response")]
    [SerializeField] private float flowerApproachHeightOffset = 0.35f;
    [SerializeField] private float flowerMoveSpeed = 1.8f;
    [SerializeField] private float stayAtFlowerTime = 1f;
    [SerializeField] private float flowerCheckInterval = 0.2f;

    [Header("Floating Motion")]
    [SerializeField] private float floatAmplitude = 0.08f;
    [SerializeField] private float floatFrequency = 2f;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Optional")]
    [SerializeField] private bool debugLogs = false;

    private SpriteRenderer spriteRenderer;

    private Vector3 patrolCenter;
    private Vector3 patrolLeft;
    private Vector3 patrolRight;

    // Position before the bobbing/floating visual offset is applied in Update().
    private Vector3 visualBasePosition;

    private Coroutine patrolRoutine;
    private Coroutine monitorRoutine;

    private bool movingRight = true;
    private bool busy = false;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (useStartPositionAsCenter || patrolCenterOverride == null)
            patrolCenter = transform.position;
        else
            patrolCenter = patrolCenterOverride.position;

        patrolLeft = patrolCenter + Vector3.left * patrolDistance;
        patrolRight = patrolCenter + Vector3.right * patrolDistance;

        visualBasePosition = transform.position;

        patrolRoutine = StartCoroutine(PatrolRoutine());
        monitorRoutine = StartCoroutine(MonitorAssignedFlowers());
    }

    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (busy)
            {
                yield return null;
                continue;
            }

            Vector3 target = movingRight ? patrolRight : patrolLeft;

            yield return StartCoroutine(MoveBaseTo(target, patrolSpeed));

            if (busy)
                continue;

            if (patrolPauseAtEnds > 0f)
                yield return new WaitForSeconds(patrolPauseAtEnds);

            movingRight = !movingRight;
        }
    }

    private IEnumerator MonitorAssignedFlowers()
    {
        WaitForSeconds wait = new WaitForSeconds(flowerCheckInterval);

        while (true)
        {
            if (!busy)
            {
                SproutAndLightManager targetFlower = GetNextEligibleFlower();
                if (targetFlower != null)
                {
                    yield return StartCoroutine(HandleFlower(targetFlower));
                }
            }

            yield return wait;
        }
    }

    private SproutAndLightManager GetNextEligibleFlower()
    {
        for (int i = 0; i < assignedFlowers.Count; i++)
        {
            SproutAndLightManager flower = assignedFlowers[i];

            if (flower == null)
                continue;

            if (!flower.IsFullyGrown)
                continue;

            if (IsFlowerAlreadyLit(flower))
                continue;

            return flower;
        }

        return null;
    }

    private bool IsFlowerAlreadyLit(SproutAndLightManager flower)
    {
        if (flower == null)
            return true;

        if (flower.litFlowerRenderer != null && flower.litFlowerRenderer.enabled)
            return true;

        Transform litChild = flower.transform.Find("LitFlowerB");
        if (litChild != null && litChild.gameObject.activeSelf)
        {
            SpriteRenderer litSR = litChild.GetComponent<SpriteRenderer>();
            if (litSR == null || litSR.enabled)
                return true;
        }

        return false;
    }

    private IEnumerator HandleFlower(SproutAndLightManager flower)
    {
        if (busy || flower == null)
            yield break;

        busy = true;

        Vector3 returnPoint = visualBasePosition;
        Vector3 flowerTarget = flower.transform.position + new Vector3(0f, flowerApproachHeightOffset, 0f);

        if (debugLogs)
            Debug.Log($"{name}: Flying to assigned flower {flower.name}");

        yield return StartCoroutine(MoveBaseTo(flowerTarget, flowerMoveSpeed));

        ForceIlluminateFlower(flower);

        yield return new WaitForSeconds(stayAtFlowerTime);

        yield return StartCoroutine(MoveBaseTo(returnPoint, flowerMoveSpeed));

        busy = false;
    }

    private void ForceIlluminateFlower(SproutAndLightManager flower)
    {
        if (flower == null)
            return;

        // Turn on the explicit lit child if present.
        Transform lit = flower.transform.Find("LitFlowerB");
        if (lit != null)
        {
            lit.gameObject.SetActive(true);

            SpriteRenderer litRenderer = lit.GetComponent<SpriteRenderer>();
            if (litRenderer != null)
                litRenderer.enabled = true;

            LitFlowerBForceOn forceOn = lit.GetComponent<LitFlowerBForceOn>();
            if (forceOn != null)
                forceOn.enabled = true;
        }

        // Also turn on the manager-assigned lit renderer if present.
        if (flower.litFlowerRenderer != null)
        {
            flower.litFlowerRenderer.gameObject.SetActive(true);
            flower.litFlowerRenderer.enabled = true;
        }

        // This is the important part:
        // let the flower itself finalize its lit state, hide the light hint,
        // and do any score/bookkeeping without requiring Luna to be nearby.
        flower.ForceGiveLightFromFairyfly();

        if (debugLogs)
            Debug.Log($"{name}: Forced illumination on {flower.name}");
    }

    private IEnumerator MoveBaseTo(Vector3 target, float speed)
    {
        Vector3 start = visualBasePosition;
        float distance = Vector3.Distance(start, target);

        if (distance <= 0.001f)
        {
            visualBasePosition = target;
            yield break;
        }

        float safeSpeed = Mathf.Max(speed, 0.01f);
        float duration = Mathf.Max(distance / safeSpeed, 0.01f);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float easedT = easeCurve.Evaluate(Mathf.Clamp01(t));
            visualBasePosition = Vector3.Lerp(start, target, easedT);
            yield return null;
        }

        visualBasePosition = target;
    }

    private void Update()
    {
        Vector3 pos = visualBasePosition;
        pos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = pos;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 center;

        if (Application.isPlaying)
        {
            center = patrolCenter;
        }
        else
        {
            if (!useStartPositionAsCenter && patrolCenterOverride != null)
                center = patrolCenterOverride.position;
            else
                center = transform.position;
        }

        Vector3 left = center + Vector3.left * patrolDistance;
        Vector3 right = center + Vector3.right * patrolDistance;

        Gizmos.color = new Color(0.8f, 1f, 1f, 0.75f);
        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.05f);
        Gizmos.DrawSphere(right, 0.05f);

        Gizmos.color = new Color(1f, 0.95f, 0.5f, 0.6f);
        Gizmos.DrawSphere(center, 0.04f);
    }
}