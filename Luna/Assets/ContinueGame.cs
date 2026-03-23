using UnityEngine;
using UnityEngine.SceneManagement;

public class ContinueGame : MonoBehaviour
{
    [SerializeField] private string fallbackScene = "Level0_Meadow";

    public void LoadContinue()
    {
        Time.timeScale = 1f;

        GameTransitionUtility.PrepareForSceneChange(
            resetDialogueDatabase: false,
            resetPersistentDialogueData: false
        );

        if (LotusSavePoint.HasSavedGame())
        {
            string scene = LotusSavePoint.GetSavedScene(fallbackScene);
            SceneManager.LoadScene(scene);
        }
        else
        {
            SceneManager.LoadScene(fallbackScene);
        }
    }
}