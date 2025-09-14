using UnityEngine;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class GardenStickySlot : MonoBehaviour
{
    [Header("Unique key for THIS garden (required & unique)")]
    public string key = "Garden-1";

    [Header("Where the flower should sit (defaults to this.transform)")]
    public Transform plantingPoint;

    // Shared stash for this session
    private static Transform stashRoot;
    private static readonly Dictionary<string, GameObject> saved = new();

    void Awake()
    {
        if (!plantingPoint) plantingPoint = transform;
        EnsureStash();
        if (string.IsNullOrEmpty(key)) key = gameObject.name; // fallback
    }

    void OnEnable()
    {
        if (saved.TryGetValue(key, out var go) && go)
        {
            if (go.transform.parent == stashRoot)
            {
                Reattach(go);
            }
            else if (!go.transform.IsChildOf(transform))
            {
                Debug.LogWarning($"GardenStickySlot[{key}]: flower already attached elsewhere; duplicate key?", this);
            }
        }
        else
        {
            var planted = FindChildSprout();
            if (planted) saved[key] = planted; // track first time; don't move it
        }
    }

    void OnDisable()
    {
        var planted = FindChildSprout();
        if (planted) Stash(planted);
    }

    // -------- helpers --------
    GameObject FindChildSprout()
    {
        foreach (Transform c in transform)
        {
            if (c && c.CompareTag("Sprout"))
                return c.gameObject;
        }
        return null;
    }

    static void EnsureStash()
    {
        if (stashRoot) return;
        var stash = new GameObject("GardenStickyStash");
        stash.hideFlags = HideFlags.DontSave;
        Object.DontDestroyOnLoad(stash);
        stashRoot = stash.transform;
    }

    void ToggleAllColliders(GameObject go, bool enabled)
    {
        foreach (var col in go.GetComponentsInChildren<Collider2D>(true))
            if (col) col.enabled = enabled;
    }

    void Stash(GameObject go)
    {
        if (!go) return;
        saved[key] = go;

        if (go.transform.parent != stashRoot)
            go.transform.SetParent(stashRoot, true);

        try { Object.DontDestroyOnLoad(go); } catch {}

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

    void Reattach(GameObject go)
    {
        if (!go) return;

        go.transform.SetParent(plantingPoint, true);
        go.transform.position = plantingPoint.position; // keep world scale/rot

        ToggleAllColliders(go, true);

        var spr = go.GetComponent<SproutAndLightManager>();
        if (spr != null)
        {
            spr.isHeld = false;
            spr.isPlanted = true;
            spr.isPlayerNearby = false;
            spr.ClearAllHints();
        }

        // Pulse-enable one frame to wake any OnEnable/Start-based effects (e.g., sway/animators)
        StartCoroutine(PulseEnable(go));
    }

    System.Collections.IEnumerator PulseEnable(GameObject go)
    {
        if (!go) yield break;
        if (!go.activeSelf) { go.SetActive(true); yield break; }
        go.SetActive(false);
        yield return null; // one frame
        go.SetActive(true);
    }
}
