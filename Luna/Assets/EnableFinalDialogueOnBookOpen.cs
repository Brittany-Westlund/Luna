using UnityEngine;
using PixelCrushers.DialogueSystem;

public class EnableFinalDialogueOnBookOpen : MonoBehaviour
{
    [Header("Enable this when the book opens (it should start inactive)")]
    public GameObject finalDialogueGO;

    [Header("Book state source (best)")]
    public BookPageController book; // optional; will use Instance

    [Header("Only do it once")]
    public bool fireOnce = true;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool lastOpen = false;
    private bool fired = false;

    // If book opens while another convo is playing, we queue it.
    private bool pending = false;

    void Awake()
    {
        if (book == null && BookPageController.Instance != null)
            book = BookPageController.Instance;

        if (finalDialogueGO != null)
            finalDialogueGO.SetActive(false); // IMPORTANT: keep inactive until we want it

        lastOpen = IsOpen();
    }

    void Update()
    {
        if (book == null && BookPageController.Instance != null)
            book = BookPageController.Instance;

        bool openNow = IsOpen();

        // detect transition closed -> open
        if (openNow && !lastOpen)
        {
            if (!fireOnce || !fired)
            {
                pending = true;
                if (debugLogs) Debug.Log("📘 FinalDialogue queued (book opened).");
            }
        }

        lastOpen = openNow;

        // If queued, wait until Dialogue System is not in a conversation
        if (pending)
        {
            if (!IsAnyConversationActive())
            {
                pending = false;
                fired = true;

                if (finalDialogueGO != null)
                {
                    finalDialogueGO.SetActive(true); // this should trigger your OnEnable -> Start Conversation
                    if (debugLogs) Debug.Log("📘 FinalDialogue enabled (no convo active).");
                }
                else
                {
                    Debug.LogWarning("EnableFinalDialogueOnBookOpen: finalDialogueGO is null.");
                }
            }
            else
            {
                if (debugLogs) Debug.Log("📘 Waiting… another conversation is still active.");
            }
        }
    }

    bool IsOpen()
    {
        return (book != null) ? book.IsOpen : false;
    }

    bool IsAnyConversationActive()
    {
        // Dialogue System API (most common):
        // If your DS version uses a different name, tell me the compile error text and I’ll swap it.
        return DialogueManager.isConversationActive;
    }
}