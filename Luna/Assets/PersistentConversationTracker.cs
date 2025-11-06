using UnityEngine;
using PixelCrushers.DialogueSystem;

public class PersistentConversationTracker : MonoBehaviour
{
    [Tooltip("How often to check for NPCs that should stay disabled (on scene load).")]
    public float checkDelay = 0.5f;

    private void Awake()
    {
        // Apply saved disable states after a short delay to let scene finish loading
        Invoke(nameof(ApplySavedStates), checkDelay);
    }

    private void OnEnable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded += OnConversationEnd;
    }

    private void OnDisable()
    {
        if (DialogueManager.hasInstance)
            DialogueManager.instance.conversationEnded -= OnConversationEnd;
    }

    private void OnConversationEnd(Transform actor)
    {
        string convoName = DialogueManager.lastConversationStarted;
        if (string.IsNullOrEmpty(convoName)) return;

        // Save this conversation as "completed"
        PlayerPrefs.SetInt($"NPC_{convoName}_Disabled", 1);
        PlayerPrefs.Save();

        // Immediately disable any NPC whose name matches
        GameObject npc = GameObject.Find(convoName);
        if (npc != null)
        {
            npc.SetActive(false);
            Debug.Log($"[PersistentConversationTracker] Disabled and saved NPC '{convoName}'.");
        }
    }

    private void ApplySavedStates()
    {
        // Loop through all active root objects in the scene and disable any saved NPCs
        foreach (GameObject root in gameObject.scene.GetRootGameObjects())
        {
            foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
            {
                string key = $"NPC_{child.name}_Disabled";
                if (PlayerPrefs.GetInt(key, 0) == 1)
                {
                    child.gameObject.SetActive(false);
                    Debug.Log($"[PersistentConversationTracker] '{child.name}' kept disabled (saved state).");
                }
            }
        }
    }
}
