using UnityEngine;
using System.Collections;

public class TeaHydrationInputBlocker : MonoBehaviour
{
    [Header("Hydration Settings")]
    public float minHydrationScale = 0.6f;
    public string hydrationTag = "HydrationIcon";

    [Header("Input Settings")]
    public KeyCode teaKey = KeyCode.T;

    [Header("Hint Settings")]
    public string lilystoolTag = "LilyStool";
    public string dropHintName = "DropHint";
    public float hintRadius = 3f;
    public float hintDuration = 1.5f;
    public float pulseSpeed = 2f;

    [Header("Debug")]
    public bool debugLogs = false;

    private Transform hydrationIcon;
    private bool isHintActive = false;
    private bool inputBlockedThisFrame = false;
    private float lastTeacupLostTime = -10f;
    private const float drinkGracePeriod = 0.4f; // seconds after losing teacup before allowing hydration hint

    private TeacupInventory teacupInventory; // detect if Luna already has a teacup

    void Start()
    {
        hydrationIcon = GameObject.FindGameObjectWithTag(hydrationTag)?.transform;
        if (hydrationIcon == null)
            hydrationIcon = GameObject.Find("HydrationIcon")?.transform;

        teacupInventory = GetComponent<TeacupInventory>();
    }

    void Update()
{
    // prevent recursive blocks
    if (inputBlockedThisFrame)
    {
        inputBlockedThisFrame = false;
        return;
    }

    if (Input.GetKeyDown(teaKey))
    {
        bool currentlyHolding = IsHoldingTeacup();

        // Track when Luna *just* lost her cup (drank or gave it away)
        if (!currentlyHolding && lastTeacupLostTime < 0)
            lastTeacupLostTime = Time.time; // first frame without a cup

        // If she’s holding a cup, reset this timer entirely
        if (currentlyHolding)
        {
            lastTeacupLostTime = -10f;
            return; // skip hydration logic while holding a cup
        }

        // 🚫 Skip hydration check if we just lost the teacup (grace period)
        if (Time.time - lastTeacupLostTime < drinkGracePeriod)
        {
            if (debugLogs)
                Debug.Log("[TeaHydrationInputBlocker] Grace period after drinking—no hydration hint yet.");
            return;
        }

        // ✅ Finally: perform hydration gate
        if (!HasEnoughHydration())
        {
            if (debugLogs)
                Debug.Log("🚫 [TeaHydrationInputBlocker] Too little hydration to brew.");
            StartCoroutine(BlockTeaInputThisFrame());
            StartCoroutine(PulseNearestLilystoolHint());
        }
    }
}


    private bool IsHoldingTeacup()
    {
        return teacupInventory != null && teacupInventory.HasTeacup();
    }

    private IEnumerator BlockTeaInputThisFrame()
    {
        inputBlockedThisFrame = true;
        yield return null;
    }

    private bool HasEnoughHydration()
    {
        if (hydrationIcon == null) return true;
        return hydrationIcon.localScale.x >= minHydrationScale;
    }

    public IEnumerator PulseNearestLilystoolHint()
    {
        if (isHintActive) yield break;
        isHintActive = true;

        GameObject nearest = null;
        float minDist = Mathf.Infinity;

        foreach (var stool in GameObject.FindGameObjectsWithTag(lilystoolTag))
        {
            float d = Vector2.Distance(transform.position, stool.transform.position);
            if (d < hintRadius && d < minDist)
            {
                nearest = stool;
                minDist = d;
            }
        }

        if (nearest != null)
        {
            Transform dropHint = nearest.transform.Find(dropHintName);
            if (dropHint != null)
            {
                dropHint.gameObject.SetActive(true);
                Vector3 baseScale = dropHint.localScale;
                float timer = 0f;

                while (timer < hintDuration)
                {
                    timer += Time.deltaTime;
                    float pulse = (Mathf.Sin(Time.time * pulseSpeed) + 1f) / 2f;
                    float scale = Mathf.Lerp(1f, 1.3f, pulse);
                    dropHint.localScale = baseScale * scale;
                    yield return null;
                }

                dropHint.localScale = baseScale;
                dropHint.gameObject.SetActive(false);
            }
        }

        isHintActive = false;
    }

    public bool IsHydrationTooLow()
{
    if (hydrationIcon == null)
    {
        hydrationIcon = GameObject.Find("HydrationIcon")?.transform;
        if (hydrationIcon == null)
        {
            Debug.LogWarning("[TeaHydrationInputBlocker] HydrationIcon not found!");
            return false; // fail gracefully instead of blocking everything
        }
    }

    float currentScale = hydrationIcon.localScale.x;
    Debug.Log($"[TeaHydrationInputBlocker] Hydration scale check: {currentScale} (min {minHydrationScale})");
    return currentScale <= minHydrationScale;
}


}
