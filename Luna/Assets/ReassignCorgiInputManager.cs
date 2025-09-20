using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(Character))]
public class ReassignCorgiInputManager : MonoBehaviour
{
    private Character _character;

    void Awake()
    {
        _character = GetComponent<Character>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InputManager input = FindObjectOfType<InputManager>();
        if (_character != null && input != null)
        {
            _character.SetInputManager(input);
            Debug.Log("🎮 Reassigned InputManager to Luna after scene load.");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not reassign InputManager to Luna.");
        }
    }
}
