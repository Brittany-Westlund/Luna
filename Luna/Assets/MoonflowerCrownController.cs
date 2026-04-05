using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;

[DisallowMultipleComponent]
public class MoonflowerCrownController : MonoBehaviour
{
    [Header("References")]
    public Transform crownVisualRoot;
    public SpriteRenderer lunaRenderer;
    public LunaRest lunaRest;
    public Character character;
    public CharacterHorizontalMovement characterHorizontalMovement;

    [Header("Facing - Normal")]
    public bool autoMirrorXPosition = false;
    public bool autoMirrorXScale = false;
    public bool autoMirrorZRotation = true;

    public Vector3 rightFacingLocalPosition = new Vector3(0.25f, 0f, 0f);
    public Vector3 leftFacingLocalPosition = new Vector3(-0.25f, 0f, 0f);

    public Vector3 rightFacingLocalScale = Vector3.one;
    public Vector3 leftFacingLocalScale = new Vector3(-1f, 1f, 1f);

    public float rightFacingZRotation = 0f;
    public float leftFacingZRotation = 0f;

    [Header("Ledge Hanging Override")]
    public bool useLedgeHangOverrides = true;

    public Vector3 ledgeHangRightFacingLocalPosition = new Vector3(0.25f, 0f, 0f);
    public Vector3 ledgeHangLeftFacingLocalPosition = new Vector3(-0.25f, 0f, 0f);

    public float ledgeHangRightFacingZRotation = 0f;
    public float ledgeHangLeftFacingZRotation = 0f;

    [Header("Visibility")]
    public bool hideWhileResting = true;
    public bool hideWhileClimbing = true;
    public bool hideWhileLedgeClimbing = true;

    [Header("Manual Toggle")]
    public KeyCode toggleKey = KeyCode.C;
    public bool startEnabled = true;

    [Header("Slumberdust Protection")]
    [Range(0f, 1f)]
    public float slumberdustReductionPercent = 0.50f;

    [Header("Walking Bob")]
    public bool enableWalkingBob = true;
    public float bobAmplitude = 0.03f;
    public float bobSpeed = 8f;
    public float bobReturnSpeed = 8f;
    public float bobMovementThreshold = 0.01f;
    public bool bobOnlyWhenGrounded = true;
    public bool scaleBobByMovementSpeed = true;

    [Header("Equip Feedback")]
    public AudioSource equipAudioSource;
    public AudioClip equipSFX;
    public bool playEquipSFXOnEnable = true;

