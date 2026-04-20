using UnityEngine;

public class ButterflyNPCMountTrigger : MonoBehaviour
{
    [Header("Target NPC")]
    public ButterflyRideableNPC targetNPC;

    [Header("Butterfly Detection")]
    public string butterflyTag = "Player";
    public string holdPointName = "NPCHoldPoint";
    public bool requireTag = true;

    [Header("Rules")]
    public bool mountOnlyIfNpcNotAlreadyMounted = true;

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (targetNPC == null)
        {
            Debug.LogWarning("ButterflyNPCMountTrigger on " + gameObject.name + " has no targetNPC assigned.");
            return;
        }

        if (mountOnlyIfNpcNotAlreadyMounted && targetNPC.IsMounted())
        {
            return;
        }

        Transform butterflyRoot = GetButterflyRoot(other);
        if (butterflyRoot == null)
        {
            return;
        }

        Transform holdPoint = FindDeepChildByName(butterflyRoot, holdPointName);
        if (holdPoint == null)
        {
            Debug.LogWarning("ButterflyNPCMountTrigger: Could not find hold point '" + holdPointName + "' under butterfly root '" + butterflyRoot.name + "'.");
            return;
        }

        targetNPC.MountTo(holdPoint);
    }

    protected virtual Transform GetButterflyRoot(Collider2D other)
    {
        Transform root = other.transform.root;

        if (!requireTag)
        {
            return root;
        }

        if (root.CompareTag(butterflyTag))
        {
            return root;
        }

        if (other.CompareTag(butterflyTag))
        {
            return other.transform;
        }

        return null;
    }

    protected virtual Transform FindDeepChildByName(Transform parent, string childName)
    {
        if (parent == null)
        {
            return null;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);

            if (child.name == childName)
            {
                return child;
            }

            Transform found = FindDeepChildByName(child, childName);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}