using UnityEngine;
using UnityEngine.SceneManagement;

public class QuickSceneLoader : MonoBehaviour
{
    // Call this from Dialogue System and type the scene name
    public void LoadSceneByName(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("QuickSceneLoader: Scene name is empty.");
            return;
        }

        Debug.Log("QuickSceneLoader: Loading scene -> " + sceneName);
        SceneManager.LoadScene(sceneName);
    }
}