using UnityEngine;
using UnityEngine.Events;

public class ConversationEndEvent : MonoBehaviour
{
    [Header("Events")]
    public UnityEvent onConversationEnd;

    // Called automatically by Dialogue System
    public void OnConversationEnd(Transform actor)
    {
        onConversationEnd?.Invoke();
    }
}