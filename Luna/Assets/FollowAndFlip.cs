using UnityEngine;
using System.Collections;

public class FollowAndFlip : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;
    public Transform butterfly;
    public SpriteRenderer butterflyRenderer;

    [Header("Input")]
    public KeyCode toggleFollowKey = KeyCode.B;
    public bool allowToggleInput = true;

    [Header("B Input (Tap vs Hold)")]
    public float holdThreshold = 0.2f;

    [Header("Summon")]
    public float summonSpeed = 6f;
    public float summonStopDistance = 0.15f;

    [Header("Follow Settings")]
    public float followSpeed = 5f;
    public float followDistance = 0.7f;
    public float horizontalOffset = 0.5f;
    public float offsetFlipSpeed = 3f;
    public bool startFollowing = false;

    [Header("Facing")]
    public float lunaMovementDeadZone = 0.001f;
    public float faceTowardLunaDeadZone = 0.02f;

    [Header("Hover")]
    public float groundHoverAmplitude = 0.08f;
    public float groundHoverFrequency = 3.5f;
    public float idleHoverAmplitudeMultiplier = 1f;

    [Header("Idle Height Offset")]
    public float idleHeightAboveLuna = 1.2f;

    private bool _isFollowing = false;
    private bool _isFacingRight = true;
    private bool _lastGroundTravelWasRight = true;
    private bool _isSummoning = false;

    private bool _bHeld = false;
    private bool _flutterTriggered = false;
    private float _bHoldTimer = 0f;

    private Transform _followTarget;
    private float _currentHorizontalOffset;
    private float _targetHorizontalOffset;
    private float _lastLunaX;
    private float _idleHoverBaseY;

    private Coroutine _lowerToLunaLevelCoroutine;
    private Coroutine _summonCoroutine;

    private ButterflyMistFlutter _flutter;

    void Start()
    {
        if (!luna)
            luna = GameObject.FindWithTag("Player");

        if (!butterfly)
            butterfly = transform;

        if (luna != null)
        {
            _followTarget = luna.transform;
            _lastLunaX = luna.transform.position.x;
        }

        _flutter = GetComponent<ButterflyMistFlutter>();

        _isFollowing = startFollowing;

        if (luna != null && butterfly != null)
        {
            _lastGroundTravelWasRight = luna.transform.position.x >= butterfly.position.x;
            _targetHorizontalOffset = _lastGroundTravelWasRight
                ? -Mathf.Abs(horizontalOffset)
                : Mathf.Abs(horizontalOffset);

            float butterflyToLuna = luna.transform.position.x - butterfly.position.x;
            if (butterflyToLuna > faceTowardLunaDeadZone)
                _isFacingRight = true;
            else if (butterflyToLuna < -faceTowardLunaDeadZone)
                _isFacingRight = false;
        }
        else
        {
            _isFacingRight = true;
            _targetHorizontalOffset = -Mathf.Abs(horizontalOffset);
        }

        _currentHorizontalOffset = _targetHorizontalOffset;

        if (butterfly != null)
            _idleHoverBaseY = butterfly.position.y;

        ApplyVisualFacing();
    }

    void Update()
    {
        HandleInput();

        UpdateGroundFacingAndOffset();

        if (_isSummoning)
            return;

        if (_isFollowing)
        {
            FollowLunaGround();
        }
        else if (_lowerToLunaLevelCoroutine == null)
        {
            ApplyIdleGroundHover();
        }
    }

    void HandleInput()
    {
        if (!allowToggleInput)
            return;

        if (Input.GetKeyDown(toggleFollowKey))
        {
            _bHeld = true;
            _bHoldTimer = 0f;
            _flutterTriggered = false;
        }

        if (_bHeld && Input.GetKey(toggleFollowKey))
        {
            _bHoldTimer += Time.deltaTime;

            if (!_flutterTriggered && _bHoldTimer >= holdThreshold)
            {
                _flutterTriggered = true;

                if (_flutter != null)
                    _flutter.StartFlutter();
            }
        }

        if (_bHeld && Input.GetKeyUp(toggleFollowKey))
        {
            if (_flutterTriggered)
            {
                if (_flutter != null)
                    _flutter.StopFlutter();
            }
            else
            {
                ToggleFollowing();
            }

            _bHeld = false;
            _bHoldTimer = 0f;
            _flutterTriggered = false;
        }
    }

    public void ToggleFollowing()
    {
        SetFollowing(!_isFollowing);
    }

    public void SetFollowing(bool shouldFollow)
    {
        if (_isFollowing == shouldFollow && !_isSummoning)
            return;

        if (_lowerToLunaLevelCoroutine != null)
        {
            StopCoroutine(_lowerToLunaLevelCoroutine);
            _lowerToLunaLevelCoroutine = null;
        }

        if (_summonCoroutine != null)
        {
            StopCoroutine(_summonCoroutine);
            _summonCoroutine = null;
        }

        if (shouldFollow)
        {
            _isFollowing = false;
            _isSummoning = true;
            _summonCoroutine = StartCoroutine(SummonToLunaThenFollow());
        }
        else
        {
            _isSummoning = false;
            _isFollowing = false;
            _lowerToLunaLevelCoroutine = StartCoroutine(LowerToLunaLevelMerged());
        }
    }

    public bool IsFollowing()
    {
        return _isFollowing;
    }

    public bool IsSummoning()
    {
        return _isSummoning;
    }

    public void ForceFaceTowardLuna()
    {
        UpdateGroundFacingAndOffset();
    }

    IEnumerator SummonToLunaThenFollow()
    {
        if (_followTarget == null || butterfly == null)
        {
            _isSummoning = false;
            _summonCoroutine = null;
            yield break;
        }

        while (_followTarget != null && butterfly != null)
        {
            Vector3 target = _followTarget.position;
            target.z = butterfly.position.z;

            butterfly.position = Vector3.MoveTowards(
                butterfly.position,
                target,
                summonSpeed * Time.deltaTime
            );

            float lunaRelativeX = _followTarget.position.x - butterfly.position.x;
            if (lunaRelativeX > faceTowardLunaDeadZone)
                _isFacingRight = true;
            else if (lunaRelativeX < -faceTowardLunaDeadZone)
                _isFacingRight = false;

            ApplyVisualFacing();

            if (Vector2.Distance(butterfly.position, _followTarget.position) <= summonStopDistance)
                break;

            yield return null;
        }

        _isSummoning = false;
        _isFollowing = true;
        _idleHoverBaseY = butterfly.position.y;
        _summonCoroutine = null;
    }

    void UpdateGroundFacingAndOffset()
    {
        if (luna == null || butterfly == null)
            return;

        float lunaDX = luna.transform.position.x - _lastLunaX;

        if (Mathf.Abs(lunaDX) > lunaMovementDeadZone)
            _lastGroundTravelWasRight = lunaDX > 0f;

        _lastLunaX = luna.transform.position.x;

        _targetHorizontalOffset = _lastGroundTravelWasRight
            ? -Mathf.Abs(horizontalOffset)
            : Mathf.Abs(horizontalOffset);

        _currentHorizontalOffset = Mathf.Lerp(
            _currentHorizontalOffset,
            _targetHorizontalOffset,
            offsetFlipSpeed * Time.deltaTime
        );

        float lunaRelativeX = luna.transform.position.x - butterfly.position.x;

        if (lunaRelativeX > faceTowardLunaDeadZone)
            _isFacingRight = true;
        else if (lunaRelativeX < -faceTowardLunaDeadZone)
            _isFacingRight = false;

        ApplyVisualFacing();
    }

    void FollowLunaGround()
    {
        if (_followTarget == null || butterfly == null)
            return;

        float hoverY = GetGroundHoverOffsetY(groundHoverAmplitude);

        Vector3 goal = _followTarget.position + new Vector3(
            _currentHorizontalOffset,
            followDistance + hoverY,
            0f
        );

        butterfly.position = Vector3.Lerp(
            butterfly.position,
            goal,
            followSpeed * Time.deltaTime
        );
    }

    void ApplyIdleGroundHover()
    {
        if (butterfly == null || _followTarget == null)
            return;

        float hoverY = GetGroundHoverOffsetY(groundHoverAmplitude * idleHoverAmplitudeMultiplier);

        Vector3 pos = butterfly.position;
        pos.y = _followTarget.position.y + idleHeightAboveLuna + hoverY;
        butterfly.position = pos;
    }

    float GetGroundHoverOffsetY(float amplitude)
    {
        return Mathf.Sin(Time.time * groundHoverFrequency) * amplitude;
    }

    IEnumerator LowerToLunaLevelMerged()
    {
        if (_followTarget == null || butterfly == null)
        {
            _lowerToLunaLevelCoroutine = null;
            yield break;
        }

        Vector3 goal = new Vector3(
            butterfly.position.x,
            _followTarget.position.y + idleHeightAboveLuna,
            butterfly.position.z
        );

        while (Mathf.Abs(butterfly.position.y - goal.y) > 0.01f)
        {
            butterfly.position = Vector3.Lerp(
                butterfly.position,
                goal,
                followSpeed * Time.deltaTime
            );
            yield return null;
        }

        butterfly.position = goal;
        _idleHoverBaseY = butterfly.position.y;
        _lowerToLunaLevelCoroutine = null;
    }

    void ApplyVisualFacing()
    {
        if (butterflyRenderer != null)
            butterflyRenderer.flipX = _isFacingRight;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_followTarget != null && col.transform == _followTarget)
            UpdateGroundFacingAndOffset();
    }
}