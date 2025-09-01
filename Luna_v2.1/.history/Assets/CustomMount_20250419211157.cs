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

    [Header("Butterfly Physical Collider")]
    public Collider2D butterflyPhysicalCollider;

    // internals
    Rigidbody2D     _rbButterfly;
    Collider2D      _colButterfly;
    Collider2D[]    _colsLuna;
    Rigidbody2D     _rbLuna;
    float           _lunaGravity;
    SpriteRenderer  _sprLuna;
    LunaSporeSystem _spore;
    FlowerHolder    _holder;

    float  _flightTimer;
    bool   _isFlying;
    bool   _warningTriggered;
    bool   _canBeExtended;
    bool   _hasTempBoost;
    float  _nextTempBoost;
    bool   _inCooldown;

    // track facing purely by input
    bool _isFacingRight = true;

    void Start()
    {
        _rbButterfly  = butterfly.GetComponent<Rigidbody2D>();
        _colButterfly = butterfly.GetComponent<Collider2D>();

        _rbLuna      = luna.GetComponent<Rigidbody2D>();
        if (_rbLuna != null) _lunaGravity = _rbLuna.gravityScale;

        _sprLuna     = luna.GetComponent<SpriteRenderer>();
        _spore       = luna.GetComponent<LunaSporeSystem>();
        _holder      = luna.GetComponent<FlowerHolder>();
        _colsLuna    = luna.GetComponentsInChildren<Collider2D>();

        // default attach on ground
        _spore.attachPoint = sporeHoldPoint;

        // initial visuals/physics
        _sprLuna.enabled        = true;
        lunaInFlight.SetActive(false);
        butterflyRenderer.color = normalColor;
        _rbButterfly.bodyType   = RigidbodyType2D.Kinematic;
        _rbButterfly.gravityScale = 0f;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        _isFacingRight = !butterflyRenderer.flipX;
    }

    void Update()
    {
        // F = mount / extend / dismount
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

        // Flight controls
        if (_isFlying)
        {
            HandleMovement();
            HandleFacing();
            HandleTimer();

            // R: create & (if ready) consume + extend
            if (Input.GetKeyDown(KeyCode.R))
            {
                _spore.CreateSpore();

                if (_canBeExtended)
                {
                    // show it briefly, then consume
                    StartCoroutine(ConsumeSpore());
                    ExtendFlight();
                }
            }

            // Space = jump off
            if (Input.GetButtonDown("Jump"))
                Dismount(true);

            // X = mid‑air pick/plant
            if (Input.GetKeyDown(KeyCode.X))
                HandleAirInteract();
        }
    }

    IEnumerator ConsumeSpore()
    {
        // wait for the spore to slide into place
        yield return new WaitForSeconds(0.3f);
        _spore.DestroyAttachedSpore();
    }

    void HandleMovement()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  dir.x = -1f;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x = +1f;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    dir.y = +1f;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  dir.y = -1f;

        if (dir != Vector2.zero)
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

    void HandleFacing()
    {
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _isFacingRight = true;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _isFacingRight = false;

        // flip butterfly sprite
        butterflyRenderer.flipX = !_isFacingRight;

        // flip Luna‑in‑flight
        Vector3 ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;
    }

    void Mount()
    {
        justDismounted = false;

        // swap visuals
        _sprLuna.enabled    = false;
        lunaInFlight.SetActive(true);
        _spore.attachPoint  = flightSporeAttachPoint;

        // disable physical colliders to prevent bobbing
        if (butterflyPhysicalCollider != null)
            butterflyPhysicalCollider.enabled = false;
        foreach (var c in _colsLuna)
            Physics2D.IgnoreCollision(_colButterfly, c, true);

        // camera & FX
        vCam.Follow                = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled      = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // matching facing on mount
        butterflyRenderer.flipX = !_isFacingRight;
        Vector3 ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        // flight physics & animation
        butterflyAnimator.speed   = animationSpeedFlying;
        _rbButterfly.bodyType     = RigidbodyType2D.Dynamic;
        _rbButterfly.gravityScale = 0f;

        // reset timers/flags
        _flightTimer      = 0f;
        _warningTriggered = false;
        _canBeExtended    = false;
        _isFlying         = true;
    }

    void Dismount(bool jumpOff)
    {
        // swap visuals back
        lunaInFlight.SetActive(false);
        _sprLuna.enabled = true;
        _spore.attachPoint = sporeHoldPoint;

        // re-enable physical colliders
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

        // reposition Luna
        luna.transform.position = butterfly.position + dismountOffset;

        // camera & FX
        vCam.Follow              = luna.transform;
        followAndFlip.enabled    = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);

        // ground animation & jump
        butterflyAnimator.speed  = animationSpeedNormal;
        if (jumpOff && _rbLuna != null)
            _rbLuna.velocity = new Vector2(_rbLuna.velocity.x, jumpForce);

        // cooldown tint
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
        _canBeExtended    = false;
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
        _inCooldown = false;
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
            bonusSpeed         += speedBoost;
            teaRosePollenCount  = 0;
        }
    }
}
