using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

[DisallowMultipleComponent]
public class FlowerPersistenceManager : MonoBehaviour
{
    public static FlowerPersistenceManager Instance { get; private set; }

    [Header("Behavior")]
    [Tooltip("Autosave on scene changes & on quit.")]
    public bool autoSave = true;

    [Tooltip("Garden search radius when re-parenting a planted flower to the nearest Garden.")]
    public float gardenSnapRadius = 0.75f;

    [Tooltip("Guard window to block phantom lighting after restore (seconds).")]
    public float phantomLightGuardSeconds = 0.5f;

    [Header("Emergency Respawn Prefabs (fallback if stash fails)")]
    [Tooltip("Map FlowerPickup.flowerType -> Prefab for respawn. Only used if a saved flower is missing.")]
    public List<PrefabEntry> prefabMap = new List<PrefabEntry>();
    [Serializable] public class PrefabEntry { public string key; public GameObject prefab; }

    // ---------- internals ----------
    Transform _stash; // DDOL stash for live carry
    readonly Dictionary<string, FlowerState> _cache = new(); // by flowerID

    // Freeze/restore trackers for guard window
    readonly Dictionary<int, bool> _origFullyGrown = new();
    readonly Dictionary<int, bool> _origCollider   = new();

    // Visual mute (no-flash): remember each sprite's color, set alpha=0, then restore
    readonly Dictionary<SpriteRenderer, Color> _origSpriteColor = new();

    // Score baselines
    int? _pointsBaselinePreScene = null; // captured in old scene, before load

    [Serializable]
    public class FlowerState
    {
        public string flowerID;
        public string sceneName;
        public string typeKey;         // FlowerPickup.flowerType for respawn

        public bool   isHeld;
        public bool   isPlanted;
        public bool   isLit;
        public bool   isFullyGrown;

        public Vector3   position;
        public Quaternion rotation;
        public Vector3   localScale;
    }

    [Serializable]
    public class SaveBlob
    {
        public int version = 7;        // adds no-flash mute + stash fallback fixes
        public string lastScene;
        public List<FlowerState> flowers = new List<FlowerState>();
    }

    string SavePath => Path.Combine(Application.persistentDataPath, "flowersave.json");

