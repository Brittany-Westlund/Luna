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

    [Header("Movement & Jump")]
    public float baseSpeed        = 2.2f;
    public float bonusSpeed       = 0f;
    public float jumpForce        = 5f;
    public Vector3 dismountOffset = new Vector3(0f, -0.5f, 0f);

    [Header("Smoothing")]
    [Tooltip("Seconds to smooth accel/decel")]
    public float smoothingTime = 0.1f;

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

    [Header("Pollen")]
    public GameObject teaRosePollenFX;
    public int   teaRosePollenCount  = 0;
    public float speedBoostPerPollen = 1f;
    public int   pollenThreshold     = 3;

    [HideInInspector] public bool justDismounted = false;

    [Header("Butterfly Physical Collider")]
    public Collider2D butterflyPhysicalCollider;

    // internals
    private Rigidbody2D     _rbButterfly;
    private Collider2D      _colButterfly;
    private Collider2D[]    _colsLuna;
    private Rigidbody2D     _rbLuna;
    private float           _lunaGravity;
    private SpriteRenderer  _sprLuna;
    private LunaSporeSystem _spore;
    private FlowerHolder    _holder;

    private float  _flightTimer;
    private bool   _isFlying;
    private bool   _warningTriggered;
    private bool   _canBeExtended;
    private bool   _hasTempBoost;
    private float  _nextTempBoost;
    private bool   _inCooldown;

    // smoothing state
    private Vector2 _currentVel = Vector2.zero;

    // input-driven facing
    private bool _isFacingRight = true;
    public float animationSpeedFlying = 2f;
    public float animationSpeedNormal = 1f;


    void Start()
    {
        _rbButterfly  = butterfly.GetComponent<Rigidbody2D>();
        _colButterfly = butterfly.GetComponent<Collider2D>();
        _rbLuna       = luna.GetComponent<Rigidbody2D>();
        if (_rbLuna != null) _lunaGravity = _rbLuna.gravityScale;

        _sprLuna   = luna.GetComponent<SpriteRenderer>();
        _spore     = luna.GetComponent<LunaSporeSystem>();
        _holder    = luna.GetComponent<FlowerHolder>();
        _colsLuna  = luna.GetComponentsInChildren<Collider2D>();

        _spore.attachPoint = sporeHoldPoint;

        // initial visuals
        _sprLuna.enabled    = true;
        lunaInFlight.SetActive(false);
        butterflyRenderer.color = normalColor;
        _rbButterfly.bodyType   = RigidbodyType2D.Kinematic;
        _rbButterfly.gravityScale = 0f;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // initial facing
        _isFacingRight = !butterflyRenderer.flipX;
    }

    void Update()
    {
        // F = mount/extend/dismount
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!_inCooldown)
            {
                if (_isFlying)
                {
                    if (_spore.HasSporeAttached && _canBeExtended) ExtendFlight();
                    else                                           Dismount(false);
                }
                else Mount();
            }
            return;
        }

        // Flight controls when airborne
        if (_isFlying)
        {
            HandleMovement();
            HandleFacing();
            HandleTimer();

            // R: spawn spore + immediate extend if ready
            if (Input.GetKeyDown(KeyCode.R))
            {
                _spore.CreateSpore();
                if (_canBeExtended) ExtendFlight();
            }

            // Space: jump off
            if (Input.GetButtonDown("Jump"))
                Dismount(true);

            // X: mid-air pick/plant
            if (Input.GetKeyDown(KeyCode.X))
                HandleAirInteract();
        }
    }

    void HandleMovement()
    {
        // direct key checks for crisp movement
        Vector2 input = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  input.x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) input.x = +1;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    input.y = +1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  input.y = -1;

        if (input != Vector2.zero)
            input.Normalize();

        float speed = baseSpeed + bonusSpeed + (_hasTempBoost ? _nextTempBoost : 0f);
        if (_hasTempBoost)
        {
            _hasTempBoost = false;
            teaRosePollenFX?.SetActive(false);
        }

        // smooth velocity
        Vector2 targetVel = input * speed;
        _currentVel = Vector2.SmoothDamp(_currentVel, targetVel, ref _currentVel, smoothingTime);

        // apply movement
        butterfly.position += (Vector3)(_currentVel * Time.deltaTime);
    }

    void HandleFacing()
    {
        // A/D override
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _isFacingRight = true;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _isFacingRight = false;

        // apply flip
        butterflyRenderer.flipX = !_isFacingRight;
        Vector3 ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ?  Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;
    }

    void Mount()
    {
        justDismounted = false;

        // show flight sprite
        _sprLuna.enabled = false;
        lunaInFlight.SetActive(true);

        // attach spore
        _spore.attachPoint = flightSporeAttachPoint;

        // disable physical collisions
        if (butterflyPhysicalCollider != null)
            butterflyPhysicalCollider.enabled = false;

        // ignore butterfly↔Luna
        foreach (var c in _colsLuna)
            Physics2D.IgnoreCollision(_colButterfly, c, true);

        // camera & FX
        vCam.Follow = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // enforce facing
        butterflyRenderer.flipX = !_isFacingRight;
        Vector3 ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        // physics
        butterflyAnimator.speed   = animationSpeedFlying;
        _rbButterfly.bodyType     = RigidbodyType2D.Dynamic;
        _rbButterfly.gravityScale = 0f;

        // reset flight state
        _flightTimer      = 0f;
        _warningTriggered = false;
        _canBeExtended    = false;
        _isFlying         = true;
    }

    void Dismount(bool jumpOff)
    {
        // show ground sprite
        lunaInFlight.SetActive(false);
        _sprLuna.enabled = true;

        // re‑attach spore
        _spore.attachPoint = sporeHoldPoint;

        // restore collisions immediately
        if (butterflyPhysicalCollider != null)
            butterflyPhysicalCollider.enabled = true;
        foreach (var c in _colsLuna)
            Physics2D.IgnoreCollision(_colButterfly, c, false);

        // restore Luna physics
        if (_rbLuna != null)
        {
            _rbLuna.velocity     = Vector2.zero;
            _rbLuna.gravityScale = _lunaGravity;
        }

        // reposition & camera
        luna.transform.position = butterfly.position + dismountOffset;
        vCam.Follow              = luna.transform;
        followAndFlip.enabled    = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);
        butterflyAnimator.speed  = animationSpeedNormal;

        // jump impulse
        if (jumpOff && _rbLuna != null)
            _rbLuna.velocity = new Vector2(_rbLuna.velocity.x, jumpForce);

        // cooldown color
        butterflyRenderer.color = cooldownColor;
        _inCooldown             = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        _isFlying = false;
    }

    void HandleTimer()
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

    void ExtendFlight()
    {
        _flightTimer = Mathf.Max(0f, _flightTimer - warningTime);
        _spore.DestroyAttachedSpore();
        _canBeExtended = false;
        butterflyRenderer.color = normalColor;
    }

    void HandleAirInteract()
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
            if (spot != null && !_holder.HasFlower)
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

    IEnumerator FlashWarning()
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

    void EndCooldown()
    {
        _inCooldown           = false;
        butterflyRenderer.color = normalColor;
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost  = true;
        _nextTempBoost = speedBoost;
        teaRosePollenFX?.SetActive(true);

        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed       += speedBoost;
            teaRosePollenCount = 0;
        }
    }
}
