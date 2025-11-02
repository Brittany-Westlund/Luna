using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class FairyflyMovement : MonoBehaviour
{
    [Header("Path Settings")]
    public List<Transform> destinations = new();
    public bool loop = false;
    public float pauseBeforeFade = 1f;

    [Header("Motion Settings")]
    public float moveSpeed = 1.5f;
    public float floatAmplitude = 0.15f;
    public float floatFrequency = 2f;
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Fade Settings")]
    public float fadeSpeed = 1f;
    public bool destroyOnFade = true;

    private SpriteRenderer sr;
    private int currentIndex = 0;
    private bool isMoving = false;
    private bool waitingForLuna = false;
    private bool playerInside = false;

    // Cache of world-space destinations
    private List<Vector3> worldDestinations = new();

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        // Cache destination positions in world space
        foreach (var d in destinations)
        {
            if (d != null)
                worldDestinations.Add(d.position);
        }

        if (worldDestinations.Count == 0)
            Debug.LogWarning($"{name}: No destinations assigned for FairyflyMovement.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;

        // If idle, start movement; if waiting, trigger next leg
        if (!isMoving)
            StartCoroutine(MoveSequence());
        else if (waitingForLuna)
            waitingForLuna = false;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInside = false;
    }

    private IEnumerator MoveSequence()
    {
        isMoving = true;

        while (currentIndex < worldDestinations.Count)
        {
            yield return StartCoroutine(MoveTo(worldDestinations[currentIndex]));

            // Stop and wait for Luna’s next touch
            waitingForLuna = true;
            bool mustExitFirst = playerInside;

            while (waitingForLuna)
            {
                if (!playerInside) mustExitFirst = false;
                yield return null;
            }

            currentIndex++;
        }

        yield return new WaitForSeconds(pauseBeforeFade);
        yield return StartCoroutine(FadeOutAndFinish());
    }

    private IEnumerator MoveTo(Vector3 targetPos)
    {
        Vector3 start = transform.position;
        float distance = Vector3.Distance(start, targetPos);
        float duration = distance / moveSpeed;

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
    }

    private IEnumerator FadeOutAndFinish()
    {
        Color c = sr.color;
        while (c.a > 0.05f)
        {
            c.a = Mathf.MoveTowards(c.a, 0f, Time.deltaTime * fadeSpeed);
            sr.color = c;
            yield return null;
        }

        if (destroyOnFade)
            Destroy(gameObject);
        else
            gameObject.SetActive(false);

        isMoving = false;
        waitingForLuna = false;
        currentIndex = 0;
    }

    // Optional: visualize flight path in Scene view
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.9f, 1f, 0.4f);
        for (int i = 0; i < destinations.Count - 1; i++)
        {
            if (destinations[i] != null && destinations[i + 1] != null)
                Gizmos.DrawLine(destinations[i].position, destinations[i + 1].position);
        }
    }
}
