using UnityEngine;
using UnityEngine.SceneManagement;

// Attach to MinimalCameraRig (child of GameManager)
public class CameraRigSceneGate : MonoBehaviour
{
    public string[] gameplayScenes = { "Level0_Meadow", "Level0_MiniCave" };

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Toggle(SceneManager.GetActiveScene().name);
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnSceneLoaded(Scene s, LoadSceneMode m) => Toggle(s.name);

    void Toggle(string sceneName)
    {
        bool enable = System.Array.IndexOf(gameplayScenes, sceneName) >= 0;
        gameObject.SetActive(enable);
    }
}
