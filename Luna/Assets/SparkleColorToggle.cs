/* using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class SparkleColorToggle : MonoBehaviour
{
    [Header("Collisions")]
    public string playerTag = "Player";

    [Header("Effects")]
    public float fadeDuration = 0.3f;

    [Header("Debug")]
    public bool logDebug = false;

    private SpriteRenderer sr;
    private Color originalColor;
    private Coroutine fadeRoutine;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;

        var col = GetComponent<Collider2D>();
        if (col != null && !col.isTrigger)
            col.isTrigger = true;

        if (logDebug) Debug.Log($"[{name}] Original color = {originalColor}");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToColor(Color.white));

        if (logDebug) Debug.Log($"[{name}] Fading to WHITE by {other.name}");
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeToColor(originalColor));

        if (logDebug) Debug.Log($"[{name}] Fading back to {originalColor} after {other.name} left");
    }

    private IEnumerator FadeToColor(Color target)
    {
        Color start = sr.color;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / fadeDuration;
            sr.color = Color.Lerp(start, target, t);
            yield return null;
        }

        sr.color = target;
        fadeRoutine = null;
    }
} */
