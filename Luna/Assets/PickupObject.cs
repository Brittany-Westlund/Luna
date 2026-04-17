using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Collider2D))]
public class PickupObject : MonoBehaviour
{
    [Header("Player / Hold Target")]
    public Transform player;

    [Tooltip("Optional manual override. If empty, auto-finds AcornHoldPoint.")]
    public Transform holdPoint;

    public string autoHoldPointName = "AcornHoldPoint";
    public string playerTag = "Player";

    [Header("Pickup Position")]
    public Vector3 pickUpOffset = Vector3.zero;

    [Header("Pickup Scale")]
    public bool usePickupScaleOverride = true;
    public Vector3 pickupScaleOverride = Vector3.one;

    [Header("Fallback Scale Options")]
    public bool resetLocalScaleOnPickup = false;
    public Vector3 heldLocalScale = Vector3.one;

    [Header("Pickup Delay")]
    [Tooltip("How long after this object becomes active before it can be picked up.")]
    public float pickupDelay = 0.5f;

    [Header("Optional Swap Renderer On SetDown")]
    public SpriteRenderer objectToActivateRenderer;

    [Header("Optional SetDownObject Hook")]
    public SetDownObject setDownObject;

    [Header("Behavior")]
    public bool preventRepickWhileHeld = true;
    public bool allowTriggerStayFallback = true;

    [Header("Debug")]
    public bool debugLogs = true;

    private Vector3 originalScale;
    private SpriteRenderer spriteRenderer;
    private Collider2D ownCollider;
    private bool isPickedUp = false;
    private bool canBePickedUp = false;
    private Coroutine pickupDelayRoutine;

    private void Awake()
    {
        originalScale = transform.localScale;
        spriteRenderer = GetComponent<SpriteRenderer>();
        ownCollider = GetComponent<Collider2D>();

        if (ownCollider != null)
            ownCollider.isTrigger = true;

        ResolvePlayerAndSetDownObject();
        ResolveHoldPoint();
    }

    private void Start()
    {
        ResolvePlayerAndSetDownObject();
        ResolveHoldPoint();

        if (debugLogs)
        {
            Debug.Log(
                $"[PickupObject] START on {name} | " +
                $"player={(player != null ? player.name : "NULL")} | " +
                $"holdPoint={(GetResolvedHoldPoint() != null ? GetResolvedHoldPoint().name : "NULL")}"
            );
        }
    }

    private void OnEnable()
    {
        if (pickupDelayRoutine != null)
            StopCoroutine(pickupDelayRoutine);

        if (pickupDelay > 0f)
            pickupDelayRoutine = StartCoroutine(EnablePickupAfterDelay());
        else
            canBePickedUp = true;
    }

    private void OnDisable()
    {
        if (pickupDelayRoutine != null)
        {
            StopCoroutine(pickupDelayRoutine);
            pickupDelayRoutine = null;
        }

        canBePickedUp = false;
    }

    private IEnumerator EnablePickupAfterDelay()
    {
        canBePickedUp = false;
        yield return new WaitForSeconds(pickupDelay);
        canBePickedUp = true;
        pickupDelayRoutine = null;

        if (debugLogs)
            Debug.Log($"[PickupObject] {name}: Pickup enabled after {pickupDelay} seconds.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryPickupFromCollider(other, "Enter");
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!allowTriggerStayFallback)
            return;

        TryPickupFromCollider(other, "Stay");
    }

    private void ResolvePlayerAndSetDownObject()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag(playerTag);
            if (found != null)
                player = found.transform;
        }

        if (setDownObject == null && player != null)
        {
            setDownObject = player.GetComponent<SetDownObject>();
            if (setDownObject == null)
                setDownObject = player.GetComponentInChildren<SetDownObject>(true);
        }
    }

    private void ResolveHoldPoint()
    {
        if (holdPoint != null || player == null)
            return;

        holdPoint = FindChildRecursive(player, autoHoldPointName);

        if (debugLogs)
        {
            if (holdPoint != null)
                Debug.Log($"[PickupObject] Found hold point: {holdPoint.name}");
            else
                Debug.LogWarning($"[PickupObject] Could not find '{autoHoldPointName}' under player.");
        }
    }

    private Transform GetResolvedHoldPoint()
    {
        return holdPoint != null ? holdPoint : player;
    }

    private void TryPickupFromCollider(Collider2D other, string source)
    {
        if (!canBePickedUp)
            return;

        if (preventRepickWhileHeld && isPickedUp)
            return;

        if (other == null)
            return;

        ResolvePlayerAndSetDownObject();
        ResolveHoldPoint();

        Transform matchedPlayer = GetMatchingPlayerRoot(other);
        if (matchedPlayer == null)
            return;

        if (player == null)
            player = matchedPlayer;

        Transform parentTarget = GetResolvedHoldPoint();
        if (parentTarget == null)
            return;

        transform.SetParent(parentTarget, false);
        transform.localPosition = pickUpOffset;
        transform.localRotation = Quaternion.identity;

        if (usePickupScaleOverride)
        {
            transform.localScale = pickupScaleOverride;
        }
        else if (resetLocalScaleOnPickup)
        {
            transform.localScale = heldLocalScale;
        }
        else
        {
            transform.localScale = originalScale;
        }

        isPickedUp = true;
        canBePickedUp = false;

        if (setDownObject != null)
            setDownObject.PickUpObject(this);

        if (debugLogs)
        {
            Debug.Log(
                $"[PickupObject] PICKED UP via {source} | Parent={parentTarget.name} | Scale={transform.localScale}"
            );
        }
    }

    private Transform GetMatchingPlayerRoot(Collider2D other)
    {
        if (other == null)
            return null;

        if (player != null && other.transform.IsChildOf(player))
            return player;

        if (other.CompareTag(playerTag))
            return other.transform;

        if (other.transform.root != null && other.transform.root.CompareTag(playerTag))
            return other.transform.root;

        return null;
    }

    private Transform FindChildRecursive(Transform parent, string nameToFind)
    {
        foreach (Transform child in parent)
        {
            if (child.name == nameToFind)
                return child;

            Transform found = FindChildRecursive(child, nameToFind);
            if (found != null)
                return found;
        }

        return null;
    }

    public void SetDown()
    {
        transform.SetParent(null);
        transform.localScale = originalScale;
        isPickedUp = false;

        if (spriteRenderer != null)
            spriteRenderer.enabled = false;

        if (objectToActivateRenderer != null)
            objectToActivateRenderer.enabled = true;

        if (debugLogs)
            Debug.Log($"[PickupObject] SetDown called.");
    }

    public void ForcePickup()
    {
        ResolvePlayerAndSetDownObject();
        ResolveHoldPoint();

        Transform parentTarget = GetResolvedHoldPoint();
        if (parentTarget == null)
            return;

        transform.SetParent(parentTarget, false);
        transform.localPosition = pickUpOffset;
        transform.localRotation = Quaternion.identity;

        if (usePickupScaleOverride)
            transform.localScale = pickupScaleOverride;
        else
            transform.localScale = originalScale;

        isPickedUp = true;
        canBePickedUp = false;

        if (setDownObject != null)
            setDownObject.PickUpObject(this);

        if (debugLogs)
            Debug.Log($"[PickupObject] ForcePickup succeeded.");
    }
}