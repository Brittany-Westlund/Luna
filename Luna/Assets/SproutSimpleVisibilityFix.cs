using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

/// Minimal, surgical: make sprouts visible after a scene load.
/// - No renames, no refs to ScoreManager, no persistence changes.
/// - Default behavior: set alpha=1 on all SpriteRenderers under each object tagged "Sprout".
/// - Optional toggles (in Inspector) if you still can’t see them:
///     • Force Sorting Order (e.g., 100)
///     • Force Z to 0
///     • Normalize tiny scales (< 0.01 -> 0.1)
[DisallowMultipleComponent]
public class SproutSimpleVisibilityFix : MonoBehaviour
{
    [Header("Behavior")]
    [Tooltip("Run automatically after each scene load.")]
    public bool runOnSceneLoad = true;

    [Tooltip("Seconds to wait after scene load (let portals place player).")]
    public float delayAfterLoad = 0.05f;

    [Header("Optional helpers (enable only if needed)")]
    public bool normalizeTinyScale = false;
    public bool forceSortingOrder = false;
    public int  sortingOrder = 100;
    public bool forceZToZero = false;

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        if (runOnSceneLoad) SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (runOnSceneLoad) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StartCoroutine(FixAfterDelay());
    }

    IEnumerator FixAfterDelay()
    {
        if (delayAfterLoad > 0f) yield return new WaitForSeconds(delayAfterLoad);
        FixAllSproutsNow();
    }

    [ContextMenu("Fix All Sprouts Now")]
    public void FixAllSproutsNow()
    {
        var sprouts = GameObject.FindGameObjectsWithTag("Sprout");
        foreach (var sprout in sprouts) ForceVisible(sprout);
#if UNITY_EDITOR
        Debug.Log($"SproutSimpleVisibilityFix: normalized {sprouts.Length} sprouts.");
#endif
    }

    void ForceVisible(GameObject root)
    {
        if (!root) return;

        // 1) Ensure every SpriteRenderer is enabled and alpha=1.
        var renderers = root.GetComponentsInChildren<SpriteRenderer>(true);
        foreach (var r in renderers)
        {
            if (!r) continue;
            r.enabled = true;
            var c = r.color; if (c.a < 0.99f) { c.a = 1f; r.color = c; }
            if (forceSortingOrder) r.sortingOrder = sortingOrder;
        }

        // 2) Optional transforms
        if (normalizeTinyScale)
        {
            var ls = root.transform.localScale;
            if (Mathf.Abs(ls.x) < 0.01f || Mathf.Abs(ls.y) < 0.01f)
                root.transform.localScale = new Vector3(
                    Mathf.Max(0.1f, Mathf.Abs(ls.x)) * Mathf.Sign(ls.x == 0 ? 1 : ls.x),
                    Mathf.Max(0.1f, Mathf.Abs(ls.y)) * Mathf.Sign(ls.y == 0 ? 1 : ls.y),
                    ls.z
                );
        }
        if (forceZToZero)
        {
            var p = root.transform.position;
            root.transform.position = new Vector3(p.x, p.y, 0f);
        }
    }
}
