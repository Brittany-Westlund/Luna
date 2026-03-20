using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;
using MoreMountains.CorgiEngine;

public class PersistentCinemachineBinder : MonoBehaviour
{
    [Header("References (Persistent)")]
    [Tooltip("Root of your persistent camera rig (e.g., MinimalCameraRig).")]
    public GameObject rigRoot; // your persistent rig GameObject

    [Tooltip("The active Cinemachine Virtual Camera under your rig.")]
    public CinemachineVirtualCamera vcam; // the vcam that should follow Luna

    [Tooltip("Optional 2D confiner on the vcam (assign if you use 2D bounds).")]
    public CinemachineConfiner2D confiner2D; // optional

    [Tooltip("How many frames to wait after a scene load before binding (lets LevelManager spawn Luna).")]
    public int framesToWait = 3;

    [Header("Scene Filtering")]
    [Tooltip("Only flip on / bind in scenes whose name starts with this (leave empty to bind everywhere).")]
    public string gameplayPrefix = "Level";

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        // Also handle the starting scene (eg. if you start directly in a level)
        StartCoroutine(RebindNextFrames());
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StartCoroutine(RebindNextFrames());
    }

    private IEnumerator RebindNextFrames()
    {
        // gate by scene name if you want
        var s = SceneManager.GetActiveScene();
        if (!string.IsNullOrEmpty(gameplayPrefix) && !s.name.StartsWith(gameplayPrefix))
        {
            // Non-gameplay (e.g., MainMenu): we keep the rig enabled but it won’t follow anything.
            // If you prefer, you can disable it here:
            // if (rigRoot) rigRoot.SetActive(false);
            yield break;
        }

        // 1) Force the persistent rig ON
        if (rigRoot != null && !rigRoot.activeSelf)
        {
            rigRoot.SetActive(true);
            Debug.Log("🎥 PersistentBinder: enabled rigRoot.");
        }

        // 2) Wait a few frames so LevelManager can spawn the player
        for (int i = 0; i < Mathf.Max(0, framesToWait); i++)
        {
            yield return null;
        }

        // 3) Make sure vcam exists and is active
        if (vcam == null)
        {
            vcam = FindObjectOfType<CinemachineVirtualCamera>(true);
            if (vcam != null) Debug.Log("🎥 PersistentBinder: auto-found vcam.");
        }

        if (vcam == null)
        {
            Debug.LogWarning("🎥 PersistentBinder: No CinemachineVirtualCamera found under persistent rig.");
            yield break;
        }

        if (!vcam.gameObject.activeInHierarchy)
        {
            vcam.gameObject.SetActive(true);
            Debug.Log("🎥 PersistentBinder: enabled vcam.");
            yield return null; // let CM initialize
        }

        // 4) Find Luna (must be tagged Player)
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogWarning("🧍 PersistentBinder: No GameObject tagged 'Player' found. Is Luna tagged correctly?");
            yield break;
        }

        // 5) Bind follow target
        vcam.Follow = playerGO.transform;
        Debug.Log($"🎥 PersistentBinder: vcam now following {playerGO.name} in scene '{s.name}'.");

        // 6) Hook a 2D confiner if available in scene
        if (confiner2D == null)
        {
            confiner2D = vcam.GetComponent<CinemachineConfiner2D>();
        }

        if (confiner2D != null)
        {
            // Try to get bounds from LevelManager (preferred)
            var lm = LevelManager.Instance;
            if (lm != null && lm.BoundsCollider2D != null)
            {
                confiner2D.m_BoundingShape2D = lm.BoundsCollider2D;
                Debug.Log("🧭 PersistentBinder: Confiner2D bound to LevelManager.BoundsCollider2D.");
            }
            else
            {
                // Fallback: any CompositeCollider2D in the scene
                var composite = FindObjectOfType<UnityEngine.CompositeCollider2D>();
                if (composite != null)
                {
                    confiner2D.m_BoundingShape2D = composite;
                    Debug.Log("🧭 PersistentBinder: Confiner2D bound to scene CompositeCollider2D.");
                }
                else
                {
                    Debug.LogWarning("🧭 PersistentBinder: No 2D confiner bounds found in this scene.");
                }
            }
        }

        // 7) FINAL sanity: ensure rig is active if we got this far
        if (rigRoot != null && !rigRoot.activeSelf)
        {
            rigRoot.SetActive(true);
        }
    }
}
/* using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;
using MoreMountains.CorgiEngine;

public class PersistentCinemachineBinder : MonoBehaviour
{
    [Header("References (Persistent)")]
    [Tooltip("Root of your persistent camera rig (e.g., MinimalCameraRig).")]
    public GameObject rigRoot;

    [Tooltip("The Cinemachine Virtual Camera under your persistent rig.")]
    public CinemachineVirtualCamera vcam;

    [Tooltip("Optional confiner on the vcam (NEW Cinemachine Confiner, not Confiner2D).")]
    public CinemachineConfiner confiner2D;

    [Header("Frame Waits")]
    [Tooltip("How many frames to wait after a gameplay scene loads (lets LevelManager spawn Luna).")]
    public int gameplayFramesToWait = 3;

    [Tooltip("How many frames to wait in menu scenes (usually 0 is fine).")]
    public int menuFramesToWait = 0;

    [Header("Scene Filtering")]
    [Tooltip("Treat scenes whose name starts with this as gameplay scenes (bind Follow to Player).")]
    public string gameplayPrefix = "Level";

    [Header("Scene Overrides")]
    [Tooltip("If this exact scene name matches, we can force it to be stationary even though it is gameplay.")]
    public string teaRoomSceneName = "Level0_TeaRoom";

    [Tooltip("If true, the tea room scene will NOT follow Luna (stationary camera).")]
    public bool teaRoomStationary = true;

    [Tooltip("If true, we clear vcam.Follow in non-gameplay scenes (menus).")]
    public bool clearFollowInNonGameplayScenes = true;

    [Header("Menu Camera Anchor")]
    [Tooltip("Name of a Transform in your menu scenes for the camera to sit on (e.g., MenuCamAnchor).")]
    public string menuAnchorName = "MenuCamAnchor";

    [Tooltip("If true, snap the virtual camera to the menu anchor in non-gameplay scenes.")]
    public bool snapToMenuAnchorInNonGameplay = true;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        StartCoroutine(RebindNextFrames());
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        StartCoroutine(RebindNextFrames());
    }

    private IEnumerator RebindNextFrames()
    {
        Scene s = SceneManager.GetActiveScene();

        // 0) Ensure persistent rig is enabled
        if (rigRoot != null && !rigRoot.activeSelf)
        {
            rigRoot.SetActive(true);
            Debug.Log("🎥 PersistentBinder: enabled rigRoot.");
        }

        // 1) Ensure vcam reference
        if (vcam == null)
        {
            vcam = FindObjectOfType<CinemachineVirtualCamera>(true);
            if (vcam != null) Debug.Log("🎥 PersistentBinder: auto-found vcam.");
        }

        if (vcam == null)
        {
            Debug.LogWarning("🎥 PersistentBinder: No CinemachineVirtualCamera found.");
            yield break;
        }

        // 2) Ensure vcam active
        if (!vcam.gameObject.activeInHierarchy)
        {
            vcam.gameObject.SetActive(true);
            Debug.Log("🎥 PersistentBinder: enabled vcam.");
            yield return null;
        }

        // 3) Determine gameplay vs non-gameplay
        bool isGameplay = string.IsNullOrEmpty(gameplayPrefix) ? true : s.name.StartsWith(gameplayPrefix);
        bool isTeaRoom = (!string.IsNullOrEmpty(teaRoomSceneName) && s.name == teaRoomSceneName);

        // 4) MENU / NON-GAMEPLAY behavior
        if (!isGameplay)
        {
            // optional small wait
            for (int i = 0; i < Mathf.Max(0, menuFramesToWait); i++) { yield return null; }

            if (clearFollowInNonGameplayScenes)
            {
                vcam.Follow = null;
            }

            if (snapToMenuAnchorInNonGameplay)
            {
                Transform anchor = FindMenuAnchor(menuAnchorName);
                if (anchor != null)
                {
                    // Position the VCam to anchor (works well when Follow is null + FramingTransposer)
                    vcam.transform.position = anchor.position;
                    vcam.transform.rotation = anchor.rotation;

                    Debug.Log($"🎥 PersistentBinder: snapped vcam to menu anchor '{anchor.name}' in scene '{s.name}'.");
                }
                else
                {
                    Debug.LogWarning($"🎥 PersistentBinder: menu anchor '{menuAnchorName}' not found in scene '{s.name}'.");
                }
            }

            yield break;
        }

        // 5) GAMEPLAY behavior (with Tea Room override)
        // If tea room is stationary, we do NOT bind Follow to Player
        if (isTeaRoom && teaRoomStationary)
        {
            // optional wait (usually fine to still wait a couple frames, but not required)
            for (int i = 0; i < Mathf.Max(0, gameplayFramesToWait); i++) { yield return null; }

            vcam.Follow = null;

            // Confiner can still be bound if you want it bounded even while stationary
            BindConfinerIfPresent();

            Debug.Log($"🎥 PersistentBinder: Tea room stationary; cleared Follow in scene '{s.name}'.");
            yield break;
        }

        // 6) Wait gameplay frames so LevelManager spawns player
        for (int i = 0; i < Mathf.Max(0, gameplayFramesToWait); i++) { yield return null; }

        // 7) Find player (tagged Player)
        GameObject playerGO = GameObject.FindWithTag("Player");
        if (playerGO == null)
        {
            Debug.LogWarning($"🧍 PersistentBinder: No GameObject tagged 'Player' found in scene '{s.name}'.");
            yield break;
        }

        // 8) Bind follow target
        vcam.Follow = playerGO.transform;
        Debug.Log($"🎥 PersistentBinder: vcam now following {playerGO.name} in scene '{s.name}'.");

        // 9) Bind confiner bounds (if confiner exists)
        BindConfinerIfPresent();

        // 10) Final sanity: ensure rig is active
        if (rigRoot != null && !rigRoot.activeSelf)
        {
            rigRoot.SetActive(true);
        }
    }

    private void BindConfinerIfPresent()
    {
        // Auto-find Confiner if missing reference
        if (confiner2D == null && vcam != null)
        {
            confiner2D = vcam.GetComponent<CinemachineConfiner>();
        }

        if (confiner2D == null)
        {
            // no confiner on this vcam; nothing to do
            return;
        }

        Collider2D bounds = null;

        // Preferred: LevelManager bounds (Corgi)
        var lm = LevelManager.Instance;
        if (lm != null && lm.BoundsCollider2D != null)
        {
            bounds = lm.BoundsCollider2D;
            confiner2D.m_BoundingShape2D = bounds;
            Debug.Log("🧭 PersistentBinder: Confiner bound to LevelManager.BoundsCollider2D.");
            return;
        }

        // Fallback: any CompositeCollider2D in scene
        var composite = FindObjectOfType<CompositeCollider2D>();
        if (composite != null)
        {
            bounds = composite;
            confiner2D.m_BoundingShape2D = bounds;
            Debug.Log("🧭 PersistentBinder: Confiner bound to scene CompositeCollider2D.");
            return;
        }

        Debug.LogWarning("🧭 PersistentBinder: Confiner present, but no bounds collider found to assign.");
    }

    private Transform FindMenuAnchor(string anchorObjectName)
    {
        if (string.IsNullOrEmpty(anchorObjectName)) return null;

        GameObject go = GameObject.Find(anchorObjectName);
        if (go != null) return go.transform;

        // Extra fallback: search inactive objects too
        Transform[] all = Resources.FindObjectsOfTypeAll<Transform>();
        foreach (var t in all)
        {
            if (t != null && t.name == anchorObjectName && t.gameObject.scene == SceneManager.GetActiveScene())
                return t;
        }

        return null;
    }
} */