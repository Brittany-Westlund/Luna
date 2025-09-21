using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GardenStickySlot : MonoBehaviour
{
    [Header("Key / Identity")]
    [Tooltip("Leave empty to auto-generate a unique key per scene + object path.")]
    public string key = "";
    [Tooltip("Prefix keys with scene name automatically (recommended).")]
    public bool prefixWithScene = true;

    [Header("Planting Point")]
    [Tooltip("Where the flower should sit (defaults to this.transform).")]
    public Transform plantingPoint;

    [Header("Debug")]
    public bool verboseLogs = false;

    // Shared (session) stash
    private static Transform stashRoot;
    private static readonly Dictionary<string, GameObject> saved = new();

    // --- Lifecycle ---
    void Awake()
    {
        if (!plantingPoint) plantingPoint = transform;
        EnsureStash();

        if (string.IsNullOrEmpty(key))
            key = BuildAutoKey();     // scene+path based key
        else if (prefixWithScene)
            key = $"{SceneManager.GetActiveScene().name}:{key}";
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        // try immediately…
        TryRestore();
        // …and again next frame in case slot/spawn order is slow
        StartCoroutine(DeferredRestore());
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        TryStash();
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // When returning to this scene, slot gets enabled => TryRestore in OnEnable covers it
        // If the slot was already active (additive cases), do an extra pass:
        StartCoroutine(DeferredRestore());
    }

    IEnumerator DeferredRestore()
    {
        yield return null; // one frame
        TryRestore();
    }

    // --- Core ---
    private void TryRestore()
    {
        // clean nulls
        if (saved.ContainsKey(key) && saved[key] == null)
            saved.Remove(key);

        if (saved.TryGetValue(key, out var go) && go)
        {
            // If our saved sprout is still in the stash, reattach to this garden
            if (go.transform.parent == stashRoot)
            {
                // Remove any duplicate sprout Unity baked in this scene
                var extra = FindChildSprout();
                if (extra && extra != go) Destroy(extra);

                Log($"Reattaching saved sprout to [{key}].");
                Reattach(go);
            }
            else if (!go.transform.IsChildOf(transform))
            {
                LogWarn($"Saved sprout for [{key}] is attached elsewhere. Duplicate key?");
            }
        }
        else
        {
            // First time: if a sprout is already present as child, register it
            var planted = FindChildSprout();
            if (planted)
            {
                Log($"Registering existing sprout under [{key}].");
                saved[key] = planted;
            }
        }
    }

    private void TryStash()
    {
        var planted = FindChildSprout();
        if (planted)
        {
            Log($"Stashing sprout from [{key}] while scene/slot disables.");
            Stash(planted);
        }
    }

    // --- Helpers ---
    private string BuildAutoKey()
    {
        // unique path under scene (SceneName:Root/Parent/This)
        string scene = SceneManager.GetActiveScene().name;
        string path = GetHierarchyPath(transform);
        return $"{scene}:{path}";
    }

    private static string GetHierarchyPath(Transform t)
    {
        var names = new Stack<string>();
        var cur = t;
        while (cur != null)
        {
            names.Push(cur.name);
            cur = cur.parent;
        }
        return string.Join("/", names);
    }

    private GameObject FindChildSprout()
    {
        // requires your sprout prefab to be tagged "Sprout"
        foreach (Transform c in transform)
            if (c && c.CompareTag("Sprout"))
                return c.gameObject;
        return null;
    }

    private static void EnsureStash()
    {
        if (stashRoot) return;
        var stash = new GameObject("GardenStickyStash");
        stash.hideFlags = HideFlags.DontSave;
        Object.DontDestroyOnLoad(stash);
        stashRoot = stash.transform;
    }

    private void ToggleAllColliders(GameObject go, bool enabled)
    {
        foreach (var col in go.GetComponentsInChildren<Collider2D>(true))
            if (col) col.enabled = enabled;
    }

    private void Stash(GameObject go)
    {
        if (!go) return;

        saved[key] = go;

        if (go.transform.parent != stashRoot)
            go.transform.SetParent(stashRoot, true);

        try { Object.DontDestroyOnLoad(go); } catch { /* some editors throw in edit */ }

        ToggleAllColliders(go, false);

        var spr = go.GetComponent<SproutAndLightManager>();
        if (spr != null)
        {
            spr.isHeld = false;
            spr.isPlanted = true;
            spr.isPlayerNearby = false;
            spr.ClearAllHints();
        }
    }

    private void Reattach(GameObject go)
    {
        if (!go) return;

        go.transform.SetParent(plantingPoint ? plantingPoint : transform, true);
        go.transform.position = (plantingPoint ? plantingPoint : transform).position;

        ToggleAllColliders(go, true);

        var spr = go.GetComponent<SproutAndLightManager>();
        if (spr != null)
        {
            spr.isHeld = false;
            spr.isPlanted = true;
            spr.isPlayerNearby = false;
            spr.ClearAllHints();
        }

        // Wake any OnEnable/Start on the sprout
        StartCoroutine(PulseEnable(go));
    }

    private IEnumerator PulseEnable(GameObject go)
    {
        if (!go) yield break;
        if (!go.activeSelf) { go.SetActive(true); yield break; }
        go.SetActive(false);
        yield return null;
        go.SetActive(true);
    }

    // --- Debug ---
    private void Log(string msg)
    {
        if (verboseLogs) Debug.Log($"[GardenStickySlot] {msg}", this);
    }
    private void LogWarn(string msg)
    {
        if (verboseLogs) Debug.LogWarning($"[GardenStickySlot] {msg}", this);
    }

    // Optional: clear saved state for this slot (call from context menu or a debug button)
    [ContextMenu("Clear Saved For This Slot")]
    private void ClearThis()
    {
        if (saved.ContainsKey(key)) saved.Remove(key);
        Debug.Log($"[GardenStickySlot] Cleared saved for [{key}].", this);
    }
}
