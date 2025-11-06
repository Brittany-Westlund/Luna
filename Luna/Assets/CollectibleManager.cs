using UnityEngine;

public class CollectibleManager : MonoBehaviour
{
    [Header("Reference to your global CollectibleState asset")]
    [SerializeField] private CollectibleState collectibleState;

    private static CollectibleManager instance;

    private void Awake()
    {
        // Singleton pattern so you can safely access from anywhere
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public static CollectibleManager Instance => instance;

    // Called when player picks up a collectible
    public void MarkCollected(string id)
    {
        collectibleState.MarkCollected(id);
    }

    // Called by collectibles to check if they should exist
    public bool HasCollected(string id)
    {
        return collectibleState.HasCollected(id);
    }
    private void OnApplicationQuit() => collectibleState.Save();

    public void ResetAll()
{
    if (collectibleState != null)
    {
        collectibleState.ResetAll();
        Debug.Log("[CollectibleManager] Global collectible state reset.");
    }
    else
    {
        Debug.LogWarning("[CollectibleManager] No CollectibleState assigned — cannot reset.");
    }
}


}
