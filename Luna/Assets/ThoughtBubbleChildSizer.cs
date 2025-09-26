// ThoughtBubbleChildSizer.cs
using UnityEngine;

public class ThoughtBubbleChildSizer : MonoBehaviour
{
    [Header("Child that holds the SpriteRenderer(s)")]
    public Transform visual;   // assign your "Visual" child here

    [Header("Size")]
    [Tooltip("Final world height (in Unity units) regardless of parent scale.")]
    public float targetWorldHeight = 0.8f;

    [Tooltip("Apply automatically on Start; or call ApplySize() manually after spawn.")]
    public bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart) ApplySize();
    }

    public void ApplySize()
    {
        if (visual == null)
        {
            // auto-find first SpriteRenderer child if not assigned
            var srAuto = GetComponentInChildren<SpriteRenderer>();
            if (srAuto == null) return;
            visual = srAuto.transform;
        }

        var sr = visual.GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        float spriteWorldHeight = sr.sprite.bounds.size.y;
        if (spriteWorldHeight <= 0f) return;

        float desiredWorld = targetWorldHeight;

        // Compensate for parent lossy scale so visual ends up uniform in world space
        Vector3 parentLossy = visual.parent ? visual.parent.lossyScale : Vector3.one;
        float sWorld = desiredWorld / spriteWorldHeight;
        float sx = sWorld / Mathf.Max(parentLossy.x, 1e-6f);
        float sy = sWorld / Mathf.Max(parentLossy.y, 1e-6f);

        visual.localScale = new Vector3(sx, sy, 1f); // uniform, no distortion
    }
}
