using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class StartNewGame : MonoBehaviour
{
    [SerializeField] private string firstLevel = "Level0_Meadow";

    [Header("Optional Global State Reference")]
    [Tooltip("Drag your global CollectibleState asset here.")]
    public CollectibleState worldState;

    public void LoadFirstLevel()
    {
        Time.timeScale = 1f;

        // 🌿 Clear CollectibleState JSON data
        if (worldState != null)
        {
            Debug.Log("[StartNewGame] Clearing global collectible state...");
            worldState.ResetAll();
        }

        // 🌿 Clear the CollectibleManager's runtime cache
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.ResetAll();
            Debug.Log("[StartNewGame] CollectibleManager reset complete.");
        }

        // 🗣 Reset Dialogue System (compatible with older API)
        if (DialogueManager.instance != null)
        {
            // Wipe Lua globals
            Lua.Run("for k,v in pairs(_G) do if type(v) ~= 'function' then _G[k] = nil end end");

            // Reset the dialogue database (no parameters in older versions)
            DialogueManager.ResetDatabase();

            // Reset any persistent data
            PersistentDataManager.Reset();

            Debug.Log("[StartNewGame] Dialogue System reset (database + Lua + persistent data).");
        }

        // 💾 Clear Unity PlayerPrefs
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();

        // 🌱 Load the first scene
        SceneManager.LoadScene(firstLevel);
    }
}
