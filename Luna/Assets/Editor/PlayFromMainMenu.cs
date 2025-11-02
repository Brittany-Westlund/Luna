using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PlayFromMainMenu
{
    static PlayFromMainMenu()
    {
        EditorApplication.playModeStateChanged += OnPlayModeChanged;
    }

    private static void OnPlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingEditMode)
        {
            // Find a scene named "MainMenu" automatically
            string mainMenuPath = null;
            foreach (var scene in EditorBuildSettings.scenes)
            {
                if (scene.path.Contains("MainMenu"))
                {
                    mainMenuPath = scene.path;
                    break;
                }
            }

            if (string.IsNullOrEmpty(mainMenuPath))
            {
                Debug.LogWarning("⚠️ Couldn't find a scene named 'MainMenu' in Build Settings.");
                return;
            }

            // Save current scene so you can go back to it later
            string currentScene = SceneManager.GetActiveScene().path;
            EditorPrefs.SetString("LastOpenedSceneBeforePlay", currentScene);

            // Only switch if you’re not already in MainMenu
            if (currentScene != mainMenuPath)
            {
                EditorSceneManager.OpenScene(mainMenuPath);
                Debug.Log("🎬 Starting Play Mode from MainMenu.");
            }
        }
        else if (state == PlayModeStateChange.EnteredEditMode)
        {
            // Restore the previous scene after you stop playing
            string previousScene = EditorPrefs.GetString("LastOpenedSceneBeforePlay", "");
            if (!string.IsNullOrEmpty(previousScene) && previousScene != SceneManager.GetActiveScene().path)
            {
                EditorSceneManager.OpenScene(previousScene);
                Debug.Log("🔙 Restored previous scene after Play Mode.");
            }
        }
    }
}
