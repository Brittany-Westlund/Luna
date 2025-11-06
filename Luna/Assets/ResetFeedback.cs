using UnityEngine;

public class ResetCollectiblesButton : MonoBehaviour
{
    [Tooltip("Reference to your CollectibleState asset.")]
    public CollectibleState collectibleState;

    // Called by your UI Button
    public void ResetProgress()
    {
        if (collectibleState == null)
        {
            Debug.LogWarning("⚠️ No CollectibleState asset assigned.");
            return;
        }

        collectibleState.ResetAll();

#if UNITY_WEBGL && !UNITY_EDITOR
        Debug.Log("🧹 Reset pressed (WebGL) — old save data cleared.");
#else
        Debug.Log("🧹 Reset pressed — collectibles cleared for fresh playthrough.");
#endif
    }
}
