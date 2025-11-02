using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class FairyflyCoordinator : MonoBehaviour
{
    [Header("Wait Points")]
    public Transform waitPoint1;
    public Transform waitPoint2;

    [Header("Motion")]
    public float moveSpeed = 2f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float floatAmplitude = 0.1f;
    public float floatFrequency = 2f;

    [Header("Flower Activation")]
    public float checkInterval = 0.5f;
    public float pauseAtFlower = 0.25f;
    public bool debugLogs = false;

    private bool isMoving = false;
    private bool waitingForLuna = true;
    private Transform currentWaitPoint;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        currentWaitPoint = waitPoint1 != null ? waitPoint1 : transform;
        transform.position = currentWaitPoint.position;

        StartCoroutine(MonitorFlowers());
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (isMoving) return;

        // When Luna collides, move to the opposite wait point and stay there
        var nextPoint = (currentWaitPoint == waitPoint1) ? waitPoint2 : waitPoint1;
        if (nextPoint != null)
        {
            StartCoroutine(MoveTo(nextPoint.position));
            currentWaitPoint = nextPoint;
        }
    }

    private IEnumerator MonitorFlowers()
    {
        while (true)
        {
            yield return new WaitForSeconds(checkInterval);
            if (isMoving) continue;

            // Look for any grown flower that isn’t lit
            var flowers = FindObjectsOfType<SproutAndLightManager>();
            foreach (var f in flowers)
            {
                if (f == null || !f.IsFullyGrown) continue;
                if (f.litFlowerRenderer == null || f.litFlowerRenderer.enabled) continue;

                // Found a target flower
                yield return StartCoroutine(FlyToFlowerAndBack(f));
                break;
            }
        }
    }

    private IEnumerator FlyToFlowerAndBack(SproutAndLightManager flower)
{
    isMoving = true;

    // Fly to flower
    yield return StartCoroutine(MoveTo(flower.transform.position));

    // 🌕 Activate flower visually
    var lit = flower.transform.Find("LitFlowerB");
    if (lit != null)
    {
        lit.gameObject.SetActive(true);

        var renderer = lit.GetComponent<SpriteRenderer>();
        if (renderer != null) renderer.enabled = true;

        var forceOn = lit.GetComponent<LitFlowerBForceOn>();
        if (forceOn != null) forceOn.enabled = true;

        // 🌟 Disable the light hint immediately
        flower.HideLightHint();

        // Optional: also mark as lit through GiveLight() so scoring stays consistent
        flower.GiveLight();

        if (debugLogs)
            Debug.Log($"🌕 Fairyfly lit {flower.name} and disabled its light hint");
    }

    yield return new WaitForSeconds(pauseAtFlower);

    // Return to last wait point
    yield return StartCoroutine(MoveTo(currentWaitPoint.position));

    isMoving = false;
}

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        Vector3 start = transform.position;
        float dist = Vector3.Distance(start, targetPos);
        float duration = Mathf.Max(dist / moveSpeed, 0.1f);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float easedT = easeCurve.Evaluate(t);
            Vector3 pos = Vector3.Lerp(start, targetPos, easedT);
            pos.y += Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
            transform.position = pos;
            yield return null;
        }

        transform.position = targetPos;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.9f, 0.6f, 0.5f);
        if (waitPoint1) Gizmos.DrawSphere(waitPoint1.position, 0.05f);
        if (waitPoint2) Gizmos.DrawSphere(waitPoint2.position, 0.05f);
    }
}