    public Transform sparklesRoot;
    public bool pulseSparklesOnEnable = true;
    public float sparklePulseScaleMultiplier = 1.35f;
    public float sparklePulseDuration = 0.35f;
    public float sparkleReturnDuration = 0.35f;
    public AnimationCurve sparklePulseCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);
    public AnimationCurve sparkleReturnCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    [Header("Debug")]
    public bool debugLogs = false;

    private SpriteRenderer[] _crownSprites;
    private bool _lastVisibleState = true;
    private bool _initializedMirroring = false;
    private bool _isEquipped = true;

    private float _bobTimer = 0f;
    private float _currentBobOffsetY = 0f;

    private Transform[] _sparkleChildren;
    private Vector3[] _sparkleInitialScales;
    private Coroutine _sparklePulseCoroutine;

    public float CurrentSlumberdustReductionPercent
    {
        get
        {
            if (!_isEquipped)
            {
                return 0f;
            }

            return Mathf.Clamp01(slumberdustReductionPercent);
        }
    }

    private void Awake()
    {
        if (crownVisualRoot == null)
        {
            crownVisualRoot = transform;
        }

        if (lunaRenderer == null)
        {
            lunaRenderer = GetComponentInParent<SpriteRenderer>(true);
        }

        if (lunaRest == null)
        {
            lunaRest = GetComponentInParent<LunaRest>(true);
        }

        if (character == null)
        {
            character = GetComponentInParent<Character>(true);
        }

        if (characterHorizontalMovement == null)
        {
            characterHorizontalMovement = GetComponentInParent<CharacterHorizontalMovement>(true);
        }

        _crownSprites = crownVisualRoot.GetComponentsInChildren<SpriteRenderer>(true);
        _isEquipped = startEnabled;

        InitializeMirroredValues();
        CacheSparkleChildren();

        _lastVisibleState = !_isEquipped;
    }

    private void Update()
    {
        HandleToggleInput();
    }

    private void LateUpdate()
    {
        UpdateFacingAndPlacement();
        UpdateVisibility();
    }

    private void InitializeMirroredValues()
    {
        if (_initializedMirroring)
        {
            return;
        }

        if (autoMirrorXPosition)
        {
            rightFacingLocalPosition = new Vector3(
                Mathf.Abs(rightFacingLocalPosition.x),
                rightFacingLocalPosition.y,
                rightFacingLocalPosition.z
            );

            leftFacingLocalPosition = new Vector3(
                -Mathf.Abs(rightFacingLocalPosition.x),
                rightFacingLocalPosition.y,
                rightFacingLocalPosition.z
            );

            ledgeHangRightFacingLocalPosition = new Vector3(
                Mathf.Abs(ledgeHangRightFacingLocalPosition.x),
                ledgeHangRightFacingLocalPosition.y,
                ledgeHangRightFacingLocalPosition.z
            );

            ledgeHangLeftFacingLocalPosition = new Vector3(
                -Mathf.Abs(ledgeHangRightFacingLocalPosition.x),
                ledgeHangRightFacingLocalPosition.y,
                ledgeHangRightFacingLocalPosition.z
            );
        }

        if (autoMirrorZRotation)
        {
            leftFacingZRotation = -rightFacingZRotation;
            ledgeHangLeftFacingZRotation = -ledgeHangRightFacingZRotation;
        }

        _initializedMirroring = true;
    }

    private void CacheSparkleChildren()
    {
        if (sparklesRoot == null)
        {
            _sparkleChildren = null;
            _sparkleInitialScales = null;
            return;
        }

        int childCount = sparklesRoot.childCount;
        _sparkleChildren = new Transform[childCount];
        _sparkleInitialScales = new Vector3[childCount];

        for (int i = 0; i < childCount; i++)
        {
            Transform child = sparklesRoot.GetChild(i);
            _sparkleChildren[i] = child;
            _sparkleInitialScales[i] = child.localScale;
        }
    }

    private void HandleToggleInput()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            SetCrownEquipped(!_isEquipped);
        }
    }

    public void EnableCrown()
    {
        SetCrownEquipped(true);
    }

    public void DisableCrown()
    {
        SetCrownEquipped(false);
    }

    public void SetCrownEquipped(bool equipped)
    {
        bool wasEquipped = _isEquipped;
        _isEquipped = equipped;

        if (debugLogs)
        {
            Debug.Log("Moonflower Crown Set Equipped: " + equipped);
        }

        _lastVisibleState = !equipped;

        if (!wasEquipped && equipped)
        {
            PlayEquipFeedback();
        }

        if (!equipped)
        {
            ResetSparkleScalesImmediate();
        }
    }

    private void PlayEquipFeedback()
    {
        if (playEquipSFXOnEnable && equipAudioSource != null && equipSFX != null)
        {
            equipAudioSource.PlayOneShot(equipSFX);
        }

        if (pulseSparklesOnEnable)
        {
            if (_sparklePulseCoroutine != null)
            {
                StopCoroutine(_sparklePulseCoroutine);
            }

            _sparklePulseCoroutine = StartCoroutine(PulseSparklesCoroutine());
        }
    }

    private IEnumerator PulseSparklesCoroutine()
    {
        if (_sparkleChildren == null || _sparkleInitialScales == null || _sparkleChildren.Length == 0)
        {
            yield break;
        }

        float clampedScaleMultiplier = Mathf.Max(1f, sparklePulseScaleMultiplier);
        float upDuration = Mathf.Max(0.0001f, sparklePulseDuration);
        float downDuration = Mathf.Max(0.0001f, sparkleReturnDuration);

        float elapsed = 0f;

        while (elapsed < upDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / upDuration);
            float curveValue = sparklePulseCurve != null ? sparklePulseCurve.Evaluate(t) : t;

            for (int i = 0; i < _sparkleChildren.Length; i++)
            {
                if (_sparkleChildren[i] != null)
                {
                    _sparkleChildren[i].localScale = Vector3.Lerp(
                        _sparkleInitialScales[i],
                        _sparkleInitialScales[i] * clampedScaleMultiplier,
                        curveValue
                    );
                }
            }

            yield return null;
        }

        elapsed = 0f;

        while (elapsed < downDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / downDuration);
            float curveValue = sparkleReturnCurve != null ? sparkleReturnCurve.Evaluate(t) : t;

            for (int i = 0; i < _sparkleChildren.Length; i++)
            {
                if (_sparkleChildren[i] != null)
                {
                    _sparkleChildren[i].localScale = Vector3.Lerp(
                        _sparkleInitialScales[i] * clampedScaleMultiplier,
                        _sparkleInitialScales[i],
                        curveValue
                    );
                }
            }

            yield return null;
        }

        ResetSparkleScalesImmediate();
        _sparklePulseCoroutine = null;
    }

    private void ResetSparkleScalesImmediate()
    {
        if (_sparkleChildren == null || _sparkleInitialScales == null)
        {
            return;
        }

        for (int i = 0; i < _sparkleChildren.Length; i++)
        {
            if (_sparkleChildren[i] != null)
            {
                _sparkleChildren[i].localScale = _sparkleInitialScales[i];
            }
        }
    }

    private void UpdateFacingAndPlacement()
    {
        if (crownVisualRoot == null)
        {
            return;
        }

        bool facingLeft = IsFacingLeft();
        bool isLedgeHanging = IsLedgeHanging();

        Vector3 targetPosition;
        float targetZRotation;

        if (useLedgeHangOverrides && isLedgeHanging)
        {
            targetPosition = facingLeft
                ? ledgeHangLeftFacingLocalPosition
                : ledgeHangRightFacingLocalPosition;

            targetZRotation = facingLeft
                ? ledgeHangLeftFacingZRotation
                : ledgeHangRightFacingZRotation;
        }
        else
        {
            targetPosition = facingLeft
                ? leftFacingLocalPosition
                : rightFacingLocalPosition;

            targetZRotation = facingLeft
                ? leftFacingZRotation
                : rightFacingZRotation;

            float bobOffset = ComputeBobOffset();
            targetPosition.y += bobOffset;
        }

        crownVisualRoot.localPosition = targetPosition;

        if (autoMirrorXScale)
        {
            crownVisualRoot.localScale = facingLeft
                ? leftFacingLocalScale
                : rightFacingLocalScale;
        }

        crownVisualRoot.localEulerAngles = new Vector3(0f, 0f, targetZRotation);
    }

    private float ComputeBobOffset()
    {
        if (!enableWalkingBob || !ShouldBob())
        {
            _bobTimer = 0f;
            _currentBobOffsetY = Mathf.Lerp(
                _currentBobOffsetY,
                0f,
                Time.deltaTime * bobReturnSpeed
            );

            return _currentBobOffsetY;
        }

        _bobTimer += Time.deltaTime * bobSpeed;

        float bobStrength = 1f;

        if (scaleBobByMovementSpeed && characterHorizontalMovement != null)
        {
            float movementAmount = Mathf.Abs(characterHorizontalMovement.HorizontalMovementForce);
            float walkSpeed = Mathf.Max(0.0001f, characterHorizontalMovement.WalkSpeed);
            bobStrength = Mathf.Clamp01(movementAmount / walkSpeed);
        }

        _currentBobOffsetY = Mathf.Sin(_bobTimer) * bobAmplitude * bobStrength;
        return _currentBobOffsetY;
    }

    private bool ShouldBob()
    {
        if (!_isEquipped)
        {
            return false;
        }

        if (hideWhileResting && lunaRest != null && lunaRest.isResting)
        {
            return false;
        }

        if (hideWhileClimbing && IsClimbing())
        {
            return false;
        }

        if (IsLedgeHanging())
        {
            return false;
        }

        if (IsLedgeClimbing())
        {
            return false;
        }

        if (bobOnlyWhenGrounded && !IsGrounded())
        {
            return false;
        }

        if (characterHorizontalMovement == null)
        {
            return false;
        }

        float movement = Mathf.Abs(characterHorizontalMovement.HorizontalMovementForce);
        return movement > bobMovementThreshold;
    }

    private void UpdateVisibility()
    {
        bool shouldHide = false;

        if (!_isEquipped)
        {
            shouldHide = true;
        }

        if (!shouldHide && hideWhileResting && lunaRest != null && lunaRest.isResting)
        {
            shouldHide = true;
        }

        if (!shouldHide && hideWhileClimbing && IsClimbing())
        {
            shouldHide = true;
        }

        if (!shouldHide && hideWhileLedgeClimbing && IsLedgeClimbing())
        {
            shouldHide = true;
        }

        bool shouldShow = !shouldHide;

        if (_lastVisibleState != shouldShow)
        {
            SetCrownVisible(shouldShow);
            _lastVisibleState = shouldShow;

            if (debugLogs)
            {
                Debug.Log("MoonflowerCrown visibility: " + shouldShow);
            }
        }
    }

    private bool IsFacingLeft()
    {
        if (lunaRenderer != null)
        {
            return lunaRenderer.flipX;
        }

        Transform root = transform.root;
        if (root != null)
        {
            return root.localScale.x < 0f;
        }

        return false;
    }

    private bool IsClimbing()
    {
        if (character == null)
        {
            return false;
        }

        return character.MovementState.CurrentState == CharacterStates.MovementStates.LadderClimbing;
    }

    private bool IsLedgeHanging()
    {
        if (character == null)
        {
            return false;
        }

        return character.MovementState.CurrentState == CharacterStates.MovementStates.LedgeHanging;
    }

    private bool IsLedgeClimbing()
    {
        if (character == null)
        {
            return false;
        }

        return character.MovementState.CurrentState == CharacterStates.MovementStates.LedgeClimbing;
    }

    private bool IsGrounded()
    {
        if (character == null)
        {
            return true;
        }

        return character.ConditionState.CurrentState == CharacterStates.CharacterConditions.Normal;
    }

    private void SetCrownVisible(bool visible)
    {
        if (_crownSprites == null || _crownSprites.Length == 0)
        {
            return;
        }

        for (int i = 0; i < _crownSprites.Length; i++)
        {
            if (_crownSprites[i] != null)
            {
                _crownSprites[i].enabled = visible;
            }
        }
    }
}