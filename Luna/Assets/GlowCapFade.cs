using UnityEngine;
using System.Collections;

public class GlowCapFade : MonoBehaviour
{
    [Header("References")]
    public LunaRest lunaRest;              // Drag Luna (has LunaRest)
    public SpriteRenderer glowCapRenderer; // Drag GlowCaps (Caps_5)

    [Header("Timing")]
    [Tooltip("Time before the first fade begins after Luna starts resting.")]
    public float delayBeforeFade = 0.5f;
    [Tooltip("Duration for a full fade-in + fade-out cycle.")]
    public float fadeDuration = 1.5f;
    [Tooltip("How many fade cycles to play while resting. Set to 0 for infinite looping.")]
    public int fadeCycles = 1;

    [Header("Visuals")]
    [Range(0, 1)] public float maxAlpha = 0.8f;
    [Range(0, 1)] public float minAlpha = 0f;

    private bool hasStarted = false;
    private Coroutine fadeRoutine;

    void Update()
    {
        if (lunaRest == null || glowCapRenderer == null) return;

        // 💤 Luna begins resting → start coroutine
        if (lunaRest.isResting && !hasStarted)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeWithRepeats());
            hasStarted = true;
        }
        // 🌕 Luna stops resting → reset
        else if (!lunaRest.isResting && hasStarted)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            SetAlpha(minAlpha);
            hasStarted = false;
        }
    }

    private IEnumerator FadeWithRepeats()
    {
        if (delayBeforeFade > 0)
            yield return new WaitForSeconds(delayBeforeFade);

        int cyclesDone = 0;
        while (fadeCycles == 0 || cyclesDone < fadeCycles)
        {
            yield return StartCoroutine(FadeInOutOnce());
            cyclesDone++;

            // stop if Luna stops resting mid-way
            if (!lunaRest.isResting)
                break;
        }

        SetAlpha(minAlpha);
    }

    private IEnumerator FadeInOutOnce()
    {
        // Fade In
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (fadeDuration / 2f);
            SetAlpha(Mathf.Lerp(minAlpha, maxAlpha, t));
            yield return null;
        }

        // Fade Out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / (fadeDuration / 2f);
            SetAlpha(Mathf.Lerp(maxAlpha, minAlpha, t));
            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        Color c = glowCapRenderer.color;
        c.a = a;
        glowCapRenderer.color = c;
    }
}
