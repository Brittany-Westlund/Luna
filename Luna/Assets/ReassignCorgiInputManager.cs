using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;
using System.Collections;

[RequireComponent(typeof(Character))]
public class ReassignCorgiInputManager : MonoBehaviour
{
    private Character _character;

    private void Awake()
    {
        _character = GetComponent<Character>();
        SceneManager.sceneLoaded += (_, __) => StartCoroutine(BindWhenReady());
    }

    private IEnumerator BindWhenReady()
    {
        float timeout = 5f;
        float t = 0f;
        InputManager im = null;

        while (t < timeout)
        {
            im = FindObjectOfType<InputManager>(true);
            if (im != null && im.gameObject.activeInHierarchy) break;
            t += Time.unscaledDeltaTime;
            yield return null;
        }

        if (_character != null && im != null)
        {
            _character.SetInputManager(im);
            Debug.Log("🎮 Reassigned InputManager to Luna after scene load.");
        }
        else
        {
            Debug.LogWarning("⚠️ Could not reassign InputManager to Luna (still missing/disabled).");
        }
    }
}
