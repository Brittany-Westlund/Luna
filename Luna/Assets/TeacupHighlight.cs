using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeacupHighlight : MonoBehaviour
{
    [Header("Reference")]
    public GameObject highlightObject;

    [Header("Temporary Prompt Suppression")]
    public Transform promptParent;
    public bool ignoreHighlightObject = true;

    [Tooltip("Optional extra objects to force off while teacup highlight is active.")]
    public List<GameObject> extraObjectsToSuppress = new List<GameObject>();

    [Header("Custom Interaction Feedback")]
    public CustomInteractionFeedback customInteractionFeedback;
    public bool stopFeedbackCycleOnHighlight = true;
    public bool restartFeedbackCycleOnRemove = true;

    [Header("Optional Components To Disable")]
    [Tooltip("Optional components that should be temporarily disabled while teacup highlight is active.")]
    public List<Behaviour> componentsToDisable = new List<Behaviour>();

    [Header("Restore Delay")]
    [Tooltip("Delay before restoring prompts after tea is given.")]
    public float restoreDelay = 1.5f;

    private readonly List<GameObject> previouslyActiveObjects = new List<GameObject>();
    private readonly List<Behaviour> previouslyEnabledComponents = new List<Behaviour>();

    private bool highlightIsRunning = false;
    private Coroutine restoreCoroutine;

    private void Awake()
    {
        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        if (promptParent == null)
        {
            promptParent = transform;
        }
    }

    public void Highlight()
    {
        if (highlightObject == null)
            return;

        if (promptParent == null)
        {
            promptParent = transform;
        }

        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
            restoreCoroutine = null;
        }

        if (highlightIsRunning)
        {
            highlightObject.SetActive(true);
            return;
        }

        if (customInteractionFeedback != null && stopFeedbackCycleOnHighlight)
        {
            customInteractionFeedback.StopCycling();
        }

        DisableSpecifiedComponents();
        CacheAndDisableOtherPromptObjects();
        CacheAndDisableExtraObjects();

        highlightObject.SetActive(true);
        highlightIsRunning = true;
    }

    public void RemoveHighlight()
    {
        if (!highlightIsRunning)
        {
            if (highlightObject != null)
            {
                highlightObject.SetActive(false);
            }
            return;
        }

        if (highlightObject != null)
        {
            highlightObject.SetActive(false);
        }

        if (restoreCoroutine != null)
        {
            StopCoroutine(restoreCoroutine);
        }

        restoreCoroutine = StartCoroutine(RestoreAfterDelay());
        highlightIsRunning = false;
    }

    private IEnumerator RestoreAfterDelay()
    {
        yield return new WaitForSeconds(restoreDelay);

        RestorePreviouslyActivePromptObjects();
        RestorePreviouslyEnabledComponents();

        if (customInteractionFeedback != null && restartFeedbackCycleOnRemove)
        {
            if (!customInteractionFeedback.gameObject.activeInHierarchy)
            {
                customInteractionFeedback.gameObject.SetActive(true);
            }

            customInteractionFeedback.StartCycling();
        }

        restoreCoroutine = null;
    }

    private void CacheAndDisableOtherPromptObjects()
    {
        previouslyActiveObjects.Clear();

        if (promptParent == null)
            return;

        for (int i = 0; i < promptParent.childCount; i++)
        {
            Transform child = promptParent.GetChild(i);
            if (child == null)
                continue;

            GameObject childObject = child.gameObject;
            if (childObject == null)
                continue;

            if (ignoreHighlightObject && highlightObject != null && childObject == highlightObject)
                continue;

            if (childObject.activeSelf && !previouslyActiveObjects.Contains(childObject))
            {
                previouslyActiveObjects.Add(childObject);
                childObject.SetActive(false);
            }
        }
    }

    private void CacheAndDisableExtraObjects()
    {
        if (extraObjectsToSuppress == null || extraObjectsToSuppress.Count == 0)
            return;

        for (int i = 0; i < extraObjectsToSuppress.Count; i++)
        {
            GameObject obj = extraObjectsToSuppress[i];

            if (obj == null)
                continue;

            if (highlightObject != null && obj == highlightObject)
                continue;

            if (obj.activeSelf && !previouslyActiveObjects.Contains(obj))
            {
                previouslyActiveObjects.Add(obj);
                obj.SetActive(false);
            }
        }
    }

    private void RestorePreviouslyActivePromptObjects()
    {
        for (int i = 0; i < previouslyActiveObjects.Count; i++)
        {
            GameObject obj = previouslyActiveObjects[i];

            if (obj != null)
            {
                obj.SetActive(true);
            }
        }

        previouslyActiveObjects.Clear();
    }

    private void DisableSpecifiedComponents()
    {
        previouslyEnabledComponents.Clear();

        if (componentsToDisable == null || componentsToDisable.Count == 0)
            return;

        for (int i = 0; i < componentsToDisable.Count; i++)
        {
            Behaviour behaviour = componentsToDisable[i];

            if (behaviour == null)
                continue;

            if (behaviour.enabled)
            {
                previouslyEnabledComponents.Add(behaviour);
                behaviour.enabled = false;
            }
        }
    }

    private void RestorePreviouslyEnabledComponents()
    {
        for (int i = 0; i < previouslyEnabledComponents.Count; i++)
        {
            Behaviour behaviour = previouslyEnabledComponents[i];

            if (behaviour != null)
            {
                behaviour.enabled = true;
            }
        }

        previouslyEnabledComponents.Clear();
    }
}