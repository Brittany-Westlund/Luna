using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class CameraRigSceneGate : MonoBehaviour
{
    [Header("Assign your persistent MinimalCameraRig here")]
    public GameObject rigRoot; // <- drag the MinimalCameraRig from the GameManager

    [Header("Scenes where the rig should be ON")]
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
        if (rigRoot == null) return;
        bool enable = Array.IndexOf(gameplayScenes, sceneName) >= 0;
        rigRoot.SetActive(enable);
    }
}
