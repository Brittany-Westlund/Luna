using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuReset : MonoBehaviour
{
    [Tooltip("Exact name of your Main Menu scene.")]
    public string mainMenuSceneName = "MainMenu";

    [Tooltip("Name of your first gameplay scene.")]
    public string firstLevelName = "Level0_Beginning";

    public void ReturnToMainMenu()
    {
        Debug.Log("🔄 Returning to Main Menu – full reset.");

        // 🧊 Unpause if paused
        Time.timeScale = 1f;

        // 🧹 Destroy all DontDestroyOnLoad objects so next scene boots fresh
        foreach (var root in FindObjectsOfType<GameObject>())
        {
            if (root.scene.name == null || root.scene.name == string.Empty)
                Destroy(root);
        }

        // 🚀 Load Main Menu scene cleanly
        SceneManager.LoadScene(mainMenuSceneName, LoadSceneMode.Single);
    }

    public void StartNewGame()
    {
        Debug.Log("🌙 Starting new game…");

        Time.timeScale = 1f;
        SceneManager.LoadScene(firstLevelName, LoadSceneMode.Single);
    }

    // 🧩 Optional: restore input on scene load (if needed)
    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == mainMenuSceneName) return;

        // Ensure InputManager exists so Luna can move again
        var input = GameObject.Find("InputManager");
        if (input == null)
        {
            // Try to find prefab if you keep it in Resources (optional)
            var prefab = Resources.Load<GameObject>("InputManager");
            if (prefab != null)
            {
                Instantiate(prefab);
                Debug.Log("🎮 Recreated InputManager from Resources.");
            }
            else
            {
                Debug.Log("ℹ️ No InputManager prefab found — skipping recreation.");
            }
        }

        Time.timeScale = 1f;
    }
}