    // ---------- lifecycle ----------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        var stashGO = new GameObject("FlowerStash");
        _stash = stashGO.transform;
        DontDestroyOnLoad(stashGO);
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.quitting += OnQuit;
    }

    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Application.quitting -= OnQuit;
    }

    void OnQuit() { if (autoSave) SaveNow(); }

    void OnActiveSceneChanged(Scene oldS, Scene newS)
    {
        // Capture score baseline from the OLD scene (ScoreManager is alive here)
        var sm = ScoreManager.Instance;
        if (sm != null) _pointsBaselinePreScene = sm.points;

        if (autoSave) SaveNow();
        StashHeldIfAny(); // early attempt
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 0) Freeze & mute immediately to block any Start/physics-trigger lighting and hide visuals
        BeginFreezeAndMute();

        // 1) Last-chance stash
        StashHeldIfAny();

        // 2) Load persistent cache
        LoadCacheFromDisk();

        // 3) Ensure holders will yank flowers if they get destroyed
        AttachLifesavers();

        // 4) Do the heavy restore/return after one frame (still frozen & muted)
        StartCoroutine(RestoreThenUnfreeze());
    }

    // ---------- lifesaver: yank from holder on destroy ----------
    [DisallowMultipleComponent]
    private class HolderLifesaver : MonoBehaviour
    {
        FlowerHolder _holder;
        void Awake() { _holder = GetComponent<FlowerHolder>(); }

        void OnDestroy()
        {
            if (_holder == null || !_holder.HasFlower) return;

            var flower = _holder.GetHeldFlower();
            if (flower == null) return;

            // If manager/stash scene isn't ready, fall back to DontDestroyOnLoad
            var mgr = FlowerPersistenceManager.Instance;
            if (mgr == null || mgr._stash == null || !mgr._stash.gameObject.scene.IsValid())
            {
                flower.transform.SetParent(null, true);
                try { UnityEngine.Object.DontDestroyOnLoad(flower); } catch { }
#if UNITY_EDITOR
                Debug.Log($"🛟 [HolderLifesaver] DDOL fallback for '{flower.name}' (stash not ready).");
#endif
                return;
            }

            // Normal stash path: must be root to move
            flower.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(flower, mgr._stash.gameObject.scene);
            flower.transform.SetParent(mgr._stash, true);

            var rb = flower.GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = false;

#if UNITY_EDITOR
            Debug.Log($"🛟 [HolderLifesaver] Stashed '{flower.name}' to FlowerStash.");
#endif
        }
    }

    void AttachLifesavers()
    {
        var holders = FindObjectsOfType<FlowerHolder>(includeInactive: true);
        foreach (var h in holders)
            if (h != null && h.GetComponent<HolderLifesaver>() == null)
                h.gameObject.AddComponent<HolderLifesaver>();
    }

    // ---------- stash ----------
    void StashHeldIfAny()
    {
        var allIds = Resources.FindObjectsOfTypeAll<FlowerID>();
        foreach (var id in allIds)
        {
            if (id == null) continue;
            var go = id.gameObject;
            if (!go || !go.CompareTag("Sprout")) continue;

            // skip items already in DDOL (their root scene is invalid)
            var root = go.transform.root;
            if (!root || !root.gameObject.scene.IsValid()) continue;

            bool isHeld = false;
            var spr  = go.GetComponent<SproutAndLightManager>();
            if (spr != null && spr.isHeld) isHeld = true;
            var pick = go.GetComponent<FlowerPickup>();
            if (!isHeld && pick != null && pick.IsHeld) isHeld = true;

            if (!isHeld) continue;

            // Prefer real stash; if invalid, fallback to DontDestroyOnLoad
            if (_stash != null && _stash.gameObject.scene.IsValid())
            {
                go.transform.SetParent(null, true);
                SceneManager.MoveGameObjectToScene(go, _stash.gameObject.scene);
                go.transform.SetParent(_stash, true);
            }
            else
            {
                go.transform.SetParent(null, true);
                try { UnityEngine.Object.DontDestroyOnLoad(go); } catch { }
            }

            var rb = go.GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = false;

#if UNITY_EDITOR
            Debug.Log($"📦 Stashed HELD flower '{go.name}' (id {id.flowerID}) into DDOL.");
#endif
            return; // stash one
        }
    }

    // ---------- restore/return then unfreeze ----------
    IEnumerator RestoreThenUnfreeze()
    {
        // Allow Player/PortalSpawn Start() to run and place Luna
        yield return null;

        // Apply save to flowers already in the scene
        RestoreNow();

        // Find a holder
        var holder = FindObjectOfType<FlowerHolder>();

        // Prefer our stash child; if empty, also look for any root GOs living in DDOL (from lifesaver fallback)
        GameObject carried = (_stash != null && _stash.childCount > 0) ? _stash.GetChild(0).gameObject : null;

        if (carried == null)
        {
            // Search for any Sprout tagged objects that live in DDOL (scene is invalid)
            foreach (var id in Resources.FindObjectsOfTypeAll<FlowerID>())
            {
                if (!id || !id.gameObject || !id.gameObject.CompareTag("Sprout")) continue;
                var root = id.transform.root;
                if (!root || root.gameObject.scene.IsValid()) continue; // DDOL objects have invalid scene in this callback
                carried = id.gameObject;
                break;
            }
        }

        if (carried != null && holder != null && !holder.HasFlower)
        {
            // Move into active scene (PickUpFlower will re-parent)
            carried.transform.SetParent(null, true);
            SceneManager.MoveGameObjectToScene(carried, SceneManager.GetActiveScene());

            var rb = carried.GetComponent<Rigidbody2D>();
            if (rb) rb.simulated = true;

            SilentPickup(holder, carried);
#if UNITY_EDITOR
            Debug.Log($"🤝 Returned stashed flower '{carried.name}' to new scene holder.");
#endif
        }
        else
        {
            // Emergency respawn for any missing saved flowers
            var existing = new HashSet<string>();
            foreach (var id in FindObjectsOfType<FlowerID>(includeInactive: true))
                existing.Add(id.flowerID);

            foreach (var kv in _cache)
            {
                var fs = kv.Value;
                if (existing.Contains(fs.flowerID)) continue;
                if (string.IsNullOrEmpty(fs.typeKey)) continue;

                var prefab = GetPrefabForKey(fs.typeKey);
                if (prefab == null) continue;

                var spawned = Instantiate(prefab);
                spawned.tag = "Sprout";

                var id = spawned.GetComponent<FlowerID>() ?? spawned.AddComponent<FlowerID>();
                id.flowerID = fs.flowerID;

                SceneManager.MoveGameObjectToScene(spawned, SceneManager.GetActiveScene());

                ApplyState(spawned, fs);

#if UNITY_EDITOR
                Debug.Log($"🆘 Respawned missing flower '{fs.typeKey}' with ID {fs.flowerID} (held={fs.isHeld}, planted={fs.isPlanted}).");
#endif

                if (fs.isHeld && holder != null && !holder.HasFlower)
                {
                    SilentPickup(holder, spawned);
#if UNITY_EDITOR
                    Debug.Log($"🤝 Handed respawned flower ID {fs.flowerID} to holder.");
#endif
                }
            }
        }

        // Unfreeze, restore visuals (no-flash), and lock score to pre-scene baseline
        yield return StartCoroutine(EndFreezeAndUnmute());
    }

    // ---------- FREEZE + MUTE (no flash) ----------
    void BeginFreezeAndMute()
    {
        _origFullyGrown.Clear();
        _origCollider.Clear();
        _origSpriteColor.Clear();

        foreach (var id in Resources.FindObjectsOfTypeAll<FlowerID>())
        {
            var go  = id ? id.gameObject : null;
            if (!go || !go.CompareTag("Sprout")) continue;

            var spr = go.GetComponent<SproutAndLightManager>();

            // store & block fully grown + proximity
            if (spr != null)
            {
                _origFullyGrown[go.GetInstanceID()] = spr.IsFullyGrown;
                SafeSetPrivateBool(spr, "_isFullyGrown", false);
                spr.isPlayerNearby = false;
                spr.ClearAllHints();
            }

            // disable collider to stop new trigger events during loading
            if (go.TryGetComponent<Collider2D>(out var col))
            {
                _origCollider[go.GetInstanceID()] = col.enabled;
                col.enabled = false;
            }

            // visual mute — set alpha=0 on all SpriteRenderers under this flower
            foreach (var r in go.GetComponentsInChildren<SpriteRenderer>(true))
            {
                if (r == null) continue;
                if (!_origSpriteColor.ContainsKey(r))
                {
                    _origSpriteColor[r] = r.color;
                    var c = r.color; c.a = 0f; r.color = c;
                }
            }
        }
    }

    IEnumerator EndFreezeAndUnmute()
    {
        // Keep frozen briefly; let PortalSpawn and other Start() finish
        float tEnd = Time.time + Mathf.Max(0.05f, phantomLightGuardSeconds);
        while (Time.time < tEnd) yield return null;

        // 1) Set visuals to their expected (saved) state while still muted (alpha=0 so no flash)
        foreach (var id in FindObjectsOfType<FlowerID>(includeInactive: true))
        {
            var go = id.gameObject;
            if (!go.CompareTag("Sprout")) continue;

            if (_cache.TryGetValue(id.flowerID, out var fs))
                ApplyVisualsOnly(go, fs.isLit);
        }

        // 2) Restore points to baseline captured in previous scene (kills any phantom +1)
        var sm = ScoreManager.Instance;
        if (_pointsBaselinePreScene.HasValue && sm != null)
        {
            sm.points = _pointsBaselinePreScene.Value;
            sm.UpdatePointsText();
        }
        _pointsBaselinePreScene = null;

        // 3) Restore fully grown + colliders
        foreach (var id in FindObjectsOfType<FlowerID>(includeInactive: true))
        {
            var go = id.gameObject;
            if (!go.CompareTag("Sprout")) continue;

            int key = go.GetInstanceID();

            if (_origFullyGrown.TryGetValue(key, out var wasFG))
            {
                var spr = go.GetComponent<SproutAndLightManager>();
                if (spr != null) SafeSetPrivateBool(spr, "_isFullyGrown", wasFG);
            }

            if (_origCollider.TryGetValue(key, out var wasOn) && go.TryGetComponent<Collider2D>(out var col))
                col.enabled = wasOn;
        }
        _origFullyGrown.Clear();
        _origCollider.Clear();

        // 4) Unmute visuals: restore each SpriteRenderer’s original color (alpha)
        foreach (var kv in _origSpriteColor)
        {
            if (!kv.Key) continue;
            kv.Key.color = kv.Value;
        }
        _origSpriteColor.Clear();
    }

    // ---------- public API (optional: call before LoadScene in your portal) ----------
    public void PreFreezeAndStashForSceneChange()
    {
        BeginFreezeAndMute();
        StashHeldIfAny();
        if (autoSave) SaveNow();
    }

    public void SaveNow()
    {
        var blob = new SaveBlob
        {
            version   = 7,
            lastScene = SceneManager.GetActiveScene().name,
            flowers   = SnapshotAllFlowers()
        };

        File.WriteAllText(SavePath, JsonUtility.ToJson(blob, true));
#if UNITY_EDITOR
        Debug.Log($"💾 Saved {blob.flowers.Count} flowers → {SavePath}");
#endif
    }

    public void LoadNow() { LoadCacheFromDisk(); RestoreNow(); }

    // ---------- snapshot / restore ----------
    List<FlowerState> SnapshotAllFlowers()
    {
        var list = new List<FlowerState>();
        string sceneName = SceneManager.GetActiveScene().name;

        foreach (var id in FindObjectsOfType<FlowerID>(includeInactive: true))
        {
            var go = id.gameObject;
            if (!go.CompareTag("Sprout")) continue;

            var fs = new FlowerState
            {
                flowerID   = id.flowerID,
                sceneName  = sceneName,
                position   = go.transform.position,
                rotation   = go.transform.rotation,
                localScale = go.transform.localScale
            };

            var pick = go.GetComponent<FlowerPickup>();
            fs.typeKey = (pick != null && !string.IsNullOrEmpty(pick.flowerType)) ? pick.flowerType : "Unknown";

            var spr = go.GetComponent<SproutAndLightManager>();
            if (spr != null)
            {
                fs.isHeld       = spr.isHeld;
                fs.isPlanted    = spr.isPlanted;
                fs.isFullyGrown = spr.IsFullyGrown;

                bool lit = (spr.litFlowerRenderer != null && spr.litFlowerRenderer.enabled);
                var unlit = go.GetComponent<UnlitFlower>();
                if (unlit != null && unlit.litVersion != null) lit = lit || unlit.litVersion.activeSelf;
                fs.isLit = lit;
            }
            else
            {
                fs.isHeld       = (pick != null && pick.IsHeld);
                fs.isPlanted    = (pick != null && pick.IsPlanted);
                fs.isFullyGrown = fs.isPlanted; // best effort
                var unlit = go.GetComponent<UnlitFlower>();
                fs.isLit        = (unlit != null && unlit.litVersion != null && unlit.litVersion.activeSelf);
            }

            list.Add(fs);
        }
        return list;
    }

    void RestoreNow()
    {
        foreach (var id in FindObjectsOfType<FlowerID>(includeInactive: true))
        {
            var go = id.gameObject;
            if (!go.CompareTag("Sprout")) continue;

            if (_cache.TryGetValue(id.flowerID, out var fs))
                ApplyState(go, fs);
        }
    }

    void ApplyState(GameObject go, FlowerState fs)
    {
        var t = go.transform;
        t.position   = fs.position;
        t.rotation   = fs.rotation;
        t.localScale = fs.localScale;

        if (fs.isPlanted)
        {
            var parent = FindNearestGarden(t.position, gardenSnapRadius);
            if (parent != null)
            {
                t.SetParent(parent, true);
                TrySetGardenSpotPlantedFlower(parent.gameObject, go);
            }
        }
        else
        {
            if (!fs.isHeld) t.SetParent(null, true);
        }

        // Flags & visuals (set flags; visuals will be finalized while muted just before unmute)
        var spr = go.GetComponent<SproutAndLightManager>();
        if (spr != null)
        {
            spr.isHeld    = fs.isHeld;
            spr.isPlanted = fs.isPlanted;

            SafeSetPrivateBool(spr, "_isFullyGrown", fs.isFullyGrown);
            SafeSetPrivateBool(spr, "hasBeenLit",   fs.isLit);

            ApplyVisualsOnly(go, fs.isLit);
            spr.ClearAllHints();
        }
        else
        {
            ApplyVisualsOnly(go, fs.isLit);
        }

        if (go.TryGetComponent<Collider2D>(out var col))
            col.enabled = !fs.isHeld;
    }

    // Only touch lit/unlit visuals; do not affect alpha mute here
    void ApplyVisualsOnly(GameObject go, bool lit)
    {
        var spr = go.GetComponent<SproutAndLightManager>();
        if (spr != null && spr.litFlowerRenderer != null)
            spr.litFlowerRenderer.enabled = lit;

        var unlit = go.GetComponent<UnlitFlower>();
        if (unlit != null && unlit.litVersion != null)
        {
            unlit.litVersion.SetActive(lit);
            unlit.gameObject.SetActive(!lit);
        }
    }

    // ---------- helpers ----------
    void LoadCacheFromDisk()
    {
        _cache.Clear();
        if (!File.Exists(SavePath)) return;

        try
        {
            var blob = JsonUtility.FromJson<SaveBlob>(File.ReadAllText(SavePath));
            if (blob?.flowers != null)
                foreach (var f in blob.flowers)
                    _cache[f.flowerID] = f;
#if UNITY_EDITOR
            Debug.Log($"📥 Loaded flower cache ({_cache.Count}) from disk.");
#endif
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Flower load failed: {ex.Message}");
        }
    }

    GameObject GetPrefabForKey(string key)
    {
        foreach (var e in prefabMap)
            if (!string.IsNullOrEmpty(e.key) && e.key == key && e.prefab != null)
                return e.prefab;
        return null;
    }

    Transform FindNearestGarden(Vector3 pos, float radius)
    {
        var hits = Physics2D.OverlapCircleAll(pos, radius);
        Transform best = null; float bestD = Mathf.Infinity;
        foreach (var h in hits)
        {
            if (!h.CompareTag("Garden")) continue;
            float d = Vector2.Distance(pos, h.transform.position);
            if (d < bestD) { bestD = d; best = h.transform; }
        }
        return best;
    }

    void TrySetGardenSpotPlantedFlower(GameObject gardenGO, GameObject flowerGO)
    {
        var mbs = gardenGO.GetComponents<MonoBehaviour>();
        foreach (var mb in mbs)
        {
            if (mb == null) continue;
            var type = mb.GetType();
            if (type.Name != "GardenSpot") continue;

            var mi = type.GetMethod("SetPlantedFlower",
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 1)
            {
                try { mi.Invoke(mb, new object[] { flowerGO }); }
                catch { /* best-effort */ }
            }
            break;
        }
    }

    static void SafeSetPrivateBool(object obj, string fieldName, bool value)
    {
        if (obj == null) return;
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(bool)) f.SetValue(obj, value);
    }

    void SilentPickup(FlowerHolder holder, GameObject flower)
    {
        bool resetMute = false; float oldVol = 0f;
        if (holder.pickupSFXSource != null) { resetMute = true; oldVol = holder.pickupSFXSource.volume; holder.pickupSFXSource.volume = 0f; }
        holder.PickUpFlower(flower);
        if (resetMute && holder.pickupSFXSource != null) holder.pickupSFXSource.volume = oldVol;
    }
}
