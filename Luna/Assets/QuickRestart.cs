using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

public class QuickRestart : MonoBehaviour
{
    [Tooltip("Fade out before reloading?")]
    public bool Fade = true;

    public void RestartLevel()
    {
        Debug.Log("🔁 Quick restart triggered.");

        // 🧊 Unpause and normalize time
        Time.timeScale = 1f;
        try { CorgiEngineEvent.Trigger(CorgiEngineEventTypes.UnPause); } catch { }

        // 🧹 Optional: reset collectibles if you want a true clean restart
        var collectibleManager = FindObjectOfType<CollectibleManager>();
        if (collectibleManager != null)
            collectibleManager.ResetAll();

        // 🧭 Reload current level through Corgi’s LevelManager for safe handling
        var currentScene = SceneManager.GetActiveScene().name;
        LevelManager.Instance.GotoLevel(currentScene, Fade, false);
    }
}
