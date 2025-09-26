// SimpleThoughtBubbles.cs
using UnityEngine;

public class SimpleThoughtBubbles : MonoBehaviour
{
    [Header("All bubble variants (children under this rest object)")]
    public Transform[] bubbles;          // drag each child bubble here in order
    public int defaultIndex = 0;         // which bubble to use if nothing else sets it
    [Tooltip("Final world height for the bubble's visual, uniform scaling")]
    public float targetWorldHeight = 0.8f;

    private int currentIndex = -1;       // what we intend to show when enabled
    private Transform activeRoot;

    public int CurrentIndex => currentIndex;

    void Awake()
    {
        HideAll();
        currentIndex = Mathf.Clamp(defaultIndex, 0, (bubbles?.Length ?? 1) - 1);
    }

    void OnEnable()
    {
        ShowCurrent();
    }

    void OnDisable()
    {
        HideAll();
    }

    public void SetIndex(int index)
    {
        if (bubbles == null || bubbles.Length == 0) return;
        currentIndex = Mathf.Clamp(index, 0, bubbles.Length - 1);

        // If we're currently visible (resting), update immediately
        if (isActiveAndEnabled)
            ShowCurrent();
    }

    public void ResetToDefault()
    {
        SetIndex(defaultIndex);
    }

    private void ShowCurrent()
    {
        HideAll();
        if (bubbles == null || bubbles.Length == 0) return;

        activeRoot = bubbles[Mathf.Clamp(currentIndex, 0, bubbles.Length - 1)];
        if (!activeRoot) return;

        activeRoot.gameObject.SetActive(true);
        // Ensure visuals are enabled and sized consistently
        var sr = activeRoot.GetComponentInChildren<SpriteRenderer>(true);
        if (sr != null)
        {
            // normalize first (prevents giant-first-bubble issue)
            sr.transform.localScale = Vector3.one;
            sr.enabled = true;
            sr.drawMode = SpriteDrawMode.Simple;

            // uniform world-height sizing (compensates parent scale)
            if (sr.sprite != null)
            {
                float h = sr.sprite.bounds.size.y;
                if (h > 0f)
                {
                    Vector3 parentLossy = sr.transform.parent ? sr.transform.parent.lossyScale : Vector3.one;
                    float sWorld = targetWorldHeight / h;
                    float sx = sWorld / Mathf.Max(parentLossy.x, 1e-6f);
                    float sy = sWorld / Mathf.Max(parentLossy.y, 1e-6f);
                    sr.transform.localScale = new Vector3(sx, sy, 1f);
                }
            }
        }

        // If your bubble has multiple SRs, enable all (no fade, simple + predictable)
        foreach (var r in activeRoot.GetComponentsInChildren<SpriteRenderer>(true))
            r.enabled = true;
    }

    private void HideAll()
    {
        if (bubbles == null) return;
        for (int i = 0; i < bubbles.Length; i++)
        {
            if (bubbles[i] != null)
                bubbles[i].gameObject.SetActive(false);
        }
        activeRoot = null;
    }
}
