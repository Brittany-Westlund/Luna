using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;
    public GameObject lunaInFlight;
    public Transform butterfly;
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;
    public GameObject butterflySpeechBubble;
    public GameObject sparklePrefab;
    public AudioSource lunaFootstepAudio;

    [Header("Luna Ground Visuals")]
    public SpriteRenderer lunaGroundRenderer;
    public Animator lunaGroundAnimator;

    [Header("Luna In-Flight Visuals")]
    public SpriteRenderer lunaInFlightRenderer;
    public Animator lunaInFlightAnimator;

    [Header("Animation Speeds")]
    public float animationSpeedFlying = 2f;
    public float animationSpeedNormal = 1f;

    [Header("Movement & Jump")]
    public float baseSpeed = 2.2f;
    public float bonusSpeed = 0f;
    public float jumpForce = 5f;
    public Vector3 dismountOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Flight Timing")]
    public float flightDuration = 5f;
    public float warningTime = 1f;
    public float cooldownDuration = 3f;

    [Header("Extension Settings")]
    public float extendTime = 1f;

    [Header("Butterfly Colors")]
    public Color normalColor = Color.white;
    public Color warningColor = Color.yellow;
    public Color cooldownColor = Color.gray;

    [Header("Spore Attach Points")]
    public Transform sporeHoldPoint;
    public Transform flightSporeAttachPoint;

    [Header("Tea Rose Pollen")]
    public GameObject teaRosePollenFX;
    public int teaRosePollenCount = 0;
    public float speedBoostPerPollen = 0.2f;
    public int pollenThreshold = 6;
    public float maxBonusSpeed = 1f;

    [Header("Buttercup Pollen")]
    public GameObject buttercupPollenIcon;
    public Transform buttercupPollenHoldPoint;
    public float buttercupIconDuration = 7f;

    [HideInInspector] public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    [Header("Flower Hold Points")]
    public Transform groundFlowerHoldPoint;
    public Transform flightFlowerHoldPoint;

    [Header("Air-Interact Settings")]
    public float airInteractRadius = 0.3f;

    [Header("Mount Settings")]
    public float maxMountDistance = 2f;
    public float mountHoldTime = 0.35f;
    public KeyCode mountKey = KeyCode.B;

    [Header("Ground Follow Settings")]
    public float followSpeed = 5f;
    public float followDistance = 0.7f;
    public float horizontalOffset = 0.5f;
    public float offsetFlipSpeed = 3f;
    public bool startFollowing = false;
    public float lunaMovementDeadZone = 0.001f;

    [Header("Wand Hold Points")]
    public Transform groundWandHoldPoint;
    public Transform flightWandHoldPoint;

    [Header("Limits")]
    public FlightBoundaryLimiter flightLimiter;

    [Header("Pollen Hold Points")]
    public Transform anemonePollenGroundHoldPoint;
    public Transform anemonePollenFlightHoldPoint;
    public Transform foxglovePollenGroundHoldPoint;
    public Transform foxglovePollenFlightHoldPoint;
    public Transform goldenrodPollenGroundHoldPoint;
    public Transform goldenrodPollenFlightHoldPoint;

    public LunaSporeSystem _spore;

    private FlowerHolder _holder;
    private Rigidbody2D _rbLuna;
    private SpriteRenderer _sprLuna;
    private Animator _animLuna;
    private Coroutine _flashWarningCoroutine;
    private float _flightTimer;
    public bool _isFlying;
    private bool _warningTriggered;
    private bool _canExtend;
    private bool _hasTempBoost;
    private float _nextTempBoost;
    private bool _inCooldown;
    private bool _isFacingRight = true;
    private GameObject _wandObj;
    private ButterflyFatigue _fatigue;

    private float _rHoldTimer = 0f;
    private bool _followWasActiveBeforeMount = false;

    private bool _isFollowing = false;
    private Transform _followTarget;
    private float _currentHorizontalOffset;
    private float _targetHorizontalOffset;
    private float _lastLunaX;

    void Start()
    {
        if (!luna)
            luna = GameObject.FindWithTag("Player");

        if (!butterfly)
            butterfly = transform;

        if (luna != null)
        {
            _spore = luna.GetComponent<LunaSporeSystem>();
            _holder = luna.GetComponent<FlowerHolder>();
            _rbLuna = luna.GetComponent<Rigidbody2D>();
            _followTarget = luna.transform;
            _lastLunaX = luna.transform.position.x;
        }

        if (_spore != null && sporeHoldPoint != null)
            _spore.attachPoint = sporeHoldPoint;

        _sprLuna = lunaGroundRenderer;
        if (_sprLuna == null && luna != null)
        {
            _sprLuna = luna.GetComponent<SpriteRenderer>();
            if (_sprLuna == null)
                _sprLuna = luna.GetComponentInChildren<SpriteRenderer>(true);
        }

        _animLuna = lunaGroundAnimator;
        if (_animLuna == null && luna != null)
        {
            _animLuna = luna.GetComponent<Animator>();
            if (_animLuna == null)
                _animLuna = luna.GetComponentInChildren<Animator>(true);
        }

        if (lunaInFlightRenderer == null && lunaInFlight != null)
        {
            lunaInFlightRenderer = lunaInFlight.GetComponent<SpriteRenderer>();
            if (lunaInFlightRenderer == null)
                lunaInFlightRenderer = lunaInFlight.GetComponentInChildren<SpriteRenderer>(true);
        }

        if (lunaInFlightAnimator == null && lunaInFlight != null)
        {
            lunaInFlightAnimator = lunaInFlight.GetComponent<Animator>();
            if (lunaInFlightAnimator == null)
                lunaInFlightAnimator = lunaInFlight.GetComponentInChildren<Animator>(true);
        }

        if (_sprLuna != null)
            _sprLuna.enabled = true;

        if (_animLuna != null)
            _animLuna.enabled = true;

        if (lunaInFlight != null)
            lunaInFlight.SetActive(false);

        if (sparklePrefab != null)
            sparklePrefab.SetActive(false);

        if (teaRosePollenFX != null)
            teaRosePollenFX.SetActive(false);

        _isFacingRight = true;
        _isFollowing = startFollowing;
        _currentHorizontalOffset = -Mathf.Abs(horizontalOffset);
        _targetHorizontalOffset = _currentHorizontalOffset;

        if (_holder != null && groundFlowerHoldPoint != null)
            _holder.holdPoint = groundFlowerHoldPoint;

        LunariaWandAttractor wand = FindObjectOfType<LunariaWandAttractor>();
        if (wand != null)
            _wandObj = wand.gameObject;

        _fatigue = GetComponent<ButterflyFatigue>();

        // You said butterfly needed this direction.
        if (butterflyRenderer != null)
            butterflyRenderer.flipX = _isFacingRight;

        if (lunaInFlightRenderer != null)
            lunaInFlightRenderer.flipX = _isFacingRight;

        EnforceLunaVisibilityLock();
        ApplyVisualFacing();
    }

    void Update()
    {
        EnforceLunaVisibilityLock();

        HandleGroundRInput();

        if (!_isFlying)
        {
            UpdateGroundFacingAndOffset();

            if (_isFollowing)
                FollowLunaGround();

            return;
        }

        Vector2 dir = HandleMovement();
        HandleFlightFacing(dir);
        HandleTimer();

        if (_fatigue != null && _fatigue.IsExhausted() && _isFlying)
        {
            Dismount(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            if (_spore != null)
            {
                GameObject sp = _spore.CreateSpore();
                if (sp != null)
                {
                    ExtendFlight();
                    Destroy(sp, 0.1f);
                }
            }
        }

        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        if (Input.GetKeyDown(KeyCode.F))
            HandleAirInteract();
    }

    void LateUpdate()
    {
        EnforceLunaVisibilityLock();
    }

    void HandleGroundRInput()
    {
        if (_isFlying)
        {
            _rHoldTimer = 0f;
            return;
        }

        if (_inCooldown)
        {
            _rHoldTimer = 0f;
            return;
        }

        if (luna == null || butterfly == null)
        {
            _rHoldTimer = 0f;
            return;
        }

        bool closeEnough = Vector2.Distance(luna.transform.position, butterfly.position) <= maxMountDistance;

        if (Input.GetKeyDown(mountKey))
            _rHoldTimer = 0f;

        if (Input.GetKey(mountKey) && closeEnough)
        {
            _rHoldTimer += Time.deltaTime;

            if (_rHoldTimer >= mountHoldTime)
            {
                _rHoldTimer = 0f;
                Mount();
                return;
            }
        }

        if (Input.GetKeyUp(mountKey))
        {
            if (_rHoldTimer < mountHoldTime)
                ToggleGroundFollow();

            _rHoldTimer = 0f;
        }
    }

    void ToggleGroundFollow()
    {
        _isFollowing = !_isFollowing;

        if (!_isFollowing)
        {
            StopCoroutine(nameof(LowerToLunaLevelMerged));
            StartCoroutine(nameof(LowerToLunaLevelMerged));
        }
    }

    void UpdateGroundFacingAndOffset()
    {
        if (luna == null)
            return;

        float lunaDX = luna.transform.position.x - _lastLunaX;

        if (Mathf.Abs(lunaDX) > lunaMovementDeadZone)
            _isFacingRight = lunaDX > 0f;

        _lastLunaX = luna.transform.position.x;

        _targetHorizontalOffset = _isFacingRight
            ? -Mathf.Abs(horizontalOffset)
            : Mathf.Abs(horizontalOffset);

        _currentHorizontalOffset = Mathf.Lerp(
            _currentHorizontalOffset,
            _targetHorizontalOffset,
            offsetFlipSpeed * Time.deltaTime
        );

        ApplyVisualFacing();
    }

    void FollowLunaGround()
    {
        if (_followTarget == null || butterfly == null)
            return;

        Vector3 goal = _followTarget.position + new Vector3(_currentHorizontalOffset, followDistance, 0f);
        butterfly.position = Vector3.Lerp(butterfly.position, goal, followSpeed * Time.deltaTime);
    }

    IEnumerator LowerToLunaLevelMerged()
    {
        if (_followTarget == null || butterfly == null)
            yield break;

        Vector3 goal = new Vector3(
            butterfly.position.x,
            _followTarget.position.y,
            butterfly.position.z);

        while (Mathf.Abs(butterfly.position.y - goal.y) > 0.01f)
        {
            butterfly.position = Vector3.Lerp(butterfly.position, goal, followSpeed * Time.deltaTime);
            yield return null;
        }

        butterfly.position = goal;
    }

    Vector2 HandleMovement()
    {
        Vector2 dir = Vector2.zero;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) dir.x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x = 1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) dir.y = 1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) dir.y = -1f;

        if (dir != Vector2.zero && butterfly != null)
        {
            dir.Normalize();
            float speed = baseSpeed + bonusSpeed + (_hasTempBoost ? _nextTempBoost : 0f);
            butterfly.position += (Vector3)(dir * speed * Time.deltaTime);
        }

        return dir;
    }

    void HandleFlightFacing(Vector2 dir)
    {
        if (dir.x > 0.01f)
            _isFacingRight = true;
        else if (dir.x < -0.01f)
            _isFacingRight = false;

        ApplyVisualFacing();
    }

    void ApplyVisualFacing()
    {
        // You said this is the correct polarity for the butterfly.
        if (butterflyRenderer != null)
            butterflyRenderer.flipX = _isFacingRight;

        // Reinstall the same polarity onto LunaInFlight.
        if (lunaInFlightRenderer != null)
            lunaInFlightRenderer.flipX = _isFacingRight;
    }

    void EnforceLunaVisibilityLock()
    {
        bool shouldGroundBeVisible = !_isFlying;

        if (_sprLuna != null && _sprLuna.enabled != shouldGroundBeVisible)
            _sprLuna.enabled = shouldGroundBeVisible;

        if (_animLuna != null && _animLuna.enabled != shouldGroundBeVisible)
            _animLuna.enabled = shouldGroundBeVisible;

        if (lunaInFlight != null && lunaInFlight.activeSelf != _isFlying)
            lunaInFlight.SetActive(_isFlying);

        if (lunaInFlightAnimator != null)
            lunaInFlightAnimator.enabled = _isFlying;
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (_isFlying)
            return;

        if (_followTarget != null && col.transform == _followTarget)
            UpdateGroundFacingAndOffset();
    }

    void Mount()
    {
        if (_isFlying)
            return;

        if (_fatigue != null && _fatigue.WouldExceedFatigue())
            return;

        justDismounted = false;
        _isFlying = true;
        _canExtend = false;
        _warningTriggered = false;
        _flightTimer = 0f;
        _rHoldTimer = 0f;
        _followWasActiveBeforeMount = _isFollowing;

        if (_spore != null)
            _spore.enabled = false;

        EnforceLunaVisibilityLock();
        ApplyVisualFacing();

        if (lunaFootstepAudio != null)
            lunaFootstepAudio.enabled = false;

        if (flightLimiter != null)
            flightLimiter.enabled = true;

        if (_spore != null && flightSporeAttachPoint != null)
            _spore.attachPoint = flightSporeAttachPoint;

        if (sporeHoldPoint != null && flightSporeAttachPoint != null && sporeHoldPoint.childCount > 0)
        {
            Transform sp = sporeHoldPoint.GetChild(0);
            sp.SetParent(flightSporeAttachPoint, true);
            sp.localPosition = Vector3.zero;
            sp.localRotation = Quaternion.identity;
        }

        ToggleColliders(butterflySolidColliders, false);
        ToggleColliders(lunaSolidColliders, false);

        if (vCam != null && butterfly != null)
            vCam.Follow = butterfly;

        if (butterflySpeechBubble != null)
            butterflySpeechBubble.SetActive(false);

        if (followAndFlip != null)
            followAndFlip.enabled = false;

        if (sparklePrefab != null)
            sparklePrefab.SetActive(true);

        if (butterflyAnimator != null)
            butterflyAnimator.speed = animationSpeedFlying;

        if (_fatigue != null)
            _fatigue.ApplyFatigue();

        if (_holder != null && flightFlowerHoldPoint != null)
            _holder.holdPoint = flightFlowerHoldPoint;

        if (_holder != null && _holder.HasFlower && flightFlowerHoldPoint != null)
        {
            GameObject f = _holder.GetHeldFlower();
            if (f != null)
            {
                f.transform.SetParent(flightFlowerHoldPoint, true);
                f.transform.localPosition = Vector3.zero;
                f.transform.localRotation = Quaternion.identity;
            }
        }

        if (_wandObj != null && flightWandHoldPoint != null && luna != null)
        {
            if (_wandObj.transform.parent == luna.transform)
            {
                _wandObj.transform.SetParent(flightWandHoldPoint, true);
                _wandObj.transform.localPosition = Vector3.zero;
                _wandObj.transform.localRotation = Quaternion.identity;
            }
        }

        MoveIfChildExists(anemonePollenGroundHoldPoint, anemonePollenFlightHoldPoint);
        MoveIfChildExists(foxglovePollenGroundHoldPoint, foxglovePollenFlightHoldPoint);
        MoveIfChildExists(goldenrodPollenGroundHoldPoint, goldenrodPollenFlightHoldPoint);
    }

    void Dismount(bool jumpOff)
    {
        if (!_isFlying)
            return;

        _isFlying = false;
        _rHoldTimer = 0f;

        if (luna != null && butterfly != null)
            luna.transform.position = butterfly.position + dismountOffset;

        _inCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        if (flightLimiter != null)
            flightLimiter.enabled = false;

        ApplyVisualFacing();

        if (_spore != null)
        {
            _spore.enabled = true;
            if (sporeHoldPoint != null)
                _spore.attachPoint = sporeHoldPoint;
        }

        if (flightSporeAttachPoint != null && sporeHoldPoint != null && flightSporeAttachPoint.childCount > 0)
        {
            Transform sp = flightSporeAttachPoint.GetChild(0);
            sp.SetParent(sporeHoldPoint, true);
            sp.localPosition = Vector3.zero;
            sp.localRotation = Quaternion.identity;
        }

        if (_holder != null && groundFlowerHoldPoint != null)
            _holder.holdPoint = groundFlowerHoldPoint;

        if (_holder != null && _holder.HasFlower && groundFlowerHoldPoint != null)
        {
            GameObject f = _holder.GetHeldFlower();
            if (f != null)
            {
                f.transform.SetParent(groundFlowerHoldPoint, true);
                f.transform.localPosition = Vector3.zero;
                f.transform.localRotation = Quaternion.identity;
            }
        }

        if (_wandObj != null && groundWandHoldPoint != null)
        {
            _wandObj.transform.SetParent(groundWandHoldPoint, true);
            _wandObj.transform.localPosition = Vector3.zero;
            _wandObj.transform.localRotation = Quaternion.identity;
        }

        MoveIfChildExists(anemonePollenFlightHoldPoint, anemonePollenGroundHoldPoint);
        MoveIfChildExists(foxglovePollenFlightHoldPoint, foxglovePollenGroundHoldPoint);
        MoveIfChildExists(goldenrodPollenFlightHoldPoint, goldenrodPollenGroundHoldPoint);

        EnforceLunaVisibilityLock();
        ApplyVisualFacing();

        if (lunaFootstepAudio != null)
            lunaFootstepAudio.enabled = true;

        ToggleColliders(butterflySolidColliders, true);
        ToggleColliders(lunaSolidColliders, true);

        if (vCam != null && luna != null)
            vCam.Follow = luna.transform;

        if (sparklePrefab != null)
            sparklePrefab.SetActive(false);

        _isFollowing = _followWasActiveBeforeMount;
        StartCoroutine(ReenableFollowNextFrame());

        if (butterflyAnimator != null)
            butterflyAnimator.speed = animationSpeedNormal;

        if (butterflyRenderer != null)
            butterflyRenderer.color = cooldownColor;

        if (jumpOff && _rbLuna != null)
        {
            _rbLuna.velocity = Vector2.zero;
            _rbLuna.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (butterflyRenderer != null)
            butterflyRenderer.color = cooldownColor;
    }

    void ExtendFlight()
    {
        if (_flashWarningCoroutine != null)
        {
            StopCoroutine(_flashWarningCoroutine);
            _flashWarningCoroutine = null;
        }

        _flightTimer = Mathf.Max(0f, _flightTimer - extendTime);

        if (_fatigue != null && butterflyRenderer != null)
            butterflyRenderer.color = _fatigue.GetFatigueColor();

        if (sparklePrefab != null)
            sparklePrefab.SetActive(true);

        _warningTriggered = false;
        _canExtend = false;
    }

    IEnumerator FlashWarning()
    {
        for (int i = 0; i < 3; i++)
        {
            if (butterflyRenderer != null)
                butterflyRenderer.color = warningColor;

            yield return new WaitForSeconds(0.2f);

            if (_fatigue != null && butterflyRenderer != null)
                butterflyRenderer.color = _fatigue.GetFatigueColor();

            yield return new WaitForSeconds(0.2f);
        }

        if (_fatigue != null && butterflyRenderer != null)
            butterflyRenderer.color = _fatigue.GetFatigueColor();
    }

    void EndCooldown()
    {
        _inCooldown = false;

        if (_fatigue != null && butterflyRenderer != null)
            butterflyRenderer.color = _fatigue.GetFatigueColor();
    }

    void HandleAirInteract()
    {
        if (butterfly == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(butterfly.position, airInteractRadius);
        foreach (Collider2D col in hits)
        {
            if (_holder == null)
                break;

            if (col.CompareTag("Flower") && !_holder.HasFlower)
            {
                _holder.PickUpFlower(col.gameObject);
                return;
            }

            GardenSpot spot = col.GetComponent<GardenSpot>();
            if (spot != null && _holder.HasFlower)
            {
                GameObject f = _holder.GetHeldFlower();
                if (f != null)
                {
                    f.transform.SetParent(spot.transform, false);
                    f.transform.localPosition = Vector3.zero;
                    _holder.DropFlower();

                    SproutAndLightManager spr = f.GetComponent<SproutAndLightManager>();
                    if (spr != null)
                        spr.isPlanted = true;
                }
                return;
            }
        }
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost = true;
        _nextTempBoost = speedBoostPerPollen;

        if (teaRosePollenFX != null)
            teaRosePollenFX.SetActive(true);

        if (teaRosePollenCount >= pollenThreshold)
        {
            bonusSpeed = Mathf.Min(bonusSpeed + speedBoostPerPollen, maxBonusSpeed);
            teaRosePollenCount = 0;
            _hasTempBoost = false;

            if (teaRosePollenFX != null)
                teaRosePollenFX.SetActive(false);
        }
    }

    public void ShowButtercupPollenIcon()
    {
        if (buttercupPollenIcon == null)
            return;

        StopCoroutine(nameof(ButtercupIconRoutine));
        StartCoroutine(ButtercupIconRoutine());
    }

    private IEnumerator ButtercupIconRoutine()
    {
        buttercupPollenIcon.SetActive(true);
        yield return new WaitForSeconds(buttercupIconDuration);
        buttercupPollenIcon.SetActive(false);
    }

    IEnumerator ReenableFollowNextFrame()
    {
        yield return null;

        if (followAndFlip != null)
            followAndFlip.enabled = true;
    }

    void HandleTimer()
    {
        _flightTimer += Time.deltaTime;

        if (!_warningTriggered && _flightTimer >= flightDuration - warningTime)
        {
            _warningTriggered = true;
            _canExtend = true;

            if (sparklePrefab != null)
                sparklePrefab.SetActive(false);

            _flashWarningCoroutine = StartCoroutine(FlashWarning());
        }

        if (_flightTimer >= flightDuration)
            Dismount(false);
    }

    void ToggleColliders(Collider2D[] cols, bool state)
    {
        if (cols == null)
            return;

        foreach (Collider2D c in cols)
        {
            if (c != null)
                c.enabled = state;
        }
    }

    void MoveIfChildExists(Transform from, Transform to)
    {
        if (from == null || to == null)
            return;

        if (from.childCount > 0)
        {
            Transform t = from.GetChild(0);
            t.SetParent(to, true);
            t.localPosition = Vector3.zero;
            t.localRotation = Quaternion.identity;
        }
    }
}

// Golden