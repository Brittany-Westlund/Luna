using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class StartNewGame : MonoBehaviour
{
    [SerializeField] private string firstLevel;
    [SerializeField] private CollectibleState worldState;

    public void LoadFirstLevel()
    {
        Time.timeScale = 1f;

        // Stop dialogue and reset runtime systems first.
        GameTransitionUtility.PrepareForSceneChange(
            resetDialogueDatabase: true,
            resetPersistentDialogueData: true
        );

        // Clear collectible JSON / global state
        if (worldState != null)
        {
            Debug.Log("[StartNewGame] Clearing global collectible state...");
            worldState.ResetAll();
        }

        // Clear runtime collectible cache
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.ResetAll();
            Debug.Log("[StartNewGame] CollectibleManager reset complete.");
        }

        // Clear visited book page history
        BookPageHistory.ResetAll();

        // Clear all PlayerPrefs-backed saves
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("[StartNewGame] PlayerPrefs cleared.");

        SceneManager.LoadScene(firstLevel);
    }
}