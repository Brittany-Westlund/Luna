using UnityEngine;
using PixelCrushers.DialogueSystem;

public class ObjectActivationWatcherByName : MonoBehaviour
{
    [Header("Target To Watch (Exact Name)")]
    public string targetObjectName;

    [Header("Reaction Targets")]
    public GameObject[] objectsToEnable;
    public GameObject[] objectsToDisable;

    public SpriteRenderer[] spritesToEnable;
    public SpriteRenderer[] spritesToDisable;

    [Header("Optional Dialogue Trigger")]
    public DialogueSystemTrigger dialogueTrigger;

    private GameObject watchedObject;
    private bool hasTriggered = false;

    void Start()
    {
        TryFindTarget();
    }

    void Update()
    {
        if (hasTriggered) return;

        if (watchedObject == null)
        {
            TryFindTarget();
            return;
        }

        if (watchedObject.activeInHierarchy)
        {
            TriggerReactions();
        }
    }

    private void TryFindTarget()
    {
        if (string.IsNullOrEmpty(targetObjectName)) return;

        watchedObject = GameObject.Find(targetObjectName);
    }

    private void TriggerReactions()
    {
        hasTriggered = true;

        // ✅ Enable objects
        foreach (var go in objectsToEnable)
            if (go != null) go.SetActive(true);

        // ✅ Disable objects
        foreach (var go in objectsToDisable)
            if (go != null) go.SetActive(false);

        // ✅ Enable sprites
        foreach (var sr in spritesToEnable)
            if (sr != null) sr.enabled = true;

        // ✅ Disable sprites
        foreach (var sr in spritesToDisable)
            if (sr != null) sr.enabled = false;

        // ✅ Fire dialogue
        if (dialogueTrigger != null)
        {
            dialogueTrigger.enabled = false;
            dialogueTrigger.enabled = true;
        }
    }
} 
