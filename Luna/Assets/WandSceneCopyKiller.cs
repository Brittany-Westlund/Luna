using UnityEngine;
using UnityEngine.SceneManagement;

/// Attach to the *scene* wand prefab/instance.
/// If any other LunariaWandAttractor exists (e.g., the one under Luna),
/// this scene copy destroys (or disables) itself.
[DefaultExecutionOrder(100)]
public class WandSceneCopyKiller : MonoBehaviour
{
    public bool destroyInsteadOfDisable = true;
    public string playerTag = "Player";
    public bool recheckOnSceneLoad = true;

    void OnEnable()
    {
        TrySelfDestruct();
        if (recheckOnSceneLoad) SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (recheckOnSceneLoad) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // next frame so Player/wand hierarchy is ready
        StartCoroutine(NextFrame());
    }

    System.Collections.IEnumerator NextFrame()
    {
        yield return null;
        TrySelfDestruct();
    }

    void TrySelfDestruct()
    {
        // If there's any LunariaWandAttractor in the world that's not this object,
        // we assume the player already has a wand → remove this scene copy.
        var allWands = FindObjectsOfType<LunariaWandAttractor>(true);
        foreach (var w in allWands)
        {
            if (w && w.gameObject != gameObject)
            {
                KillMe();
                return;
            }
        }

        // Secondary check: specifically look under the Player
        var player = GameObject.FindWithTag(playerTag);
        if (player)
        {
            var owned = player.GetComponentInChildren<LunariaWandAttractor>(true);
            if (owned && owned.gameObject != gameObject)
            {
                KillMe();
            }
        }
    }

    void KillMe()
    {
        if (destroyInsteadOfDisable) Destroy(gameObject);
        else gameObject.SetActive(false);
    }
}
