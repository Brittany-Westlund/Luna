using UnityEngine;
using System.Collections;

public class LunaHydrationGrowOnMoonbow : MonoBehaviour
{
    [Header("References")]
    public MystRestTransitionAuto mystTransition;   // Link to your existing Myst script
    public Transform hydrationIcon;                 // The drop or hydration icon

    [Header("Growth Settings")]
    [Tooltip("How much to increase hydration icon’s scale each time Luna leaves a Moonbow.")]
    public float growthAmount = 0.2f;

    [Tooltip("Maximum allowed overall scale.")]
    public float maxScale = 1.5f;

    [Tooltip("Speed of growth animation.")]
    public float growthSpeed = 2f;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool _wasMoonbowVisible;
    private Vector3 _currentTargetScale;

    void Start()
    {
        if (hydrationIcon == null)
        {
            Debug.LogWarning($"💧 {name}: No HydrationIcon assigned!");
            enabled = false;
            return;
        }

        if (mystTransition == null)
            mystTransition = GetComponent<MystRestTransitionAuto>();

        _currentTargetScale = hydrationIcon.localScale; // remember starting size
    }

    void Update()
    {
        if (mystTransition == null || hydrationIcon == null) return;

        // detect if Moonbow is visible
        bool moonbowVisible = IsMoonbowVisible();

        // detect transition from visible → hidden (fade out)
        if (_wasMoonbowVisible && !moonbowVisible)
        {
            // Moonbow just faded out → hydrate Luna!
            GrowHydrationIcon();
        }

        _wasMoonbowVisible = moonbowVisible;
    }

    private bool IsMoonbowVisible()
    {
        if (mystTransition.moonbowRenderer == null)
            return false;

        return mystTransition.moonbowRenderer.gameObject.activeInHierarchy &&
               mystTransition.moonbowRenderer.color.a > 0.05f;
    }

    private void GrowHydrationIcon()
    {
        // Calculate new size but don’t exceed maxScale
        Vector3 newScale = hydrationIcon.localScale + Vector3.one * growthAmount;
        newScale = Vector3.Min(newScale, Vector3.one * maxScale);

        if (debugLogs)
            Debug.Log($"💧 {name}: Moonbow faded — increasing hydration to {newScale.x:F2}");

        StopAllCoroutines();
        StartCoroutine(GrowSmoothly(hydrationIcon.localScale, newScale));
        _currentTargetScale = newScale;
    }

    private IEnumerator GrowSmoothly(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (Vector3.Distance(hydrationIcon.localScale, to) > 0.01f)
        {
            t += Time.deltaTime * growthSpeed;
            hydrationIcon.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        hydrationIcon.localScale = to;
    }
}
