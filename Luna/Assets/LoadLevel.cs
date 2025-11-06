using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadLevel : MonoBehaviour
{
    [SerializeField] private string levelName;

    public void LoadScene()
    {
        Time.timeScale = 1f;

        // ✅ No resets here! We just carry on with the current progress.
        Debug.Log($"[LoadLevel] Loading {levelName} without resetting collectibles...");

        SceneManager.LoadScene(levelName);
    }
}
