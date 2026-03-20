using System.Collections;
using UnityEngine;
using PixelCrushers.DialogueSystem;

public class StopDialogueAutoDisable : MonoBehaviour
{
    private bool subscribed = false;

    void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    IEnumerator SubscribeWhenReady()
    {
        while (!DialogueManager.HasInstance)
        {
            yield return null;
        }

        DialogueManager.Instance.conversationEnded -= OnConversationEnded;
        DialogueManager.Instance.conversationEnded += OnConversationEnded;
        subscribed = true;
    }

    void OnDisable()
    {
        if (subscribed && DialogueManager.HasInstance)
        {
            DialogueManager.Instance.conversationEnded -= OnConversationEnded;
            subscribed = false;
        }
    }

    void OnConversationEnded(Transform actor)
    {
        GameObject luna = GameObject.FindWithTag("Player");
        if (luna != null && !luna.activeSelf)
        {
            Debug.Log("DialogueSystem tried to disable Luna—re-enabling.");
            luna.SetActive(true);
        }
    }
}