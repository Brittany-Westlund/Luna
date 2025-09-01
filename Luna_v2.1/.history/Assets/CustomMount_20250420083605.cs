using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;               // ground‐state Luna (with physics)
    public GameObject lunaInFlight;       // flight sprite, child of butterfly
    public Transform butterfly;
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;   // stays active on ground only
    public GameObject butterflySpeechBubble;
    public GameObject sparklePrefab;

    [Header("Animation Speeds")]
    public float animationSpeedFlying = 2f;
    public float animationSpeedNormal = 1f;

    [Header("Movement & Jump")]
    public float baseSpeed        = 2.2f;
    public float bonusSpeed       = 0f;
    public float jumpForce        = 5f;
    public Vector3 dismountOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Flight Timing")]
    public float flightDuration   = 5f;
    public float warningTime      = 1f;
    public float cooldownDuration = 3f;

    [Header("Colors")]
    public Color normalColor;
    public Color warningColor;
    public Color cooldownColor;

    [Header("Spore Attach Points")]
    public Transform sporeHoldPoint;
    public Transform flightSporeAttachPoint;

    [Header("Tea Rose Pollen")]
    public GameObject teaRosePollenFX;
    public int   teaRosePollenCount  = 0;
    public float speedBoostPerPollen = 1f;
    public int   pollenThreshold     = 3;

    [HideInInspector] public bool justDismounted = false;

    [Header("Butterfly Collider (for bob‑fix)")]
    public Collider2D butterflyPhysicalCollider;

    // Internals
    private Rigidbody2D     _rbButterfly;
    private SpriteRenderer  _sprLuna;
    private Collider2D[]    _colsLuna;
    private LunaSporeSystem _spore;
    private FlowerHolder    _holder;

    private float  _flightTimer;
    private bool   _isFlying;
    private bool   _warningTriggered;
    private bool   _canBeExtended;
    private bool   _hasTempBoost;
    private float  _nextTempBoost;
    private bool   _inCooldown;
    private bool   _isFacingRight = true;

    void Start()
    {
        // Cache components
        _rbButterfly = butterfly.GetComponent<Rigidbody2D>();
        _sprLuna     = luna.GetComponent<SpriteRenderer>();
        _spore       = luna.GetComponent<LunaSporeSystem>();
        _holder      = luna.GetComponent<FlowerHolder>();
        _colsLuna    = luna.GetComponentsInChildren<Collider2D>();

        // Ground spore attach
        _spore.attachPoint = sporeHoldPoint;

        // Initial visuals
        luna.SetActive(true);
        lunaInFlight.SetActive(false);
        butterflyRenderer.color = normalColor;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // Make butterfly kinematic on ground
        _rbButterfly.bodyType    = RigidbodyType2D.Kinematic;
        _rbButterfly.gravityScale = 0f;

        // Prevent any collision between butterfly and Luna colliders
        if (butterflyPhysicalCollider != null)
        {
            foreach (var c in _colsLuna)
                if (!c.isTrigger)
                    Physics2D.IgnoreCollision(butterflyPhysicalCollider, c, true);
        }

        // Initial facing
        _isFacingRight = !butterflyRenderer.flipX;
    }

    void Update()
    {
        // F to mount / extend / dismount
        if (Input.GetKeyDown(KeyCode.F) && !_inCooldown)
        {
            if (_isFlying)
            {
                if (_spore.HasSporeAttached && _canBeExtended)
                    ExtendFlight();
                else
                    Dismount(false);
            }
            else
            {
                Mount();
            }
            return;
        }

        if (!_isFlying) return;

        // In-flight controls
        FlyMovement();
        HandleFacing();
        HandleTimer();

        if (Input.GetKeyDown(KeyCode.R))
        {
            _spore.CreateSpore();
            StartCoroutine(DestroySporeAfterDelay());
            if (_canBeExtended)
                ExtendFlight();
        }

        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        if (Input.GetKeyDown(KeyCode.X))
            HandleAirInteract();
    }

    private void Mount()
    {
        justDismounted    = false;
        _isFlying         = true;
        _flightTimer      = 0f;
        _warningTriggered = false;
        _canBeExtended    = false;

        // Swap visuals
        luna.SetActive(false);
        lunaInFlight.SetActive(true);
        _spore.attachPoint = flightSporeAttachPoint;

        // Disable ground-only facing and UI
        followAndFlip.enabled = false;
        butterflySpeechBubble?.SetActive(false);
        sparklePrefab?.SetActive(true);

        // Camera to butterfly
        vCam.Follow = butterfly;

        // Flight animation & physics
        butterflyAnimator.speed   = animationSpeedFlying;
        _rbButterfly.bodyType     = RigidbodyType2D.Dynamic;
        _rbButterfly.gravityScale = 0f;
    }

    private void Dismount(bool jumpOff)
    {
        // Swap back visuals
        lunaInFlight.SetActive(false);
        luna.SetActive(true);
        _spore.attachPoint = sporeHoldPoint;

        // Reposition ground Luna
        luna.transform.position = butterfly.position + dismountOffset;

        // Restore ground facing & camera
        followAndFlip.enabled     = true;
        vCam.Follow               = luna.transform;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);

        // Ground animation & optional jump
        butterflyAnimator.speed = animationSpeedNormal;
        if (jumpOff)
            luna.GetComponent<Rigidbody2D>().velocity = new Vector2(0, jumpForce);

        // Cooldown
        butterflyRenderer.color = cooldownColor;
        _inCooldown             = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        _isFlying = false;
    }

    private void FlyMovement()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  dir.x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x = +1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    dir.y = +1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  dir.y = -1f;

        if (dir.sqrMagnitude > 0.001f)
        {
            dir.Normalize();
            float speed = baseSpeed + bonusSpeed + (_hasTempBoost ? _nextTempBoost : 0f);
            if (_hasTempBoost)
            {
                _hasTempBoost = false;
                teaRosePollenFX?.SetActive(false);
            }
            butterfly.position += (Vector3)(dir * speed * Time.deltaTime);
        }
    }

    private void HandleFacing()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _isFacingRight = true;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _isFacingRight = false;

        bool flip = !_isFacingRight;
        butterflyRenderer.flipX = flip;
        _sprLuna.flipX          = flip;
        lunaInFlight.GetComponent<SpriteRenderer>().flipX = flip;
    }

    private void HandleTimer()
    {
        _flightTimer += Time.deltaTime;
        if (!_warningTriggered && _flightTimer >= flightDuration - warningTime)
        {
            _warningTriggered = true;
            _canBeExtended    = true;
            StartCoroutine(FlashWarning());
        }
        if (_flightTimer >= flightDuration)
            Dismount(false);
    }

    private void ExtendFlight()
    {
        _flightTimer = Mathf.Max(0f, _flightTimer - warningTime);
        _spore.DestroyAttachedSpore();
        _canBeExtended         = false;
        butterflyRenderer.color = normalColor;
    }

    private IEnumerator DestroySporeAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);
        _spore.DestroyAttachedSpore();
    }

    private void HandleAirInteract()
    {
        var hits = Physics2D.OverlapCircleAll(butterfly.position, 1f);
        foreach (var col in hits)
        {
            if (col.CompareTag("Flower") && !_holder.HasFlower)
            {
                _holder.PickUpFlower(col.gameObject);
                return;
            }
            var spot = col.GetComponent<GardenSpot>();
            if (spot != null && _holder.HasFlower)
            {
                var f = _holder.GetHeldFlower();
                f.transform.SetParent(spot.transform, false);
                f.transform.localPosition = Vector3.zero;
                _holder.DropFlower();
                var spr = f.GetComponent<SproutAndLightManager>();
                if (spr != null) spr.isPlanted = true;
                return;
            }
        }
    }

    private IEnumerator FlashWarning()
    {
        for (int i = 0; i < 3; i++)
        {
            butterflyRenderer.color = warningColor;
            yield return new WaitForSeconds(0.2f);
            butterflyRenderer.color = cooldownColor;
            yield return new WaitForSeconds(0.2f);
        }
        butterflyRenderer.color = cooldownColor;
    }

    private void EndCooldown()
    {
        _inCooldown = false;
        butterflyRenderer.color = normalColor;
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost   = true;
        _nextTempBoost  = speedBoost;
        teaRosePollenFX?.SetActive(true);

        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed         += speedBoost;
            teaRosePollenCount  = 0;
        }
    }
}
