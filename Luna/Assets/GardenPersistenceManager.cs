using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

/// Minimal, garden-centric persistence + carry-one-flower across scenes.
/// - No edits to your other scripts.
/// - Tags used: "Garden" (gardens), "Sprout" (flowers).
/// - Persists per-garden: hasFlower, flowerType, lit, local pos/rot/scale.
/// - Respawns missing flowers via prefab map (string -> prefab).
/// - Carries the one held flower across scenes (DDOL) and quietly hands it back.
/// - Brief collider mute on load to avoid flicker/phantom triggers.
/// - Ensures a visible SpriteRenderer after restore (fixes “invisible but pickable”).
[DisallowMultipleComponent]
public class GardenPersistenceManager : MonoBehaviour
{
    // ====== SINGLETON ======
    public static GardenPersistenceManager Instance { get; private set; }
    static bool _madeRuntimeInstance = false;

    static void EnsureInstance()
    {
        if (Instance != null) return;
        var go = new GameObject("GardenPersistenceManager");
        Instance = go.AddComponent<GardenPersistenceManager>();
        DontDestroyOnLoad(go);
        _madeRuntimeInstance = true;
    }

    // ====== INSPECTOR ======
    [Header("Persistence")]
    [Tooltip("Autosave current scene on scene changes & on quit.")]
    public bool autoSave = true;

    [Tooltip("How long to keep Sprout colliders disabled after scene load.")]
    public float settleSeconds = 0.25f;

    [Header("Respawn Prefabs")]
    [Tooltip("Map FlowerPickup.flowerType -> Prefab to spawn if a saved garden is missing its flower.")]
    public List<PrefabEntry> prefabMap = new List<PrefabEntry>();
    [Serializable] public class PrefabEntry { public string key; public GameObject prefab; }

    // ====== DISK DATA ======
    [Serializable]
    class GardenRec
    {
        public string scene;
        public string gardenPath;
        public bool hasFlower;
        public string flowerType;
        public bool isLit;
        public Vector3 localPos;
        public Quaternion localRot;
        public Vector3 localScale;
    }

    [Serializable]
    class CarryRec
    {
        public bool has;
        public string flowerType;
        public bool isLit;
        public Vector3 localScale;
    }

    [Serializable]
    class SaveBlob
    {
        public int version = 2;
        public List<GardenRec> gardens = new List<GardenRec>();
        public CarryRec carry = new CarryRec();
    }

    string SavePath => Path.Combine(Application.persistentDataPath, "garden_lite.json");

    // ====== RUNTIME ======
    SaveBlob _cache = new SaveBlob();
    static GameObject _carriedLive;
    static CarryRec _carriedState;

