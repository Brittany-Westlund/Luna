using UnityEngine;

// Attach to an NPC or environmental object. Call Unlock() from a dialogue,
// UnityEvent, or gameplay script; optionally unlock when the player enters a trigger.
public class BookPageUnlocker : MonoBehaviour
{
    [Header("Page To Unlock")]
    [SerializeField] private string pageId = "";

    [Header("Optional Automatic Trigger")]
    [SerializeField] private bool unlockOnPlayerTrigger = false;
    [SerializeField] private string playerTag = "Player";

    public void Unlock()
    {
        FlorilegiumProgress.Unlock(pageId);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!unlockOnPlayerTrigger || other == null) return;
        if (other.CompareTag(playerTag) || other.transform.root.CompareTag(playerTag))
            Unlock();
    }
}
