using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenuController : MonoBehaviour
{
    [SerializeField] private GameObject pauseRoot;
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused = false;

    private void Awake()
    {
        if (pauseRoot == null)
            pauseRoot = gameObject;

        pauseRoot.SetActive(false);
    }

    private void Update()
    {
        if (SceneManager.GetActiveScene().name == mainMenuSceneName)
            return;

        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePause();
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseRoot.SetActive(isPaused);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void Resume()
    {
        isPaused = false;
        pauseRoot.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        Resume();

        GameTransitionUtility.PrepareForSceneChange(
            resetDialogueDatabase: false,
            resetPersistentDialogueData: false
        );

        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMainMenu()
    {
        Resume();

        GameTransitionUtility.PrepareForSceneChange(
            resetDialogueDatabase: false,
            resetPersistentDialogueData: false
        );

        SceneManager.LoadScene(mainMenuSceneName);
    }
}