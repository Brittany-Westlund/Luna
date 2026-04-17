using UnityEngine;

public class MoveToTarget : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Movement")]
    public float speed = 2f;
    public float stopDistance = 0.01f;

    [Header("Completion Options")]
    public bool disableGameObjectOnArrival = false;
    public bool disableSpriteRendererOnArrival = false;

    [Header("Optional")]
    public SpriteRenderer spriteRendererToDisable;

    [Header("Debug")]
    public bool debugLogs = false;

    private bool isMoving = false;

    void Awake()
    {
        if (spriteRendererToDisable == null)
            spriteRendererToDisable = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!isMoving || target == null) return;

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = target.position;

        float distance = Vector3.Distance(currentPosition, targetPosition);

        if (distance <= stopDistance)
        {
            CompleteMovement();
            return;
        }

        transform.position = Vector3.MoveTowards(
            currentPosition,
            targetPosition,
            speed * Time.deltaTime
        );
    }

    /// <summary>
    /// Call this from UnityEvent
    /// </summary>
    public void StartMoving()
    {
        if (target == null)
        {
            Debug.LogWarning($"{name}: No target assigned.");
            return;
        }

        isMoving = true;

        if (debugLogs)
            Debug.Log($"{name}: Started moving toward {target.name}");
    }

    private void CompleteMovement()
    {
        isMoving = false;

        if (debugLogs)
            Debug.Log($"{name}: Reached target.");

        if (disableSpriteRendererOnArrival && spriteRendererToDisable != null)
        {
            spriteRendererToDisable.enabled = false;

            if (debugLogs)
                Debug.Log($"{name}: SpriteRenderer disabled.");
        }

        if (disableGameObjectOnArrival)
        {
            gameObject.SetActive(false);

            if (debugLogs)
                Debug.Log($"{name}: GameObject disabled.");
        }
    }
}