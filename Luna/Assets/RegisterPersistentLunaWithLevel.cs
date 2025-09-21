using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using MoreMountains.CorgiEngine;

[DisallowMultipleComponent]
public class RegisterPersistentLunaWithLevel : MonoBehaviour
{
    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        TryRegister(); // also try in the current scene (e.g., when starting from MainMenu)
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        TryRegister();
    }

    private void TryRegister()
    {
        if (_character == null) return;

        var lm = FindObjectOfType<LevelManager>();
        if (lm == null) return;

        // Make sure SceneCharacters exists and contains Luna
        if (lm.SceneCharacters == null)
            lm.SceneCharacters = new List<Character>();

        if (!lm.SceneCharacters.Contains(_character))
            lm.SceneCharacters.Add(_character);

        // If no checkpoint is set yet, pick the first one in the scene
        if (lm.CurrentCheckPoint == null)
        {
            var firstCP = FindObjectOfType<CheckPoint>();
            if (firstCP != null)
                lm.SetCurrentCheckpoint(firstCP);
        }
    }
}
