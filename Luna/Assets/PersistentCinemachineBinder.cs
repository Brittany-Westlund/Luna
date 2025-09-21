using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using Cinemachine;
using MoreMountains.CorgiEngine;

public class PersistentCinemachineBinder : MonoBehaviour
{
    [Header("References (Persistent)")]
    [Tooltip("Root of your persistent camera rig (e.g., MinimalCameraRig).")]
    public GameObject rigRoot;                              // your persistent rig GameObject

    [Tooltip("The active Cinemachine Virtual Camera under your rig.")]
    public CinemachineVirtualCamera vcam;                   // the vcam that should follow Luna

    [Tooltip("Optional 2D confiner on the vcam (assign if you use 2D bounds).")]
    public CinemachineConfiner2D confiner2D;                // optional

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
        for (int i = 0; i < Mathf.Max(0, framesToWait); i++) { yield return null; }

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
