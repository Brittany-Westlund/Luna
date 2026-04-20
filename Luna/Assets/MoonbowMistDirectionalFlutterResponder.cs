using UnityEngine;

public class MoonbowMistDirectionalFlutterResponder : MonoBehaviour
{
    public enum MoveDirectionMode
    {
        RightOnly,
        LeftOnly,
        Both
    }

    [Header("References")]
    public ButterflyMistFlutter butterflyMistFlutter;
    public Transform butterflyTransform;
    public SpriteRenderer butterflyRenderer;

    [Header("Initial Position")]
    public bool captureInitialPositionOnStart = true;

    [Header("Activation")]
    public float activationRadius = 6f;

    [Header("Directional Movement")]
    public MoveDirectionMode moveDirectionMode = MoveDirectionMode.RightOnly;
    public float moveSpeed = 2f;
    public float maxDistanceFromInitial = 2f;

    [Header("Axis Control")]
    public bool affectX = true;
    public bool affectY = false;
    public float yMoveRatio = 0f;

    [Header("Decay")]
    public bool decayBackToInitial = true;
    public float returnDelay = 1f;
    public float returnSpeed = 1.2f;

    [Header("Debug")]
    public bool debugLogs = false;

    private Vector3 _initialPosition;
    private bool _returnTimerRunning = false;
    private float _returnTimer = 0f;

    void Awake()
    {
        if (captureInitialPositionOnStart)
            _initialPosition = transform.position;

        if (butterflyMistFlutter == null)
            butterflyMistFlutter = FindObjectOfType<ButterflyMistFlutter>();

        if (butterflyTransform == null && butterflyMistFlutter != null)
            butterflyTransform = butterflyMistFlutter.transform;

        if (butterflyRenderer == null && butterflyTransform != null)
            butterflyRenderer = butterflyTransform.GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (butterflyMistFlutter == null || butterflyTransform == null)
            return;

        bool butterflyIsNearby = Vector2.Distance(transform.position, butterflyTransform.position) <= activationRadius;
        bool fluttering = butterflyMistFlutter.IsFluttering();

        if (fluttering && butterflyIsNearby)
        {
            _returnTimerRunning = false;
            _returnTimer = 0f;
            MoveDirectionally();
        }
        else if (decayBackToInitial)
        {
            if (!_returnTimerRunning)
            {
                _returnTimerRunning = true;
                _returnTimer = 0f;
            }

            _returnTimer += Time.deltaTime;

            if (_returnTimer >= returnDelay)
            {
                ReturnToInitial();
            }
        }
    }

    void MoveDirectionally()
    {
        Vector3 current = transform.position;
        Vector3 next = current;

        float directionSign = GetAllowedDirectionSign();
        if (Mathf.Approximately(directionSign, 0f))
            return;

        if (affectX)
            next.x += directionSign * moveSpeed * Time.deltaTime;

        if (affectY)
            next.y += directionSign * yMoveRatio * moveSpeed * Time.deltaTime;

        Vector3 offsetFromInitial = next - _initialPosition;

        if (affectX)
        {
            float clampedXOffset = Mathf.Clamp(offsetFromInitial.x, -maxDistanceFromInitial, maxDistanceFromInitial);

            switch (moveDirectionMode)
            {
                case MoveDirectionMode.RightOnly:
                    clampedXOffset = Mathf.Clamp(clampedXOffset, 0f, maxDistanceFromInitial);
                    break;

                case MoveDirectionMode.LeftOnly:
                    clampedXOffset = Mathf.Clamp(clampedXOffset, -maxDistanceFromInitial, 0f);
                    break;

                case MoveDirectionMode.Both:
                    break;
            }

            next.x = _initialPosition.x + clampedXOffset;
        }

        if (affectY)
        {
            float maxYDistance = Mathf.Abs(maxDistanceFromInitial * yMoveRatio);
            float yOffset = next.y - _initialPosition.y;
            yOffset = Mathf.Clamp(yOffset, -maxYDistance, maxYDistance);
            next.y = _initialPosition.y + yOffset;
        }

        transform.position = next;

        if (debugLogs)
            Debug.Log($"{name}: moved to {transform.position}");
    }

    float GetAllowedDirectionSign()
    {
        bool facingRight = true;

        if (butterflyRenderer != null)
            facingRight = butterflyRenderer.flipX;

        switch (moveDirectionMode)
        {
            case MoveDirectionMode.RightOnly:
                return 1f;

            case MoveDirectionMode.LeftOnly:
                return -1f;

            case MoveDirectionMode.Both:
                return facingRight ? 1f : -1f;
        }

        return 0f;
    }

    void ReturnToInitial()
    {
        Vector3 current = transform.position;
        Vector3 target = current;

        if (affectX)
            target.x = _initialPosition.x;

        if (affectY)
            target.y = _initialPosition.y;

        transform.position = Vector3.MoveTowards(current, target, returnSpeed * Time.deltaTime);
    }

    public void CaptureCurrentAsInitialPosition()
    {
        _initialPosition = transform.position;
    }

    public Vector3 GetInitialPosition()
    {
        return _initialPosition;
    }

    public void SnapToInitialPosition()
    {
        transform.position = _initialPosition;
        _returnTimerRunning = false;
        _returnTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRadius);

        Gizmos.color = Color.cyan;
        Vector3 left = Application.isPlaying ? _initialPosition : transform.position;
        Vector3 right = left;

        if (affectX)
        {
            left.x -= maxDistanceFromInitial;
            right.x += maxDistanceFromInitial;
        }

        Gizmos.DrawLine(left, right);
        Gizmos.DrawSphere(left, 0.06f);
        Gizmos.DrawSphere(right, 0.06f);
    }
}