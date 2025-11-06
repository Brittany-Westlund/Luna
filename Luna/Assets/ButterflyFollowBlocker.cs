using UnityEngine;

public class ButterflyFollowBlocker : MonoBehaviour
{
    [Header("Auto-detects ButterflyNPC by name or tag 'Butterfly'")]
    public FollowAndFlip butterflyFollow;

    [Tooltip("If true, disables the butterfly's FollowAndFlip script input (B key).")]
    public bool blockFollowInput = true;

    void Awake()
    {
        // Try to find by name first
        GameObject butterflyObj = GameObject.Find("ButterflyNPC");

        // If not found, try by tag
        if (butterflyObj == null)
        {
            GameObject taggedObj = GameObject.FindGameObjectWithTag("Butterfly");
            if (taggedObj != null) butterflyObj = taggedObj;
        }

        // Assign the FollowAndFlip component if found
        if (butterflyObj != null)
        {
            butterflyFollow = butterflyObj.GetComponent<FollowAndFlip>();
        }
        else
        {
            Debug.LogWarning("🦋 ButterflyFollowBlocker: No ButterflyNPC found by name or tag!");
        }
    }

    void Update()
    {
        if (butterflyFollow == null) return;

        // Toggle the butterfly's ability to follow via B key
        butterflyFollow.enabled = !blockFollowInput;
    }

    // Optional helper for Dialogue System or cutscenes
    public void SetBlocked(bool blocked)
    {
        blockFollowInput = blocked;
    }
}
