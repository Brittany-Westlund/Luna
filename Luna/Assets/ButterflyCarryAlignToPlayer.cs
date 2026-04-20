using UnityEngine;

public class ButterflyCarryAlignToPlayer : MonoBehaviour
{
    [Header("Enable")]
    public bool alignToPlayerWhileCarryingNPC = true;

    [Header("References")]
    public Transform npcHoldPoint;
    public string npcHoldPointName = "NPCHoldPoint";

    [Header("Player Detection")]
    public string playerTag = "Player";
    public bool searchForPlayerOnStart = true;
    public bool keepTryingToFindPlayerIfMissing = true;
    public float playerSearchInterval = 1f;

    [Header("Carry Rules")]
    public bool onlyAlignWhenCarryingRideableNPC = true;

    [Header("Position Offsets")]
    public float xOffsetFromPlayer = 0f;
    public float yOffsetFromPlayer = 1f;

    [Header("Alignment Axes")]
    public bool alignX = true;
    public bool alignY = true;

    [Header("Movement")]
    public bool snapDirectlyToTarget = false;
    public float moveSpeed = 8f;
    public float maxStepPerFrame = 100f;

    protected Transform _playerTransform;
    protected float _nextPlayerSearchTime = -1f;

    protected virtual void Start()
    {
        if (npcHoldPoint == null)
        {
            npcHoldPoint = FindDeepChildByName(transform, npcHoldPointName);
        }

        if (searchForPlayerOnStart)
        {
            FindPlayerByTag();
        }
    }

    protected virtual void LateUpdate()
    {
        if (!alignToPlayerWhileCarryingNPC)
        {
            return;
        }

        if (npcHoldPoint == null)
        {
            npcHoldPoint = FindDeepChildByName(transform, npcHoldPointName);
            if (npcHoldPoint == null)
            {
                return;
            }
        }

        if (_playerTransform == null)
        {
            if (!keepTryingToFindPlayerIfMissing)
            {
                return;
            }

            if (Time.time >= _nextPlayerSearchTime)
            {
                FindPlayerByTag();
                _nextPlayerSearchTime = Time.time + playerSearchInterval;
            }

            if (_playerTransform == null)
            {
                return;
            }
        }

        if (onlyAlignWhenCarryingRideableNPC && !IsCarryingRideableNPC())
        {
            return;
        }

        AlignToPlayer();
    }

    protected virtual void AlignToPlayer()
    {
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = currentPosition;

        if (alignX)
        {
            targetPosition.x = _playerTransform.position.x + xOffsetFromPlayer;
        }

        if (alignY)
        {
            targetPosition.y = _playerTransform.position.y + yOffsetFromPlayer;
        }

        if (snapDirectlyToTarget)
        {
            transform.position = targetPosition;
            return;
        }

        transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            Mathf.Min(moveSpeed * Time.deltaTime, maxStepPerFrame)
        );
    }

    protected virtual bool IsCarryingRideableNPC()
    {
        if (npcHoldPoint == null)
        {
            return false;
        }

        ButterflyRideableNPC carriedNpc = npcHoldPoint.GetComponentInChildren<ButterflyRideableNPC>(true);
        return carriedNpc != null && carriedNpc.transform.parent == npcHoldPoint;
    }

    protected virtual void FindPlayerByTag()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObject != null)
        {
            _playerTransform = playerObject.transform;
        }
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