using UnityEngine;
using UnityEngine.SceneManagement;

/// Put this on the wand prefab that appears in scenes.
/// The first wand claims "singleton" status, moves under GameManagers XXXXX and persists.
/// Any later copies self-destroy so you never get duplicates.
[DefaultExecutionOrder(-1000)]
public class WandToManagerOnLoad : MonoBehaviour
{
    [Tooltip("Exact name of your persistent manager object")]
    public string managerObjectName = "GameManagers XXXXX";

    [Tooltip("If true, scene-spawned copies will destroy themselves when a canonical wand exists.")]
    public bool destroySceneCopies = true;

    // global canonical reference
    private static WandToManagerOnLoad _canonical;

    void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryClaimOrCull();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        // if the canonical is being destroyed (eg. quitting), release the slot
        if (_canonical == this) _canonical = null;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // next frame so the manager exists in the hierarchy
        StartCoroutine(NextFrameClaim());
    }

    System.Collections.IEnumerator NextFrameClaim()
    {
        yield return null;
        TryClaimOrCull();
    }

    private void TryClaimOrCull()
    {
        // If another wand already claimed singleton, this is a scene copy → remove it
        if (_canonical && _canonical != this)
        {
            if (destroySceneCopies) Destroy(gameObject);
            else gameObject.SetActive(false);
            return;
        }

        // Otherwise, this one becomes the canonical wand
        _canonical = this;

        // Find the persistent manager
        var manager = GameObject.Find(managerObjectName);
        if (!manager)
        {
            // Manager not found yet (e.g., load order) — just persist now; parent later
            DontDestroyOnLoad(gameObject);
            return;
        }

        // Persist & parent under manager so it lives across scenes
        DontDestroyOnLoad(gameObject);
        if (transform.parent != manager.transform)
            transform.SetParent(manager.transform, true);
    }
}
