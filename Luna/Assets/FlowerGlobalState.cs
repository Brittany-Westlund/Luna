using UnityEngine;
using System.Collections.Generic;

public class FlowerGlobalState : MonoBehaviour
{
    public static FlowerGlobalState Instance;

    // Tracks all picked flower IDs
    private HashSet<string> pickedFlowers = new HashSet<string>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Returns true if this flower has already been picked.
    /// </summary>
    public bool IsFlowerPicked(string flowerID)
    {
        return pickedFlowers.Contains(flowerID);
    }

    /// <summary>
    /// Record a flower as picked so it won’t respawn.
    /// </summary>
    public void MarkFlowerPicked(string flowerID)
    {
        if (!pickedFlowers.Contains(flowerID))
        {
            pickedFlowers.Add(flowerID);
            Debug.Log($"🌸 Flower {flowerID} marked as picked.");
        }
    }

    /// <summary>
    /// Optional: clear all flower state (e.g., on New Game).
    /// </summary>
    public void ResetAll()
    {
        pickedFlowers.Clear();
        Debug.Log("🌸 All flower states reset.");
    }
}
