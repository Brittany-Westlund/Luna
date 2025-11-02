using UnityEngine;
using System.Collections;

public class HydrationReduceOnBrew_FinalFix : MonoBehaviour
{
    [Header("Hydration Icon Detection")]
    [Tooltip("Tag assigned to the hydration icon GameObject")]
    public string hydrationTag = "HydrationIcon";

    [Header("Scale Settings")]
    public float reductionAmount = 0.15f;
    public float minScale = 0.6f;
    public float shrinkSpeed = 2f;
    public float checkInterval = 0.25f;

    private Transform hydrationIcon;
    private int lastTeapotCount;
    private float timer;

    void Start()
    {
        // Attempt 1: find by tag
        var iconObj = GameObject.FindGameObjectWithTag(hydrationTag);
        if (iconObj == null)
        {
            // Attempt 2: find by name
            iconObj = GameObject.Find("HydrationIcon");
        }

        if (iconObj != null)
        {
            hydrationIcon = iconObj.transform;
            Debug.Log($"💧 Found HydrationIcon object: {iconObj.name} | scale={hydrationIcon.localScale}");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find HydrationIcon in scene by tag or name!");
        }

        lastTeapotCount = FindObjectsOfType<TeapotLightReceiver>().Length;
        Debug.Log($"🫖 Starting with {lastTeapotCount} teapots in scene.");
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        var teapots = FindObjectsOfType<TeapotLightReceiver>();
        int countNow = teapots.Length;

        if (countNow < lastTeapotCount)
        {
            Debug.Log($"🍵 Detected teapot destroyed (from {lastTeapotCount} to {countNow}) — reducing hydration.");
            ReduceHydration();
        }

        lastTeapotCount = countNow;
    }

    private void ReduceHydration()
    {
        if (hydrationIcon == null)
        {
            // Try again (scene might have loaded it later)
            var retry = GameObject.FindGameObjectWithTag(hydrationTag) ?? GameObject.Find("HydrationIcon");
            if (retry != null)
            {
                hydrationIcon = retry.transform;
                Debug.Log($"💧 Found HydrationIcon late: {retry.name}");
            }
            else
            {
                Debug.LogWarning("❌ No HydrationIcon found when reducing hydration.");
                return;
            }
        }

        Vector3 current = hydrationIcon.localScale;
        Vector3 target = current - Vector3.one * reductionAmount;
        if (target.x < minScale) target = Vector3.one * minScale;

        Debug.Log($"💧 Shrinking from {current} → {target}");
        StopAllCoroutines();
        StartCoroutine(ShrinkSmoothly(current, target));
    }

    private IEnumerator ShrinkSmoothly(Vector3 from, Vector3 to)
    {
        float t = 0f;
        while (Vector3.Distance(hydrationIcon.localScale, to) > 0.01f)
        {
            t += Time.deltaTime * shrinkSpeed;
            hydrationIcon.localScale = Vector3.Lerp(from, to, t);
            yield return null;
        }
        hydrationIcon.localScale = to;
        Debug.Log($"✅ Finished shrinking. New scale: {hydrationIcon.localScale}");
    }
}