    // ====== UNITY LIFECYCLE ======
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.activeSceneChanged += OnSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;
        Application.quitting += OnQuitting;
    }
    void OnDisable()
    {
        SceneManager.activeSceneChanged -= OnSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        Application.quitting -= OnQuitting;
    }

    void OnQuitting() { if (autoSave) SaveNow(); }
    void OnSceneChanged(Scene oldS, Scene newS) { if (autoSave) SaveNow(); }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadFromDisk();
        StartCoroutine(RestoreThenSettle());
    }

    IEnumerator RestoreThenSettle()
    {
        // 0) temporarily disable Sprout colliders and hush hints
        var toEnableLater = new List<Collider2D>();
        foreach (var go in FindObjectsOfType<GameObject>(includeInactive: true))
        {
            if (!go.CompareTag("Sprout")) continue;
            if (go.TryGetComponent<Collider2D>(out var col) && col.enabled)
            {
                col.enabled = false;
                toEnableLater.Add(col);
            }
            var s = go.GetComponent<SproutAndLightManager>();
            if (s != null) { s.isPlayerNearby = false; s.ClearAllHints(); }
        }

        yield return null; // let portals place player

        // 1) restore the gardens for this scene
        RestoreGardensForScene(SceneManager.GetActiveScene().name);

        // 2) hand back any carried flower
        HandBackCarried();

        // 3) re-enable colliders after a short settle
        if (settleSeconds > 0f) yield return new WaitForSeconds(settleSeconds);
        foreach (var col in toEnableLater) if (col) col.enabled = true;
    }

    // ====== PUBLIC: keep your existing call site the same ======
    public void PreFreezeAndStashForSceneChange()
    {
        StashHeldNow();
        if (autoSave) SaveNow();
    }

    // (Optional static helper if you prefer calling without Instance)
    public static void PrepareForSceneLoad()
    {
        EnsureInstance();
        Instance.PreFreezeAndStashForSceneChange();
    }

    // ====== SAVE / LOAD ======
    public void SaveNow()
    {
        _cache = new SaveBlob { version = 2, carry = SnapshotCarry() };
        var sceneName = SceneManager.GetActiveScene().name;

        foreach (var garden in GameObject.FindGameObjectsWithTag("Garden"))
        {
            if (!garden) continue;

            var rec = new GardenRec
            {
                scene = sceneName,
                gardenPath = GetPath(garden.transform),
                hasFlower = false,
                flowerType = null,
                isLit = false,
                localPos = Vector3.zero,
                localRot = Quaternion.identity,
                localScale = Vector3.one
            };

            var planted = GetPlantedFlower(garden);
            if (planted != null)
            {
                rec.hasFlower = true;

                var pick = planted.GetComponent<FlowerPickup>();
                rec.flowerType = (pick && !string.IsNullOrEmpty(pick.flowerType)) ? pick.flowerType : "Unknown";

                bool lit = false;
                var spr = planted.GetComponent<SproutAndLightManager>();
                if (spr && spr.litFlowerRenderer) lit = spr.litFlowerRenderer.enabled;
                var un = planted.GetComponent<UnlitFlower>();
                if (un && un.litVersion) lit = lit || un.litVersion.activeSelf;
                rec.isLit = lit;

                rec.localPos = planted.transform.localPosition;
                rec.localRot = planted.transform.localRotation;
                rec.localScale = planted.transform.localScale;
            }

            _cache.gardens.Add(rec);
        }

        try
        {
            File.WriteAllText(SavePath, JsonUtility.ToJson(_cache, true));
#if UNITY_EDITOR
            Debug.Log($"💾 Saved gardens: {_cache.gardens.Count} → {SavePath}");
#endif
        }
        catch (Exception ex) { Debug.LogWarning($"Save failed: {ex.Message}"); }
    }

    void LoadFromDisk()
    {
        _cache = new SaveBlob();
        if (!File.Exists(SavePath)) return;
        try { _cache = JsonUtility.FromJson<SaveBlob>(File.ReadAllText(SavePath)) ?? new SaveBlob(); }
        catch (Exception ex) { Debug.LogWarning($"Load failed: {ex.Message}"); _cache = new SaveBlob(); }
    }

    // ====== RESTORE ======
    void RestoreGardensForScene(string sceneName)
    {
        if (_cache.gardens == null) return;

        var recs = new Dictionary<string, GardenRec>();
        foreach (var rec in _cache.gardens)
            if (rec.scene == sceneName && !string.IsNullOrEmpty(rec.gardenPath))
                recs[rec.gardenPath] = rec;

        foreach (var garden in GameObject.FindGameObjectsWithTag("Garden"))
        {
            if (!garden) continue;
            string path = GetPath(garden.transform);

            if (!recs.TryGetValue(path, out var rec)) continue;

            var planted = GetPlantedFlower(garden);

            if (!rec.hasFlower)
            {
                // leave designer-placed content alone
                continue;
            }

            if (planted == null)
            {
                var prefab = GetPrefabFor(rec.flowerType);
                if (prefab == null) continue;

                planted = Instantiate(prefab);
                planted.tag = "Sprout";

                planted.transform.SetParent(garden.transform, false);
                planted.transform.localPosition = rec.localPos;
                planted.transform.localRotation = rec.localRot;
                planted.transform.localScale = rec.localScale;

                TrySetGardenSpotPlantedFlower(garden, planted);
            }
            else
            {
                planted.transform.SetParent(garden.transform, true);
                planted.transform.localPosition = rec.localPos;
                planted.transform.localRotation = rec.localRot;
                planted.transform.localScale = rec.localScale;
            }

            // apply visuals/flags without triggering score
            var spr = planted.GetComponent<SproutAndLightManager>();
            if (spr)
            {
                spr.isPlanted = true;
                SafeSetPrivateBool(spr, "hasBeenLit", rec.isLit);
                if (spr.litFlowerRenderer) spr.litFlowerRenderer.enabled = rec.isLit;
                spr.isPlayerNearby = false;
                spr.ClearAllHints();
            }
            else
            {
                var un = planted.GetComponent<UnlitFlower>();
                if (un && un.litVersion)
                {
                    un.litVersion.SetActive(rec.isLit);
                    un.gameObject.SetActive(!rec.isLit);
                }
            }

            EnsureRenderableVisible(planted, rec.isLit);
        }
    }

    void HandBackCarried()
    {
        if (_carriedLive == null) return;

        SceneManager.MoveGameObjectToScene(_carriedLive, SceneManager.GetActiveScene());

        var holder = FindObjectOfType<FlowerHolder>();
        if (holder != null && !holder.HasFlower)
        {
            var spr = _carriedLive.GetComponent<SproutAndLightManager>();
            if (spr)
            {
                SafeSetPrivateBool(spr, "hasBeenLit", _carriedState.isLit);
                if (spr.litFlowerRenderer) spr.litFlowerRenderer.enabled = _carriedState.isLit;
                spr.isHeld = true;
                spr.isPlanted = false;
                spr.isPlayerNearby = false;
                spr.ClearAllHints();
            }
            else
            {
                var un = _carriedLive.GetComponent<UnlitFlower>();
                if (un && un.litVersion)
                {
                    un.litVersion.SetActive(_carriedState.isLit);
                    un.gameObject.SetActive(!_carriedState.isLit);
                }
            }

            _carriedLive.transform.localScale = _carriedState.localScale;
            EnsureRenderableVisible(_carriedLive, _carriedState.isLit);

            bool mute = (holder.pickupSFXSource != null);
            float oldVol = mute ? holder.pickupSFXSource.volume : 0f;
            if (mute) holder.pickupSFXSource.volume = 0f;

            holder.PickUpFlower(_carriedLive);

            if (mute) holder.pickupSFXSource.volume = oldVol;
        }

        _carriedLive = null;
    }

    // ====== CARRY STASH ======
    void StashHeldNow()
    {
        var holder = FindObjectOfType<FlowerHolder>();
        if (holder == null || !holder.HasFlower) { _carriedLive = null; _carriedState = new CarryRec(); return; }

        _carriedLive = holder.GetHeldFlower();
        if (_carriedLive == null) { _carriedState = new CarryRec(); return; }

        _carriedState = new CarryRec { has = true, isLit = false, localScale = _carriedLive.transform.localScale, flowerType = null };

        var pick = _carriedLive.GetComponent<FlowerPickup>();
        if (pick != null) _carriedState.flowerType = pick.flowerType;

        var spr = _carriedLive.GetComponent<SproutAndLightManager>();
        if (spr && spr.litFlowerRenderer) _carriedState.isLit = spr.litFlowerRenderer.enabled;
        var un = _carriedLive.GetComponent<UnlitFlower>();
        if (un && un.litVersion) _carriedState.isLit = _carriedState.isLit || un.litVersion.activeSelf;

        _carriedLive.transform.SetParent(null, true);
        try { DontDestroyOnLoad(_carriedLive); } catch { }
        if (_carriedLive.TryGetComponent<Collider2D>(out var col)) col.enabled = false;
    }

    CarryRec SnapshotCarry()
    {
        if (_carriedLive != null) return _carriedState ?? new CarryRec();

        var holder = FindObjectOfType<FlowerHolder>();
        if (holder != null && holder.HasFlower)
        {
            var go = holder.GetHeldFlower();
            var cr = new CarryRec { has = true, localScale = go.transform.localScale };

            var pick = go.GetComponent<FlowerPickup>();
            cr.flowerType = (pick != null) ? pick.flowerType : "Unknown";

            bool lit = false;
            var spr = go.GetComponent<SproutAndLightManager>();
            if (spr && spr.litFlowerRenderer) lit = spr.litFlowerRenderer.enabled;
            var un = go.GetComponent<UnlitFlower>();
            if (un && un.litVersion) lit = lit || un.litVersion.activeSelf;
            cr.isLit = lit;

            return cr;
        }

        return new CarryRec();
    }

    // ====== HELPERS ======
    static string GetPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null) { t = t.parent; path = t.name + "/" + path; }
        return path;
    }

    static GameObject GetPlantedFlower(GameObject gardenGO)
    {
        var mb = GetGardenSpotBehaviour(gardenGO);
        if (mb != null)
        {
            var mi = mb.GetType().GetMethod("GetPlantedFlower", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (mi != null && mi.GetParameters().Length == 0)
            { try { return mi.Invoke(mb, null) as GameObject; } catch { } }
        }
        foreach (Transform c in gardenGO.transform)
            if (c && c.CompareTag("Sprout")) return c.gameObject;
        return null;
    }

    static void TrySetGardenSpotPlantedFlower(GameObject gardenGO, GameObject flowerGO)
    {
        var mb = GetGardenSpotBehaviour(gardenGO);
        if (mb == null) return;
        var mi = mb.GetType().GetMethod("SetPlantedFlower", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (mi != null && mi.GetParameters().Length == 1)
        { try { mi.Invoke(mb, new object[] { flowerGO }); } catch { } }
    }

    static MonoBehaviour GetGardenSpotBehaviour(GameObject gardenGO)
    {
        foreach (var mb in gardenGO.GetComponents<MonoBehaviour>())
        { if (mb && mb.GetType().Name == "GardenSpot") return mb; }
        return null;
    }

    GameObject GetPrefabFor(string key)
    {
        foreach (var e in prefabMap)
            if (!string.IsNullOrEmpty(e.key) && e.key == key && e.prefab != null) return e.prefab;
        return null;
    }

    static void SafeSetPrivateBool(object obj, string fieldName, bool value)
    {
        if (obj == null) return;
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(bool)) f.SetValue(obj, value);
    }

    // Ensure at least one visible SpriteRenderer after restore/hand-back.
    static void EnsureRenderableVisible(GameObject flower, bool isLit)
    {
        if (!flower) return;

        var spr = flower.GetComponent<SproutAndLightManager>();
        var all = flower.GetComponentsInChildren<SpriteRenderer>(true);
        if (all == null || all.Length == 0) return;

        foreach (var r in all)
            if (r && r.enabled && r.gameObject.activeInHierarchy && r.color.a > 0.01f) return; // already visible

        if (spr && spr.litFlowerRenderer && isLit)
        {
            spr.litFlowerRenderer.enabled = true;
            var c = spr.litFlowerRenderer.color; if (c.a < 0.99f) { c.a = 1f; spr.litFlowerRenderer.color = c; }
            return;
        }

        foreach (var r in all)
        {
            if (!r) continue;
            if (spr && spr.litFlowerRenderer == r && !isLit) continue; // avoid lit renderer when unlit
            r.enabled = true;
            var c = r.color; if (c.a < 0.99f) { c.a = 1f; r.color = c; }
            return;
        }

        var rr = all[0];
        rr.enabled = true;
        var cc = rr.color; if (cc.a < 0.99f) { cc.a = 1f; rr.color = cc; }
    }

}
