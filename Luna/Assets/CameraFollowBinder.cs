using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Reflection;
using MoreMountains.CorgiEngine;

public class UnifiedSceneBinder : MonoBehaviour
{
    [Header("Scene Gating (optional)")]
    public bool onlyInGameplayScenes = true;
    public string gameplayScenePrefix = "Level";

    [Header("Timing")]
    [Tooltip("Max time to wait for scene objects to be ready.")]
    public float bindTimeoutSeconds = 8f;

    private void OnEnable()  { SceneManager.sceneLoaded += OnSceneLoaded; }
    private void OnDisable() { SceneManager.sceneLoaded -= OnSceneLoaded; }
    private void OnDestroy() { SceneManager.sceneLoaded -= OnSceneLoaded; }

    private void Start()
    {
        var s = SceneManager.GetActiveScene();
        if (!onlyInGameplayScenes || s.name.StartsWith(gameplayScenePrefix))
            StartCoroutine(BindAllWhenReady());
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!onlyInGameplayScenes || scene.name.StartsWith(gameplayScenePrefix))
            StartCoroutine(BindAllWhenReady());
    }

    private IEnumerator BindAllWhenReady()
    {
        float t = 0f, timeout = Mathf.Max(0.25f, bindTimeoutSeconds);

        LevelManager lm = null;
        CinemachineCameraController cam = null;
        Character luna = null;

        // Wait for LevelManager, Controller, and Player(Character)
        while (t < timeout)
        {
            if (lm == null) lm = FindObjectOfType<LevelManager>(true);
            if (cam == null) cam = FindObjectOfType<CinemachineCameraController>(true);

            if (luna == null)
            {
                var playerGO = GameObject.FindWithTag("Player");
                if (playerGO) luna = playerGO.GetComponent<Character>();
            }

            if (lm != null && cam != null && cam.gameObject.activeInHierarchy && luna != null)
                break;

            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (lm == null || cam == null || luna == null)
        {
            if (lm == null)  Debug.LogWarning("🧭 Binder: LevelManager not found.");
            if (cam == null) Debug.LogWarning("📷 Binder: CinemachineCameraController not found.");
            if (luna == null) Debug.LogWarning("🧍 Binder: Player(Character) not found (is Luna tagged 'Player'?).");
            yield break;
        }

        // 1) Make sure LevelManager tracks Luna and uses index 0
        TrySetPlayerOnLevelManager(lm, luna);

        // 2) Stop the controller from reacting to early MMEvents
        DisableControllerMMEvents(cam);

        // 3) Give vcams/brains an extra frame to initialize
        yield return null;
        yield return new WaitForEndOfFrame();

        // 4) Bind follow safely (with a one-time retry)
        bool needRetry = false;
        try
        {
            cam.SetTarget(luna);
            cam.StartFollowing();
            Debug.Log("📷 UnifiedBinder: camera is now following Luna.");
        }
        catch (System.Exception e)
        {
            Debug.LogWarning("📷 UnifiedBinder: StartFollowing threw; will retry once. " + e.Message);
            needRetry = true;
        }

        if (needRetry)
        {
            yield return null;
            try
            {
                cam.SetTarget(luna);
                cam.StartFollowing();
                Debug.Log("📷 UnifiedBinder: second attempt succeeded.");
            }
            catch (System.Exception e2)
            {
                Debug.LogError("📷 UnifiedBinder: failed to bind camera. " + e2);
            }
        }
    }

    private void TrySetPlayerOnLevelManager(LevelManager lm, Character luna)
    {
        // Preferred public API if present
        var setPlayer = lm.GetType().GetMethod("SetPlayer", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (setPlayer != null)
        {
            try
            {
                setPlayer.Invoke(lm, new object[] { luna });
                Debug.Log("🧭 UnifiedBinder: LevelManager.SetPlayer(Luna) invoked.");
            }
            catch { /* fall through to fields */ }
        }

        // Reflection fallback: ensure lists contain Luna and index is 0
        TryEnsureListContains(lm, "Players", luna);
        TryEnsureListContains(lm, "CurrentPlayableCharacters", luna);
        TrySetIntField(lm, new[] { "CurrentPlayableCharacterIndex", "_currentPlayableCharacter", "CurrentPlayerIndex" }, 0);
    }

    private void TryEnsureListContains(object obj, string fieldName, Character luna)
    {
        var f = obj.GetType().GetField(fieldName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f == null) return;
        var list = f.GetValue(obj) as System.Collections.IList;
        if (list == null) return;

        bool has = false;
        foreach (var item in list) { if (ReferenceEquals(item, luna)) { has = true; break; } }
        if (!has)
        {
            list.Clear();
            list.Add(luna);
            Debug.Log($"🧭 UnifiedBinder: set {fieldName} = [Luna]");
        }
    }

    private void TrySetIntField(object obj, string[] names, int value)
    {
        foreach (var name in names)
        {
            var f = obj.GetType().GetField(name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (f != null && f.FieldType == typeof(int))
            {
                f.SetValue(obj, value);
                Debug.Log($"🧭 UnifiedBinder: {name} = {value}");
                return;
            }
        }
    }

    private void DisableControllerMMEvents(CinemachineCameraController cam)
    {
        var type = cam.GetType();

        // Property first
        var p = type.GetProperty("ListenToMMEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (p != null && p.PropertyType == typeof(bool))
        {
            p.SetValue(cam, false, null);
            Debug.Log("📷 UnifiedBinder: ListenToMMEvents (property) = false");
            return;
        }

        // Field fallback
        var f = type.GetField("ListenToMMEvents", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (f != null && f.FieldType == typeof(bool))
        {
            f.SetValue(cam, false);
            Debug.Log("📷 UnifiedBinder: ListenToMMEvents (field) = false");
        }
    }
}
