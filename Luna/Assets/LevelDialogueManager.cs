using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class LevelDialogueManager : MonoBehaviour
{
    [Header("Player Save/Load")]
    public string playerTag = "Player";
    public bool loadSavedPositionOnStart = true;

    [Header("Dialogue Persistence")]
    public bool persistDialogueState = true;
    public string saveKeyPrefix = "LDM_";

    [Header("Conversation Cancellation")]
    [Tooltip("If true, active conversations will be cancelled when the player changes scenes or the scene reloads.")]
    public bool cancelConversationOnSceneChange = true;

    [Tooltip("If true, active conversations can be cancelled when the player walks too far away from the conversant.")]
    public bool cancelConversationWhenWalkingAway = true;

    [Tooltip("Default max distance allowed before cancelling if the entry does not override it.")]
    public float defaultCancelDistance = 2.5f;

    [Header("Prompt Refresh")]
    [Tooltip("This increments whenever dialogue state changes. Prompt scripts can watch this to refresh without requiring re-collision.")]
    [SerializeField] private int dialogueStateVersion = 0;

    public enum ConditionType
    {
        ConversationPlayed,
        ConversationNotPlayed,
        ConversationTerminated,
        ConversationNotTerminated,
        GameObjectActive,
        GameObjectInactive,
        SpriteRendererEnabled,
        SpriteRendererDisabled,
        LuaVariableBoolTrue,
        LuaVariableBoolFalse
    }

    public enum EffectTargetType
    {
        GameObject,
        SpriteRenderer
    }

    public enum EffectActionType
    {
        SetGameObjectActiveTrue,
        SetGameObjectActiveFalse,
        SetSpriteRendererEnabledTrue,
        SetSpriteRendererEnabledFalse
    }

    public enum EffectTiming
    {
        OnConversationStart,
        OnConversationEnd
    }

    [Serializable]
    public class DialogueCondition
    {
        public ConditionType conditionType;

        [Tooltip("Used for conversation IDs or Lua variable names.")]
        public string stringValue;

        public GameObject targetObject;
        public SpriteRenderer targetSpriteRenderer;
    }

    [Serializable]
    public class DialogueEffect
    {
        [Header("Basic Effect")]
        public EffectTargetType targetType = EffectTargetType.GameObject;
        public EffectTiming timing = EffectTiming.OnConversationStart;
        public EffectActionType actionType = EffectActionType.SetGameObjectActiveTrue;

        [Tooltip("If false, this effect only happens the first time this dialogue entry plays. If true, it can happen again on repeat plays.")]
        public bool allowRepeatTrigger = false;

        [Header("Standard Targets")]
        public GameObject targetObject;
        public SpriteRenderer targetSpriteRenderer;

        [Header("Optional Custom Interaction Feedback")]
        [Tooltip("If true, this effect will also trigger a CustomInteractionFeedback on the target below.")]
        public bool useCustomInteractionFeedback = false;

        [Tooltip("Parent object that has CustomInteractionFeedback on it.")]
        public GameObject customInteractionFeedbackTarget;

        [Header("Optional One-Shot SFX")]
        [Tooltip("If true, this effect will also play a sound.")]
        public bool playSFX = false;

        [Tooltip("Sound to play when this effect is triggered.")]
        public AudioClip sfxClip;

        [Range(0f, 1f)]
        [Tooltip("Volume for the sound effect.")]
        public float sfxVolume = 1f;

        [Tooltip("If true, play the sound at the target's world position. If false, play at this manager's position.")]
        public bool spatializeSFX = false;
    }

    [Serializable]
    public class DialogueEntry
    {
        [Header("Identity")]
        public string entryID;
        public string actorID;
        public string conversationTitle;

        [Header("Flow")]
        public int order = 0;
        public bool repeatable = false;
        public bool isFallback = false;
        public bool markPlayedAfterStart = false;

        [Header("Immediate Play")]
        [Tooltip("If true, this conversation can auto-play as soon as it becomes newly available.")]
        public bool playImmediatelyWhenAvailable = false;

        [Tooltip("Optional conversant transform to use for immediate play. Recommended for auto-play entries.")]
        public Transform immediateConversantOverride;

        [Tooltip("If true, walking away can cancel this conversation.")]
        public bool cancelIfPlayerWalksAway = true;

        [Tooltip("If > 0, overrides the manager's default cancel distance for this entry.")]
        public float cancelDistanceOverride = 0f;

        [Header("Availability Conditions")]
        public List<DialogueCondition> conditions = new List<DialogueCondition>();

        [Header("Termination")]
        [Tooltip("If true, this entry will permanently terminate once all termination conditions are met.")]
        public bool terminateWhenConditionsMet = false;

        [Tooltip("All termination conditions in this list must pass before this entry is permanently terminated.")]
        public List<DialogueCondition> terminationConditions = new List<DialogueCondition>();

        [Header("Triggered Effects")]
        public List<DialogueEffect> effects = new List<DialogueEffect>();
    }

    [Serializable]
    public class PromptState
    {
        public bool showPrompt;
        public float alpha;
        public DialogueEntry entry;

        public PromptState(bool showPrompt, float alpha, DialogueEntry entry)
        {
            this.showPrompt = showPrompt;
            this.alpha = alpha;
            this.entry = entry;
        }
    }

    [Header("Dialogue Entries For This Level")]
    public List<DialogueEntry> entries = new List<DialogueEntry>();

    [Header("Prompt Settings")]
    [Range(0f, 1f)]
    public float repeatedFallbackAlpha = 0.4f;

    [Header("Debug")]
    public bool debugLogging = false;

    public static event Action OnDialogueStateChanged;

    public int DialogueStateVersion => dialogueStateVersion;

    private DialogueEntry activeEntry;
    private bool activeEntryWasAlreadyPlayed = false;
    private bool activeConversationWasCancelled = false;

    private Transform activePlayerTransform;
    private Transform activeConversantTransform;
    private string activeActorID;

    private Transform cachedPlayerTransform;

    // Tracks whether an entry was available last frame, for immediate-play transitions.
    private readonly Dictionary<string, bool> lastAvailabilityByEntryID = new Dictionary<string, bool>();

    private void Start()
    {
        RestoreDialogueStateToLua();
        CachePlayerTransform();

        if (DialogueManager.instance != null)
        {
            DialogueManager.instance.conversationEnded -= OnConversationEnded;
            DialogueManager.instance.conversationEnded += OnConversationEnded;
        }

        SceneManager.activeSceneChanged += OnActiveSceneChanged;
        SceneManager.sceneLoaded += OnSceneLoaded;

        InitializeAvailabilityTracking();

        if (loadSavedPositionOnStart)
            StartCoroutine(LoadSavedPositionRoutine());

        if (debugLogging)
            Debug.Log($"[LevelDialogueManager] Start complete on scene '{SceneManager.GetActiveScene().name}'.");
    }

    private void OnDisable()
    {
        if (DialogueManager.instance != null)
            DialogueManager.instance.conversationEnded -= OnConversationEnded;

        SceneManager.activeSceneChanged -= OnActiveSceneChanged;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Update()
    {
        CachePlayerTransform();
        UpdateTerminationStates();
        UpdateImmediatePlayEntries();
        UpdateActiveConversationCancellation();
    }

    private void CachePlayerTransform()
    {
        if (cachedPlayerTransform != null)
            return;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
            cachedPlayerTransform = player.transform;
    }

    private void InitializeAvailabilityTracking()
    {
        lastAvailabilityByEntryID.Clear();

        foreach (DialogueEntry entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.entryID))
                continue;

            bool available = IsEntryAvailableForSelection(entry);
            lastAvailabilityByEntryID[entry.entryID] = available;
        }
    }

    private void UpdateImmediatePlayEntries()
    {
        if (entries == null || entries.Count == 0)
            return;

        if (DialogueManager.isConversationActive)
            return;

        foreach (DialogueEntry entry in entries.OrderBy(e => e != null ? e.order : int.MaxValue))
        {
            if (entry == null)
                continue;

            if (!entry.playImmediatelyWhenAvailable)
                continue;

            if (string.IsNullOrWhiteSpace(entry.entryID))
                continue;

            bool nowAvailable = IsEntryAvailableForSelection(entry);
            bool wasAvailable = lastAvailabilityByEntryID.TryGetValue(entry.entryID, out bool previous) && previous;

            lastAvailabilityByEntryID[entry.entryID] = nowAvailable;

            // Only auto-fire on transition from unavailable -> available.
            if (!nowAvailable || wasAvailable)
                continue;

            Transform playerTransform = cachedPlayerTransform;
            Transform conversantTransform = entry.immediateConversantOverride;

            if (playerTransform == null)
            {
                if (debugLogging)
                    Debug.LogWarning($"[LevelDialogueManager] Cannot immediate-play '{entry.entryID}' because player transform is missing.");
                continue;
            }

            if (conversantTransform == null)
            {
                if (debugLogging)
                    Debug.LogWarning($"[LevelDialogueManager] Cannot immediate-play '{entry.entryID}' because immediateConversantOverride is not assigned.");
                continue;
            }

            if (debugLogging)
                Debug.Log($"[LevelDialogueManager] Immediate-playing entry '{entry.entryID}' because it just became available.");

            TryStartConversationByEntryID(entry.entryID, playerTransform, conversantTransform);
            return;
        }
    }

    private void UpdateActiveConversationCancellation()
    {
        if (!cancelConversationWhenWalkingAway)
            return;

        if (!DialogueManager.isConversationActive)
            return;

        if (activeEntry == null)
            return;

        if (!activeEntry.cancelIfPlayerWalksAway)
            return;

        if (activePlayerTransform == null || activeConversantTransform == null)
            return;

        float allowedDistance = activeEntry.cancelDistanceOverride > 0f
            ? activeEntry.cancelDistanceOverride
            : defaultCancelDistance;

        float distance = Vector2.Distance(activePlayerTransform.position, activeConversantTransform.position);

        if (distance > allowedDistance)
        {
            CancelActiveConversation($"Player walked away from '{activeEntry.entryID}' (distance {distance:0.00} > {allowedDistance:0.00}).");
        }
    }

    private void OnActiveSceneChanged(Scene oldScene, Scene newScene)
    {
        if (!cancelConversationOnSceneChange)
            return;

        if (DialogueManager.isConversationActive)
            CancelActiveConversation($"Scene changed from '{oldScene.name}' to '{newScene.name}'.");
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!cancelConversationOnSceneChange)
            return;

        if (DialogueManager.isConversationActive)
            CancelActiveConversation($"Scene '{scene.name}' loaded.");
    }

    public void CancelActiveConversation(string reason)
    {
        if (!DialogueManager.isConversationActive)
            return;

        if (debugLogging)
            Debug.Log($"[LevelDialogueManager] Cancelling conversation. Reason: {reason}");

        activeConversationWasCancelled = true;

        try
        {
            DialogueManager.StopConversation();
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LevelDialogueManager] StopConversation threw exception: {ex.Message}");
        }

        // Safety cleanup in case the dialogue system doesn't fire conversationEnded in this scenario.
        if (!DialogueManager.isConversationActive)
        {
            ClearActiveConversationState();
            NotifyDialogueStateChanged();
        }
    }

    private void ClearActiveConversationState()
    {
        activeEntry = null;
        activeEntryWasAlreadyPlayed = false;
        activeConversationWasCancelled = false;
        activePlayerTransform = null;
        activeConversantTransform = null;
        activeActorID = null;
    }

    private void UpdateTerminationStates()
    {
        if (entries == null || entries.Count == 0)
            return;

        foreach (DialogueEntry entry in entries)
        {
            if (entry == null)
                continue;

            if (!entry.terminateWhenConditionsMet)
                continue;

            if (HasBeenTerminated(entry.entryID))
                continue;

            if (AreAllConditionsMet(entry.terminationConditions))
            {
                MarkTerminated(entry.entryID);

                if (debugLogging)
                    Debug.Log($"[LevelDialogueManager] Permanently terminated entry '{entry.entryID}'.");
            }
        }
    }

    public PromptState GetPromptState(string actorID)
    {
        if (string.IsNullOrWhiteSpace(actorID))
            return new PromptState(false, 0f, null);

        List<DialogueEntry> actorEntries = entries
            .Where(e => e != null && e.actorID == actorID)
            .OrderBy(e => e.order)
            .ToList();

        if (actorEntries.Count == 0)
            return new PromptState(false, 0f, null);

        DialogueEntry fallbackCandidate = null;

        foreach (DialogueEntry entry in actorEntries)
        {
            if (!EntryExistsInDatabase(entry))
                continue;

            if (HasBeenTerminated(entry.entryID))
            {
                if (debugLogging)
                    Debug.Log($"[LevelDialogueManager] Skipping terminated entry '{entry.entryID}'.");
                continue;
            }

            if (!AreConditionsMet(entry))
                continue;

            bool played = HasBeenPlayed(entry.entryID);

            if (!entry.repeatable && played)
                continue;

            if (entry.isFallback)
            {
                if (fallbackCandidate == null)
                    fallbackCandidate = entry;

                continue;
            }

            if (debugLogging)
                Debug.Log($"[LevelDialogueManager] Bright prompt for actor '{actorID}' using entry '{entry.entryID}'.");

            return new PromptState(true, 1f, entry);
        }

        if (fallbackCandidate != null)
        {
            bool fallbackPlayed = HasBeenPlayed(fallbackCandidate.entryID);
            float alpha = fallbackPlayed ? repeatedFallbackAlpha : 1f;

            if (debugLogging)
                Debug.Log($"[LevelDialogueManager] Fallback prompt for actor '{actorID}' using entry '{fallbackCandidate.entryID}'. Alpha: {alpha}");

            return new PromptState(true, alpha, fallbackCandidate);
        }

        return new PromptState(false, 0f, null);
    }

    public bool TryStartConversation(string actorID, Transform playerTransform, Transform conversantTransform)
    {
        if (DialogueManager.isConversationActive)
            return false;

        PromptState state = GetPromptState(actorID);

        if (!state.showPrompt || state.entry == null)
            return false;

        return StartEntryInternal(state.entry, actorID, playerTransform, conversantTransform);
    }

    public bool TryStartConversationByEntryID(string entryID, Transform playerTransform = null, Transform conversantTransform = null)
    {
        if (DialogueManager.isConversationActive)
            return false;

        if (string.IsNullOrWhiteSpace(entryID))
            return false;

        DialogueEntry entry = entries.FirstOrDefault(e => e != null && e.entryID == entryID);
        if (entry == null)
        {
            if (debugLogging)
                Debug.LogWarning($"[LevelDialogueManager] TryStartConversationByEntryID could not find entryID '{entryID}'.");
            return false;
        }

        if (!EntryExistsInDatabase(entry))
            return false;

        if (HasBeenTerminated(entry.entryID))
            return false;

        if (!AreConditionsMet(entry))
            return false;

        bool played = HasBeenPlayed(entry.entryID);
        if (!entry.repeatable && played)
            return false;

        Transform resolvedPlayer = playerTransform != null ? playerTransform : cachedPlayerTransform;
        Transform resolvedConversant = conversantTransform != null ? conversantTransform : entry.immediateConversantOverride;

        if (resolvedPlayer == null || resolvedConversant == null)
        {
            if (debugLogging)
                Debug.LogWarning($"[LevelDialogueManager] Cannot start entry '{entryID}' because player or conversant transform is missing.");
            return false;
        }

        return StartEntryInternal(entry, entry.actorID, resolvedPlayer, resolvedConversant);
    }

    private bool StartEntryInternal(DialogueEntry entry, string actorID, Transform playerTransform, Transform conversantTransform)
    {
        activeEntry = entry;
        activeEntryWasAlreadyPlayed = HasBeenPlayed(activeEntry.entryID);
        activeConversationWasCancelled = false;

        activePlayerTransform = playerTransform;
        activeConversantTransform = conversantTransform;
        activeActorID = actorID;

        if (debugLogging)
        {
            Debug.Log($"[LevelDialogueManager] Starting conversation '{activeEntry.conversationTitle}' for actor '{actorID}'. EntryID='{activeEntry.entryID}'. Already played: {activeEntryWasAlreadyPlayed}");
        }

        ApplyEffects(activeEntry, EffectTiming.OnConversationStart, activeEntryWasAlreadyPlayed);

        DialogueManager.StartConversation(
            activeEntry.conversationTitle,
            playerTransform,
            conversantTransform
        );

        if (activeEntry.markPlayedAfterStart)
        {
            MarkPlayed(activeEntry.entryID);

            if (debugLogging)
                Debug.Log($"[LevelDialogueManager] Marked played on start: {activeEntry.entryID}");
        }

        NotifyDialogueStateChanged();
        return true;
    }

    public bool AreConditionsMet(DialogueEntry entry)
    {
        if (entry == null)
            return false;

        return AreAllConditionsMet(entry.conditions);
    }

    public bool AreAllConditionsMet(List<DialogueCondition> conditions)
    {
        if (conditions == null || conditions.Count == 0)
            return true;

        foreach (DialogueCondition condition in conditions)
        {
            if (!EvaluateCondition(condition))
                return false;
        }

        return true;
    }

    private bool EvaluateCondition(DialogueCondition condition)
    {
        if (condition == null)
            return false;

        switch (condition.conditionType)
        {
            case ConditionType.ConversationPlayed:
                return HasBeenPlayed(condition.stringValue);

            case ConditionType.ConversationNotPlayed:
                return !HasBeenPlayed(condition.stringValue);

            case ConditionType.ConversationTerminated:
                return HasBeenTerminated(condition.stringValue);

            case ConditionType.ConversationNotTerminated:
                return !HasBeenTerminated(condition.stringValue);

            case ConditionType.GameObjectActive:
                return condition.targetObject != null && condition.targetObject.activeInHierarchy;

            case ConditionType.GameObjectInactive:
                return condition.targetObject != null && !condition.targetObject.activeInHierarchy;

            case ConditionType.SpriteRendererEnabled:
                return condition.targetSpriteRenderer != null && condition.targetSpriteRenderer.enabled;

            case ConditionType.SpriteRendererDisabled:
                return condition.targetSpriteRenderer != null && !condition.targetSpriteRenderer.enabled;

            case ConditionType.LuaVariableBoolTrue:
                return !string.IsNullOrWhiteSpace(condition.stringValue) &&
                       DialogueLua.GetVariable(condition.stringValue).asBool;

            case ConditionType.LuaVariableBoolFalse:
                return !string.IsNullOrWhiteSpace(condition.stringValue) &&
                       !DialogueLua.GetVariable(condition.stringValue).asBool;
        }

        return false;
    }

    private bool IsEntryAvailableForSelection(DialogueEntry entry)
    {
        if (entry == null)
            return false;

        if (!EntryExistsInDatabase(entry))
            return false;

        if (HasBeenTerminated(entry.entryID))
            return false;

        if (!AreConditionsMet(entry))
            return false;

        bool played = HasBeenPlayed(entry.entryID);
        if (!entry.repeatable && played)
            return false;

        return true;
    }

    private void ApplyEffects(DialogueEntry entry, EffectTiming timing, bool wasAlreadyPlayed)
    {
        if (entry == null || entry.effects == null || entry.effects.Count == 0)
            return;

        foreach (DialogueEffect effect in entry.effects)
        {
            if (effect == null)
                continue;

            if (effect.timing != timing)
                continue;

            if (wasAlreadyPlayed && !effect.allowRepeatTrigger)
                continue;

            ApplySingleEffect(effect, entry.entryID);
        }
    }

    private void ApplySingleEffect(DialogueEffect effect, string entryID)
    {
        if (effect == null)
            return;

        if (effect.useCustomInteractionFeedback && effect.customInteractionFeedbackTarget != null)
        {
            CustomInteractionFeedback feedback = effect.customInteractionFeedbackTarget.GetComponent<CustomInteractionFeedback>();

            if (feedback != null)
            {
                effect.customInteractionFeedbackTarget.SetActive(true);
                feedback.RefreshDisplay();

                if (debugLogging)
                {
                    Debug.Log($"[LevelDialogueManager] Effect on '{entryID}': Triggered CustomInteractionFeedback on '{effect.customInteractionFeedbackTarget.name}'.");
                }
            }
            else if (debugLogging)
            {
                Debug.LogWarning($"[LevelDialogueManager] Effect on '{entryID}': customInteractionFeedbackTarget '{effect.customInteractionFeedbackTarget.name}' has no CustomInteractionFeedback.");
            }
        }

        switch (effect.actionType)
        {
            case EffectActionType.SetGameObjectActiveTrue:
                if (effect.targetObject != null)
                {
                    effect.targetObject.SetActive(true);

                    if (debugLogging)
                        Debug.Log($"[LevelDialogueManager] Effect on '{entryID}': SetActive(true) on GameObject '{effect.targetObject.name}'.");
                }
                break;

            case EffectActionType.SetGameObjectActiveFalse:
                if (effect.targetObject != null)
                {
                    effect.targetObject.SetActive(false);

                    if (debugLogging)
                        Debug.Log($"[LevelDialogueManager] Effect on '{entryID}': SetActive(false) on GameObject '{effect.targetObject.name}'.");
                }
                break;

            case EffectActionType.SetSpriteRendererEnabledTrue:
                if (effect.targetSpriteRenderer != null)
                {
                    effect.targetSpriteRenderer.enabled = true;

                    if (debugLogging)
                        Debug.Log($"[LevelDialogueManager] Effect on '{entryID}': SpriteRenderer enabled on '{effect.targetSpriteRenderer.name}'.");
                }
                break;

            case EffectActionType.SetSpriteRendererEnabledFalse:
                if (effect.targetSpriteRenderer != null)
                {
                    effect.targetSpriteRenderer.enabled = false;

                    if (debugLogging)
                        Debug.Log($"[LevelDialogueManager] Effect on '{entryID}': SpriteRenderer disabled on '{effect.targetSpriteRenderer.name}'.");
                }
                break;
        }

        if (effect.playSFX && effect.sfxClip != null)
        {
            Vector3 playPosition = transform.position;

            if (effect.spatializeSFX)
            {
                if (effect.targetObject != null)
                    playPosition = effect.targetObject.transform.position;
                else if (effect.targetSpriteRenderer != null)
                    playPosition = effect.targetSpriteRenderer.transform.position;
            }

            AudioSource.PlayClipAtPoint(effect.sfxClip, playPosition, effect.sfxVolume);

            if (debugLogging)
                Debug.Log($"[LevelDialogueManager] Effect on '{entryID}': Played SFX '{effect.sfxClip.name}'.");
        }
    }

    public void MarkPlayed(string entryID)
    {
        if (string.IsNullOrWhiteSpace(entryID))
            return;

        string luaVar = GetPlayedVariableName(entryID);
        DialogueLua.SetVariable(luaVar, true);

        if (persistDialogueState)
        {
            PlayerPrefs.SetInt(GetPlayedPrefKey(entryID), 1);
            PlayerPrefs.Save();
        }

        if (debugLogging)
            Debug.Log($"[LevelDialogueManager] MarkPlayed -> entryID='{entryID}', luaVar='{luaVar}'");

        NotifyDialogueStateChanged();
    }

    public bool HasBeenPlayed(string entryID)
    {
        if (string.IsNullOrWhiteSpace(entryID))
            return false;

        return DialogueLua.GetVariable(GetPlayedVariableName(entryID)).asBool;
    }

    public string GetPlayedVariableName(string entryID)
    {
        return $"Played_{entryID}";
    }

    public void MarkTerminated(string entryID)
    {
        if (string.IsNullOrWhiteSpace(entryID))
            return;

        string luaVar = GetTerminatedVariableName(entryID);
        DialogueLua.SetVariable(luaVar, true);

        if (persistDialogueState)
        {
            PlayerPrefs.SetInt(GetTerminatedPrefKey(entryID), 1);
            PlayerPrefs.Save();
        }

        if (debugLogging)
            Debug.Log($"[LevelDialogueManager] MarkTerminated -> entryID='{entryID}', luaVar='{luaVar}'");

        NotifyDialogueStateChanged();
    }

    public bool HasBeenTerminated(string entryID)
    {
        if (string.IsNullOrWhiteSpace(entryID))
            return false;

        return DialogueLua.GetVariable(GetTerminatedVariableName(entryID)).asBool;
    }

    public string GetTerminatedVariableName(string entryID)
    {
        return $"Terminated_{entryID}";
    }

    public bool EntryExistsInDatabase(DialogueEntry entry)
    {
        if (entry == null || string.IsNullOrWhiteSpace(entry.conversationTitle))
            return false;

        if (DialogueManager.masterDatabase == null)
            return false;

        return DialogueManager.masterDatabase.GetConversation(entry.conversationTitle) != null;
    }

    private void OnConversationEnded(Transform actor)
    {
        if (activeEntry == null)
            return;

        if (!activeConversationWasCancelled)
        {
            ApplyEffects(activeEntry, EffectTiming.OnConversationEnd, activeEntryWasAlreadyPlayed);

            if (!activeEntry.markPlayedAfterStart)
            {
                MarkPlayed(activeEntry.entryID);

                if (debugLogging)
                    Debug.Log($"[LevelDialogueManager] Marked played on end: {activeEntry.entryID}");
            }
        }
        else if (debugLogging)
        {
            Debug.Log($"[LevelDialogueManager] Conversation '{activeEntry.entryID}' ended due to cancellation. Not marking played.");
        }

        ClearActiveConversationState();
        NotifyDialogueStateChanged();
    }

    private IEnumerator LoadSavedPositionRoutine()
    {
        yield return null;

        GameObject player = GameObject.FindGameObjectWithTag(playerTag);

        if (player != null)
            LotusSavePoint.TryLoadSavedPosition(player.transform, debugLogging);
        else if (debugLogging)
            Debug.LogWarning("[LevelDialogueManager] Player not found with tag: " + playerTag);
    }

    private void RestoreDialogueStateToLua()
    {
        if (entries == null || entries.Count == 0)
            return;

        foreach (DialogueEntry entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.entryID))
                continue;

            bool played = persistDialogueState && PlayerPrefs.GetInt(GetPlayedPrefKey(entry.entryID), 0) == 1;
            bool terminated = persistDialogueState && PlayerPrefs.GetInt(GetTerminatedPrefKey(entry.entryID), 0) == 1;

            DialogueLua.SetVariable(GetPlayedVariableName(entry.entryID), played);
            DialogueLua.SetVariable(GetTerminatedVariableName(entry.entryID), terminated);

            if (debugLogging)
                Debug.Log($"[LevelDialogueManager] RestoreDialogueStateToLua -> entryID='{entry.entryID}', played={played}, terminated={terminated}");
        }

        NotifyDialogueStateChanged();
    }

    private string GetPlayedPrefKey(string entryID)
    {
        return $"{saveKeyPrefix}{SceneManager.GetActiveScene().name}_Played_{entryID}";
    }

    private string GetTerminatedPrefKey(string entryID)
    {
        return $"{saveKeyPrefix}{SceneManager.GetActiveScene().name}_Terminated_{entryID}";
    }

    private void NotifyDialogueStateChanged()
    {
        dialogueStateVersion++;
        OnDialogueStateChanged?.Invoke();

        if (debugLogging)
            Debug.Log($"[LevelDialogueManager] Dialogue state changed. Version={dialogueStateVersion}");
    }

    [ContextMenu("DEBUG: Reset Dialogue State For This Scene")]
    public void DebugResetDialogueStateForScene()
    {
        if (entries == null)
            return;

        foreach (DialogueEntry entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.entryID))
                continue;

            PlayerPrefs.DeleteKey(GetPlayedPrefKey(entry.entryID));
            PlayerPrefs.DeleteKey(GetTerminatedPrefKey(entry.entryID));

            DialogueLua.SetVariable(GetPlayedVariableName(entry.entryID), false);
            DialogueLua.SetVariable(GetTerminatedVariableName(entry.entryID), false);
        }

        PlayerPrefs.Save();
        InitializeAvailabilityTracking();
        NotifyDialogueStateChanged();

        if (debugLogging)
            Debug.Log($"[LevelDialogueManager] Reset dialogue state for scene '{SceneManager.GetActiveScene().name}'.");
    }
}