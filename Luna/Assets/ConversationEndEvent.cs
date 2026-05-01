using UnityEngine;
using UnityEngine.SceneManagement;

public class ConversationEndSceneLoader : MonoBehaviour
{
    [Header("Conversation Match")]
    [Tooltip("Type the exact conversation title this applies to.")]
    public string conversationTitle;

    [Header("Scene To Load")]
    [Tooltip("Type the exact scene name from Build Settings.")]
    public string sceneName;

    public void LoadAssignedScene()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogWarning("ConversationEndSceneLoader: Scene name is empty.");
            return;
        }

        Debug.Log("ConversationEndSceneLoader: Conversation ended -> " + conversationTitle);
        Debug.Log("ConversationEndSceneLoader: Loading scene -> " + sceneName);

        SceneManager.LoadScene(sceneName);
    }
}