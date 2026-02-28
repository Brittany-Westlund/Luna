using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class MystMoonbowRevealOnce : MonoBehaviour
{
    [Header("Auto-generated per mist instance; do not edit")]
    [SerializeField] private string locationId;

    [Header("Detection")]
    public float alphaThreshold = 0.15f;

    [Header("References (optional; will auto-find if empty)")]
    public MystRestTransitionAuto mistTransition;

    [Tooltip("NEW book controller (preferred)")]
    public BookControllerSimple bookSimple;

    [Tooltip("OLD book controller (optional fallback)")]
    public BookPageController bookLegacy;

    [Header("Polling")]
    public float pollInterval = 0.1f;

    [Header("Debug")]
    public bool debugLogs = false;

    private float nextPollTime;
    private bool didReveal;

    void Reset()
    {
        EnsureId();
        AutoWire();
    }

    void OnValidate()
    {
        EnsureId();
    }

    void Awake()
    {
        EnsureId();
        AutoWire();
    }

    void Update()
    {
        if (didReveal) return;

        if (Time.time < nextPollTime) return;
        nextPollTime = Time.time + pollInterval;

        if (mistTransition == null)
            mistTransition = GetComponent<MystRestTransitionAuto>();

        ResolveBookRefs();

        if (mistTransition == null) return;

        // Only trigger when moonbow sprite is active and visible enough
        var moonbowRenderer = mistTransition.moonbowRenderer;
        if (moonbowRenderer == null) return;
        if (!moonbowRenderer.gameObject.activeInHierarchy) return;

        float a = moonbowRenderer.color.a;
        if (a < alphaThreshold) return;

        bool success = Reveal();

        if (debugLogs)
            Debug.Log($"📖 {name}: Reveal attempt. Success={success}. LocationId={locationId} (alpha={a:0.00})");

        // IMPORTANT: only latch + disable if SUCCESS.
        if (success)
        {
            didReveal = true;
            enabled = false;
        }
    }

    bool Reveal()
    {
        // Prefer new controller
        if (bookSimple != null)
            return bookSimple.RevealNextFromLocation(locationId);

        // Optional fallback to legacy
        if (bookLegacy != null)
            return bookLegacy.RevealNextFromLocation(locationId);

        return false;
    }

    void ResolveBookRefs()
    {
        // Prefer new controller
        if (bookSimple == null)
            bookSimple = FindFirstObjectByTypeCompat<BookControllerSimple>();

        // Legacy fallback
        if (bookLegacy == null && BookPageController.Instance != null)
            bookLegacy = BookPageController.Instance;

        if (bookLegacy == null)
            bookLegacy = FindFirstObjectByTypeCompat<BookPageController>();
    }

    void AutoWire()
    {
        if (mistTransition == null)
            mistTransition = GetComponent<MystRestTransitionAuto>();

        ResolveBookRefs();
    }

    void EnsureId()
    {
        if (!string.IsNullOrEmpty(locationId)) return;

        // Stable-ish unique ID per scene instance serialized into the component
        locationId = System.Guid.NewGuid().ToString("N");

#if UNITY_EDITOR
        if (!Application.isPlaying)
            EditorUtility.SetDirty(this);
#endif
    }

    // -------------------------
    // Unity version compatibility helper
    // -------------------------
    static T FindFirstObjectByTypeCompat<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }
}