using UnityEngine;
using UnityEngine.SceneManagement;

public class FlowerGlobalState : MonoBehaviour
{
    public static FlowerGlobalState Instance;

    [Header("Held Flower (runtime only)")]
    [Tooltip("True while Luna is carrying a flower across scenes.")]
    public bool hasHeldFlower = false;

    // The actual flower object being carried (DontDestroyOnLoad)
    private GameObject heldFlower;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    /// <summary>Register the currently held flower and make sure it survives scene loads.</summary>
    public void RegisterHeldFlower(GameObject flower)
    {
        if (flower == null) return;

        hasHeldFlower = true;
        heldFlower = flower;

        // Ensure this specific GameObject persists across scenes
        DontDestroyOnLoad(heldFlower);

        Debug.Log($"🌼 [FlowerGlobalState] Registered held flower: {heldFlower.name}");
    }

    /// <summary>Clear if the given flower is the one we’re tracking.</summary>
    public void ClearIfThis(GameObject flower)
    {
        if (flower != null && flower == heldFlower)
        {
            hasHeldFlower = false;
            heldFlower = null;
            Debug.Log("🌼 [FlowerGlobalState] Cleared held flower.");
        }
    }

    /// <summary>After a scene loads, reattach the held flower to Luna's HoldPoint.</summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!hasHeldFlower || heldFlower == null) return;

        var luna = GameObject.FindWithTag("Player");
        if (luna == null)
        {
            Debug.LogWarning("🌼 [FlowerGlobalState] Player not found in new scene.");
            return;
        }

        // Your existing FlowerHolder on Luna should expose holdPoint.
        var holder = luna.GetComponentInChildren<FlowerHolder>();
        if (holder == null || holder.holdPoint == null)
        {
            Debug.LogWarning("🌼 [FlowerGlobalState] FlowerHolder or holdPoint not found on Player.");
            return;
        }

        // Re-parent back into Luna’s hand
        heldFlower.transform.SetParent(holder.holdPoint, true);
        heldFlower.transform.localPosition = Vector3.zero;
        heldFlower.transform.localRotation = Quaternion.identity;

        Debug.Log("🌼 [FlowerGlobalState] Reattached held flower to Luna after scene load.");
    }
}
