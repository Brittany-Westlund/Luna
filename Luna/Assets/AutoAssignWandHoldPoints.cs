using UnityEngine;
using UnityEngine.SceneManagement;

/// Attach this to the Wand object (same GO that has LunariaWandAttractor).
/// It finds the hold points by name under the Player and fills them in automatically.
[DefaultExecutionOrder(-1000)]
public class AutoAssignWandHoldPoints : MonoBehaviour
{
    [Header("Where to search")]
    public string playerTag = "Player";

    [Header("Names to find under the Player")]
    public string groundHoldPointName = "WandHoldPoint";
    public string flightHoldPointName = "WandHoldPoint_Fly"; // optional; if not found, uses ground

    [Header("When to assign")]
    public bool assignOnAwake = true;
    public bool reassignOnSceneLoad = true;

    private LunariaWandAttractor _wand;

    void Awake()
    {
        _wand = GetComponent<LunariaWandAttractor>();
        if (assignOnAwake) AssignNow();
        if (reassignOnSceneLoad) SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        if (reassignOnSceneLoad) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene s, LoadSceneMode m)
    {
        // next frame so Player hierarchy is ready
        StartCoroutine(AssignNextFrame());
    }

    System.Collections.IEnumerator AssignNextFrame()
    {
        yield return null;
        AssignNow();
    }

    public void AssignNow()
    {
        if (_wand == null)
        {
            Debug.LogWarning("AutoAssignWandHoldPoints: LunariaWandAttractor not found on this object.");
            return;
        }

        var player = GameObject.FindWithTag(playerTag);
        if (!player)
        {
            Debug.LogWarning("AutoAssignWandHoldPoints: No object with tag '" + playerTag + "' found.");
            return;
        }

        Transform ground = FindDeepChild(player.transform, groundHoldPointName);
        Transform flight = FindDeepChild(player.transform, flightHoldPointName);

        // If only one is present, use it for both so the wand logic doesn't hit null
        if (ground == null && flight == null)
        {
            Debug.LogWarning("AutoAssignWandHoldPoints: No wand hold points found under Player.");
            return;
        }
        if (ground == null) ground = flight;
        if (flight == null) flight = ground;

        _wand.groundHoldPoint = ground;
        _wand.flightHoldPoint = flight;
        // Debug.Log($"Wand sockets: ground= {ground?.name}, flight= {flight?.name}");
    }

    private static Transform FindDeepChild(Transform root, string childName)
    {
        if (root == null || string.IsNullOrEmpty(childName)) return null;
        var all = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
            if (all[i] != null && all[i].name == childName) return all[i];
        return null;
    }
}
