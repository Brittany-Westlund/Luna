using UnityEngine;

public class QuitApplication : MonoBehaviour
{
    public void QuitGame()
    {
        Debug.Log("QuitApplication: Quitting game...");

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }
}