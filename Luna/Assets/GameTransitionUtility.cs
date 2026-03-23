using UnityEngine;
using PixelCrushers.DialogueSystem;

public static class GameTransitionUtility
{
    public static void PrepareForSceneChange(bool resetDialogueDatabase = false, bool resetPersistentDialogueData = false)
    {
        // Always restore time before changing scenes.
        Time.timeScale = 1f;

        // Stop active conversation first.
        if (DialogueManager.instance != null)
        {
            try
            {
                if (DialogueManager.isConversationActive)
                {
                    DialogueManager.StopConversation();
                    Debug.Log("[GameTransitionUtility] Stopped active conversation before scene change.");
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"[GameTransitionUtility] StopConversation threw exception: {ex.Message}");
            }

            if (resetDialogueDatabase)
            {
                try
                {
                    Lua.Run("for k,v in pairs(_G) do if type(v) ~= 'function' then _G[k] = nil end end");
                    DialogueManager.ResetDatabase();
                    Debug.Log("[GameTransitionUtility] Dialogue database + Lua reset.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GameTransitionUtility] Dialogue reset threw exception: {ex.Message}");
                }
            }

            if (resetPersistentDialogueData)
            {
                try
                {
                    PersistentDataManager.Reset();
                    Debug.Log("[GameTransitionUtility] Persistent dialogue data reset.");
                }
                catch (System.Exception ex)
                {
                    Debug.LogWarning($"[GameTransitionUtility] PersistentDataManager.Reset threw exception: {ex.Message}");
                }
            }
        }

        // Reset all runtime BookControllerSimple instances still alive in memory.
        BookControllerSimple[] books = Object.FindObjectsByType<BookControllerSimple>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (BookControllerSimple book in books)
        {
            if (book != null)
            {
                book.DebugResetSave();
                Debug.Log($"[GameTransitionUtility] Reset runtime book controller on '{book.name}'.");
            }
        }
    }
}