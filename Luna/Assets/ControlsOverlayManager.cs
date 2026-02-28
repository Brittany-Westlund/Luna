using UnityEngine;

public class ControlsOverlayManager : MonoBehaviour
{
    [Header("Child name (auto-find). If you assign controlsOverlay manually, name is ignored.")]
    public string controlsOverlayChildName = "ControlsOverlay";

    [Header("Optional: drag the child here (recommended). Otherwise it auto-finds by name.")]
    public Transform controlsOverlay;

    [Header("Keybind")]
    public KeyCode toggleKey = KeyCode.C;

    [Header("2D depth (usually 0). Must be in front of camera clipping.")]
    public float zDepth = 0f;

    [Header("Debug")]
    public bool debugLogs = true;

    private SpriteRenderer sr;
    private Camera cam;
    private bool isVisible = false;

    void Awake()
    {
        ResolveOverlay();
        ResolveCamera();

        // Start hidden
        SetVisible(false, "Awake");
    }

    void Update()
    {
        // Toggle input
        if (Input.GetKeyDown(toggleKey))
        {
            if (debugLogs) Debug.Log($"[ControlsOverlay] {toggleKey} pressed. Toggling.");
            Toggle();
        }

        // Keep centered while visible (only if we have a camera)
        if (isVisible)
        {
            if (cam == null) ResolveCamera();

            if (controlsOverlay != null && cam != null)
            {
                controlsOverlay.position = new Vector3(
                    cam.transform.position.x,
                    cam.transform.position.y,
                    zDepth
                );
            }
        }
    }

    void Toggle()
    {
        ResolveOverlay();

        if (controlsOverlay == null)
        {
            Debug.LogError("[ControlsOverlay] No overlay found. Make a child named 'ControlsOverlay' with a SpriteRenderer, or assign the Transform in inspector.");
            return;
        }

        SetVisible(!isVisible, "Toggle");
    }

    void SetVisible(bool visible, string reason)
    {
        isVisible = visible;

        if (controlsOverlay == null)
        {
            if (debugLogs) Debug.LogWarning($"[ControlsOverlay] SetVisible({visible}) but controlsOverlay is null. ({reason})");
            return;
        }

        if (sr == null) sr = controlsOverlay.GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError("[ControlsOverlay] controlsOverlay has no SpriteRenderer. Add one.");
            return;
        }

        sr.enabled = visible;

        if (debugLogs)
        {
            Debug.Log($"[ControlsOverlay] Visible={visible} ({reason}) | SR.enabled={sr.enabled} | Pos={controlsOverlay.position} | Scale={controlsOverlay.lossyScale}");
        }
    }

    void ResolveOverlay()
    {
        if (controlsOverlay == null)
        {
            Transform t = transform.Find(controlsOverlayChildName);
            if (t != null) controlsOverlay = t;
        }

        if (controlsOverlay != null && sr == null)
            sr = controlsOverlay.GetComponent<SpriteRenderer>();
    }

    void ResolveCamera()
    {
        cam = Camera.main;

        if (cam == null && debugLogs)
            Debug.LogWarning("[ControlsOverlay] Camera.main is null (no MainCamera tag?). Overlay will still toggle, but won’t auto-center.");
    }
}