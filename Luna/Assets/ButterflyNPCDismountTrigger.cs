using UnityEngine;
using UnityEngine.Events;

public class ButterflyNPCDismountTrigger : MonoBehaviour
{
    [Header("Target NPC")]
    public ButterflyRideableNPC targetNPC;

    [Header("Drop Point")]
    public Transform dropPoint;
    public Vector3 worldOffset = Vector3.zero;
    public Transform dropParent;

    [Header("Butterfly Detection")]
    public string butterflyTag = "Player";
    public bool requireTag = true;

    [Header("Events")]
    public UnityEvent onNPCDismountedHere;

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (targetNPC == null)
        {
            Debug.LogWarning("ButterflyNPCDismountTrigger on " + gameObject.name + " has no targetNPC assigned.");
            return;
        }

        if (!targetNPC.IsMounted())
        {
            return;
        }

        Transform butterflyRoot = GetButterflyRoot(other);
        if (butterflyRoot == null)
        {
            return;
        }

        Vector3 targetPosition = GetDropPosition();
        targetNPC.DismountTo(targetPosition, dropParent);

        onNPCDismountedHere.Invoke();
    }

    protected virtual Vector3 GetDropPosition()
    {
        if (dropPoint != null)
        {
            return dropPoint.position + worldOffset;
        }

        return transform.position + worldOffset;
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
}