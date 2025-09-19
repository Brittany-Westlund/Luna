using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class LevelLoader : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Optional: name of the starting level to auto-load")]
    public string firstLevelScene;

    private string _currentLevel;

    void Start()
    {
        if (!string.IsNullOrEmpty(firstLevelScene))
        {
            LoadLevel(firstLevelScene);
        }
    }

    /// <summary>
    /// Load a new level (scene) additively, and unload the previous one if needed.
    /// </summary>
    public void LoadLevel(string sceneName)
    {
        StartCoroutine(LoadLevelRoutine(sceneName));
    }

    private IEnumerator LoadLevelRoutine(string sceneName)
    {
        // Unload old level if one is loaded
        if (!string.IsNullOrEmpty(_currentLevel))
        {
            AsyncOperation unloadOp = SceneManager.UnloadSceneAsync(_currentLevel);
            if (unloadOp != null)
                while (!unloadOp.isDone)
                    yield return null;
        }

        // Load new level additively
        AsyncOperation loadOp = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
        if (loadOp != null)
        {
            while (!loadOp.isDone)
                yield return null;
        }

        // Set new level as active scene (so Instantiate, etc. work correctly)
        Scene newScene = SceneManager.GetSceneByName(sceneName);
        if (newScene.IsValid())
            SceneManager.SetActiveScene(newScene);

        _currentLevel = sceneName;
    }
}
