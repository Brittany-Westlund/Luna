using UnityEngine;
using UnityEngine.SceneManagement;

public class HoldToMainMenu : MonoBehaviour
{
    [Header("Key Settings")]
    public KeyCode returnKey = KeyCode.M;
    public float holdTime = 1.5f;

    [Header("Scene Name")]
    public string mainMenuSceneName = "MainMenu";

    private float holdTimer = 0f;

    void Update()
    {
        if (Input.GetKey(returnKey))
        {
            holdTimer += Time.unscaledDeltaTime;

            if (holdTimer >= holdTime)
            {
                LoadMainMenu();
            }
        }
        else
        {
            holdTimer = 0f;
        }
    }

    private void LoadMainMenu()
    {
        holdTimer = 0f;
        SceneManager.LoadScene(mainMenuSceneName);
    }
}
