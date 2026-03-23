using UnityEngine;
using PixelCrushers.DialogueSystem;

public class BookSpreadUnlockTerminator : MonoBehaviour
{
    public enum WatchMode
    {
        UnlockCountIncreased,
        SpecificLocationUsed
    }

    [Header("Book Save Keys")]
    [SerializeField] private string saveKeyPrefix = "BOOK_SIMPLE_";

    [Header("Watch")]
    [SerializeField] private WatchMode watchMode = WatchMode.UnlockCountIncreased;

    [Tooltip("Used only if Watch Mode = SpecificLocationUsed")]
    [SerializeField] private string locationIdToWatch;

    [Header("Optional Dialogue System Lua")]
    [Tooltip("If set, this Lua bool will be set true when the unlock is detected.")]
    [SerializeField] private string luaBoolToSetTrue;

    [Header("Optional Direct Termination")]
    [SerializeField] private string entryIDToTerminate;

    [Header("Behavior")]
    [Tooltip("If true, only react to a newly detected unlock after Start. If false, already-saved unlocks can trigger immediately.")]
    [SerializeField] private bool requireNewUnlockAfterStart = true;

    [Tooltip("If true, this watcher disables itself after it triggers once.")]
    [SerializeField] private bool disableAfterTrigger = true;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = false;

    private string KeyUnlocked => saveKeyPrefix + "UnlockedCount";
    private string KeyUsedIds => saveKeyPrefix + "UsedLocationIds";

    private int startingUnlockedCount;
    private bool hasTriggered = false;
    private LevelDialogueManager levelDialogueManager;

    private void Start()
    {
        startingUnlockedCount = PlayerPrefs.GetInt(KeyUnlocked, 0);
        levelDialogueManager = FindFirstObjectByTypeCompat<LevelDialogueManager>();

        if (debugLogs)
        {
            Debug.Log($"[BookSpreadUnlockTerminator] Start. startingUnlockedCount={startingUnlockedCount}");
            Debug.Log($"[BookSpreadUnlockTerminator] Found LevelDialogueManager={(levelDialogueManager != null ? levelDialogueManager.name : "NULL")}");
        }

        if (!requireNewUnlockAfterStart)
        {
            CheckNow();
        }
    }

    private void Update()
    {
        if (hasTriggered) return;
        CheckNow();
    }

    private void CheckNow()
    {
        bool unlockedDetected = false;

        switch (watchMode)
        {
            case WatchMode.UnlockCountIncreased:
            {
                int currentUnlocked = PlayerPrefs.GetInt(KeyUnlocked, 0);
                unlockedDetected = requireNewUnlockAfterStart ? currentUnlocked > startingUnlockedCount : currentUnlocked > 0;

                if (debugLogs)
                {
                    Debug.Log($"[BookSpreadUnlockTerminator] UnlockCount check current={currentUnlocked}, start={startingUnlockedCount}, detected={unlockedDetected}");
                }
                break;
            }

            case WatchMode.SpecificLocationUsed:
            {
                if (!string.IsNullOrWhiteSpace(locationIdToWatch))
                {
                    string joined = PlayerPrefs.GetString(KeyUsedIds, "");
                    unlockedDetected = ContainsLocationId(joined, locationIdToWatch);

                    if (debugLogs)
                    {
                        Debug.Log($"[BookSpreadUnlockTerminator] Location check '{locationIdToWatch}', detected={unlockedDetected}");
                    }
                }
                break;
            }
        }

        if (!unlockedDetected) return;

        TriggerTermination();
    }

    private void TriggerTermination()
    {
        hasTriggered = true;

        if (!string.IsNullOrWhiteSpace(luaBoolToSetTrue))
        {
            DialogueLua.SetVariable(luaBoolToSetTrue, true);

            if (debugLogs)
            {
                Debug.Log($"[BookSpreadUnlockTerminator] Set Lua bool true: {luaBoolToSetTrue}");
            }
        }

        if (levelDialogueManager != null && !string.IsNullOrWhiteSpace(entryIDToTerminate))
        {
            levelDialogueManager.MarkTerminated(entryIDToTerminate);

            if (debugLogs)
            {
                Debug.Log($"[BookSpreadUnlockTerminator] Marked terminated: {entryIDToTerminate}");
            }
        }

        if (disableAfterTrigger)
        {
            enabled = false;
        }
    }

    private bool ContainsLocationId(string joined, string targetId)
    {
        if (string.IsNullOrEmpty(joined) || string.IsNullOrEmpty(targetId))
            return false;

        string[] parts = joined.Split('|');
        for (int i = 0; i < parts.Length; i++)
        {
            if (parts[i] == targetId)
                return true;
        }

        return false;
    }

    static T FindFirstObjectByTypeCompat<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindFirstObjectByType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }
}