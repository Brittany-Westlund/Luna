// RestBubbleShowFromRegistry.cs
using UnityEngine;
using System.Collections;

public class RestBubbleShowFromRegistry : MonoBehaviour
{
    [Header("Links")]
    public ThoughtBubbleRegistry registry;   // assign on LunaResting
    public SpriteRenderer lunaSprite;        // optional; only for sorting bounds

    [Header("Which bubble to use if nothing is set")]
    public string fallbackKey = "default";

    [Header("Placement")]
    public Transform bubbleParent;           // usually the same as registry.parentBubble parent (LunaResting)
    public Vector3 localOffset = new Vector3(0f, 1.1f, 0f);

    [Header("Size")]
    public float targetWorldHeight = 0.8f;   // scales the visual child uniformly

    [Header("Fade")]
    public float fadeIn = 0.15f;
    public float fadeOut = 0.12f;

    [Header("Sorting (optional)")]
    public string sortingLayerName = "Foreground";
    public int sortingOrder = 20;

    Transform activeRoot;
    SpriteRenderer[] activeRenderers;

    void OnEnable()
    {
        if (!registry) registry = GetComponent<ThoughtBubbleRegistry>();
        if (!bubbleParent) bubbleParent = transform;

        registry.HideAll();

        // choose key via manager
        string key = ThoughtBubbleManager.Instance
            ? ThoughtBubbleManager.Instance.ResolveKey(fallbackKey)
            : fallbackKey;

        activeRoot = registry.Get(key);
        if (!activeRoot) return;

        // position and ensure correct parent
        activeRoot.SetParent(bubbleParent, worldPositionStays: true);
        activeRoot.localPosition = localOffset;
        activeRoot.localRotation = Quaternion.identity;

        // prep renderers
        activeRenderers = activeRoot.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in activeRenderers)
        {
            r.enabled = true;
            r.drawMode = SpriteDrawMode.Simple;
            if (!string.IsNullOrEmpty(sortingLayerName)) r.sortingLayerName = sortingLayerName;
            r.sortingOrder = sortingOrder;
            var c = r.color; c.a = 0f; r.color = c;
        }

        // size uniformly based on first SR
        ApplyUniformWorldHeight(activeRoot, targetWorldHeight);

        // fade in
        StartCoroutine(FadeTo(1f, fadeIn));
    }

    void LateUpdate()
    {
        if (activeRoot)
        {
            activeRoot.localPosition = localOffset; // keep pinned
        }
    }

    void OnDisable()
    {
        StopAllCoroutines();
        // no StartCoroutine here — we're disabling!
        if (activeRoot && registry) registry.HideAll();
        activeRoot = null;
        activeRenderers = null;
    }

    void ApplyUniformWorldHeight(Transform bubbleRoot, float targetH)
    {
        var sr = bubbleRoot.GetComponentInChildren<SpriteRenderer>(true);
        if (!sr || !sr.sprite) return;

        float spriteWorldH = sr.sprite.bounds.size.y;
        if (spriteWorldH <= 0f) return;

        // scale the immediate visual so WORLD height == target
        Transform visual = sr.transform;
        Vector3 parentLossy = visual.parent ? visual.parent.lossyScale : Vector3.one;
        float sWorld = targetH / spriteWorldH;
        float sx = sWorld / Mathf.Max(parentLossy.x, 1e-6f);
        float sy = sWorld / Mathf.Max(parentLossy.y, 1e-6f);
        visual.localScale = new Vector3(sx, sy, 1f);
    }

    IEnumerator FadeTo(float targetA, float dur)
    {
        if (activeRenderers == null || activeRenderers.Length == 0 || dur <= 0f) yield break;
        float t = 0f;
        float from = activeRenderers[0].color.a;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(from, targetA, t / dur);
            for (int i = 0; i < activeRenderers.Length; i++)
            {
                var c = activeRenderers[i].color; c.a = a; activeRenderers[i].color = c;
            }
            yield return null;
        }
        for (int i = 0; i < activeRenderers.Length; i++)
        {
            var c = activeRenderers[i].color; c.a = targetA; activeRenderers[i].color = c;
        }
    }

    IEnumerator FadeOutAndHide()
    {
        yield return FadeTo(0f, fadeOut);
        if (activeRoot) registry.Show(activeRoot, false);
        activeRoot = null;
        activeRenderers = null;
    }
}
