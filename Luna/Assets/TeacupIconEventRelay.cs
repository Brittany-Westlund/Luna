using UnityEngine;
using UnityEngine.Events;

public class TeacupIconEventRelay : MonoBehaviour
{
    [Header("References")]
    public GameObject happyIcon;
    public GameObject cozyIcon;
    public TeacupReceiver teacupReceiver;

    [Header("Startup Behavior")]
    [Tooltip("If true, sync internal state to current icon states on Start without firing events.")]
    public bool syncSilentlyOnStart = true;

    [Tooltip("If true, missing icon refs will be auto-resolved from TeacupReceiver or child names.")]
    public bool autoResolveReferences = true;

    [Header("Happy Icon Events")]
    public UnityEvent onHappyShown;
    public UnityEvent onHappyHidden;

    [Header("Cozy Icon Events")]
    public UnityEvent onCozyShown;
    public UnityEvent onCozyHidden;

    [Header("Combined Events")]
    [Tooltip("Fires when either happy or cozy becomes visible.")]
    public UnityEvent onAnyIconShown;

    [Tooltip("Fires when both happy and cozy are hidden.")]
    public UnityEvent onAllIconsHidden;

    [Header("Conditional Events")]
    [Tooltip("Fires when happy is shown and cozy is NOT shown.")]
    public UnityEvent onHappyOnlyShown;

    [Tooltip("Fires when both happy and cozy are shown together.")]
    public UnityEvent onHappyAndCozyShown;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool lastHappyState = false;
    private bool lastCozyState = false;
    private bool hasInitialized = false;

    private void Awake()
    {
        ResolveReferences();
    }

    private void Start()
    {
        ResolveReferences();

        bool happyNow = IsVisible(happyIcon);
        bool cozyNow = IsVisible(cozyIcon);

        lastHappyState = happyNow;
        lastCozyState = cozyNow;
        hasInitialized = true;

        if (!syncSilentlyOnStart)
        {
            FireShowEventsForCurrentState(happyNow, cozyNow);
            FireHideEventsForCurrentState(happyNow, cozyNow);
        }
    }

    private void Update()
    {
        if (!hasInitialized)
            return;

        bool happyNow = IsVisible(happyIcon);
        bool cozyNow = IsVisible(cozyIcon);

        bool happyChanged = happyNow != lastHappyState;
        bool cozyChanged = cozyNow != lastCozyState;

        if (!happyChanged && !cozyChanged)
            return;

        if (debugLogs)
        {
            Debug.Log($"[TeacupIconEventRelay] {name} state change. Happy: {lastHappyState} -> {happyNow}, Cozy: {lastCozyState} -> {cozyNow}");
        }

        if (happyChanged)
        {
            if (happyNow)
            {
                if (debugLogs)
                    Debug.Log($"[TeacupIconEventRelay] {name}: Happy shown");
                onHappyShown?.Invoke();
            }
            else
            {
                if (debugLogs)
                    Debug.Log($"[TeacupIconEventRelay] {name}: Happy hidden");
                onHappyHidden?.Invoke();
            }
        }

        if (cozyChanged)
        {
            if (cozyNow)
            {
                if (debugLogs)
                    Debug.Log($"[TeacupIconEventRelay] {name}: Cozy shown");
                onCozyShown?.Invoke();
            }
            else
            {
                if (debugLogs)
                    Debug.Log($"[TeacupIconEventRelay] {name}: Cozy hidden");
                onCozyHidden?.Invoke();
            }
        }

        // Combined / conditional events after individual state changes:
        if ((!lastHappyState && happyNow) || (!lastCozyState && cozyNow))
        {
            if (happyNow || cozyNow)
            {
                if (debugLogs)
                    Debug.Log($"[TeacupIconEventRelay] {name}: Any icon shown");
                onAnyIconShown?.Invoke();
            }
        }

        if ((lastHappyState || lastCozyState) && !happyNow && !cozyNow)
        {
            if (debugLogs)
                Debug.Log($"[TeacupIconEventRelay] {name}: All icons hidden");
            onAllIconsHidden?.Invoke();
        }

        if ((!lastHappyState || lastCozyState) && happyNow && !cozyNow)
        {
            if (debugLogs)
                Debug.Log($"[TeacupIconEventRelay] {name}: Happy only shown");
            onHappyOnlyShown?.Invoke();
        }

        if (!(lastHappyState && lastCozyState) && happyNow && cozyNow)
        {
            if (debugLogs)
                Debug.Log($"[TeacupIconEventRelay] {name}: Happy and cozy shown together");
            onHappyAndCozyShown?.Invoke();
        }

        lastHappyState = happyNow;
        lastCozyState = cozyNow;
    }

    private void ResolveReferences()
    {
        if (!autoResolveReferences)
            return;

        if (teacupReceiver == null)
            teacupReceiver = GetComponent<TeacupReceiver>();

        if (teacupReceiver != null)
        {
            if (happyIcon == null)
                happyIcon = teacupReceiver.happyIcon;

            if (cozyIcon == null)
                cozyIcon = teacupReceiver.cozyIcon;
        }

        if (happyIcon == null)
            happyIcon = transform.Find("HappyIcon")?.gameObject;

        if (cozyIcon == null)
            cozyIcon = transform.Find("CozyIcon")?.gameObject;
    }

    private bool IsVisible(GameObject obj)
    {
        return obj != null && obj.activeSelf;
    }

    private void FireShowEventsForCurrentState(bool happyNow, bool cozyNow)
    {
        if (happyNow)
            onHappyShown?.Invoke();

        if (cozyNow)
            onCozyShown?.Invoke();

        if (happyNow || cozyNow)
            onAnyIconShown?.Invoke();

        if (happyNow && !cozyNow)
            onHappyOnlyShown?.Invoke();

        if (happyNow && cozyNow)
            onHappyAndCozyShown?.Invoke();
    }

    private void FireHideEventsForCurrentState(bool happyNow, bool cozyNow)
    {
        if (!happyNow)
            onHappyHidden?.Invoke();

        if (!cozyNow)
            onCozyHidden?.Invoke();

        if (!happyNow && !cozyNow)
            onAllIconsHidden?.Invoke();
    }

    public bool IsHappyShowing()
    {
        return IsVisible(happyIcon);
    }

    public bool IsCozyShowing()
    {
        return IsVisible(cozyIcon);
    }

    public bool IsAnyIconShowing()
    {
        return IsHappyShowing() || IsCozyShowing();
    }
}