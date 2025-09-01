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
    public int   teaRosePollenCount   = 0;
    public float speedBoostPerPollen  = 1f;
    public int   pollenThreshold      = 3;

    [HideInInspector] public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    // Assign every non-trigger collider on the butterfly and on Luna
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    // Internals
    Rigidbody2D    _rbButterfly;
    Rigidbody2D    _rbLuna;
    SpriteRenderer _sprLuna;
    LunaSporeSystem _spore;
    FlowerHolder    _holder;

    private float       _lunaGravity;
    float   _flightTimer;
    bool    _isFlying;
    bool    _warningTriggered;
    bool    _canBeExtended;
    bool    _hasTempBoost;
    float   _nextTempBoost;
    bool    _inCooldown;

    // Facing: remember each transform’s initial sign
    float   _butterflySignX;
    float   _lunaSignX;
    bool    _isFacingRight = true;

    void Start()
    {
        // Cache
        _rbButterfly = butterfly.GetComponent<Rigidbody2D>();
        _rbLuna      = luna.GetComponent<Rigidbody2D>();
        _sprLuna     = luna.GetComponent<SpriteRenderer>();
        _spore       = luna.GetComponent<LunaSporeSystem>();
        _holder      = luna.GetComponent<FlowerHolder>();

        // Attach on ground by default
        _spore.attachPoint = sporeHoldPoint;

        // Initial visuals
        _sprLuna.enabled         = true;
        lunaInFlight.SetActive(false);
        butterflyRenderer.color  = normalColor;
        _rbButterfly.bodyType    = RigidbodyType2D.Kinematic;
        _rbButterfly.gravityScale = 0f;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // Remember the initial X‑scale sign for each
        _butterflySignX = Mathf.Sign(butterfly.localScale.x);
        _lunaSignX      = Mathf.Sign(lunaInFlight.transform.localScale.x);

        // Start by facing right (you can flip this if you prefer)
        _isFacingRight = true;
    }

    void Update()
    {
        // Mount / Extend / Dismount
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

        // If we’re in flight, handle movement + inputs
        if (_isFlying)
        {
            HandleMovement();
            HandleFacing();
            HandleTimer();

            // R: spawn + maybe consume/extend
            if (Input.GetKeyDown(KeyCode.R))
            {
                _spore.CreateSpore();
                if (_canBeExtended)
                {
                    StartCoroutine(ConsumeSporeVisual());
                    ExtendFlight();
                }
            }

            // Space: jump off
            if (Input.GetButtonDown("Jump"))
                Dismount(true);

            // X: pick or plant mid‑air
            if (Input.GetKeyDown(KeyCode.X))
                HandleAirInteract();
        }
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
        // A/D override sets facing
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _isFacingRight = true;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _isFacingRight = false;

        // Apply to butterfly by localScale.x
        Vector3 bScale = butterfly.localScale;
        bScale.x = _butterflySignX * (_isFacingRight ? 1f : -1f);
        butterfly.localScale = bScale;

        // Apply to lunaInFlight
        Vector3 lScale = lunaInFlight.transform.localScale;
        lScale.x = _lunaSignX * (_isFacingRight ? 1f : -1f);
        lunaInFlight.transform.localScale = lScale;
    }

    void Mount()
    {
        justDismounted = false;

        // Swap visuals
        _sprLuna.enabled       = false;
        lunaInFlight.SetActive(true);

        // Move spore attach to butterfly
        _spore.attachPoint     = flightSporeAttachPoint;

        // Disable all “solid” colliders so nothing pushes you
        foreach (var c in butterflySolidColliders) c.enabled = false;
        foreach (var c in lunaSolidColliders)      c.enabled = false;

        // Camera & FX
        vCam.Follow               = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled     = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // Enforce facing based on input state
        HandleFacing();

        // Flight animation + physics
        butterflyAnimator.speed   = animationSpeedFlying;
        _rbButterfly.bodyType     = RigidbodyType2D.Dynamic;
        _rbButterfly.gravityScale = 0f;

        // Reset timers/flags
        _flightTimer      = 0f;
        _warningTriggered = false;
        _canBeExtended    = false;
        _isFlying         = true;
    }

    void Dismount(bool jumpOff)
    {
        // Swap visuals back
        lunaInFlight.SetActive(false);
        _sprLuna.enabled = true;

        // Restore spore attach
        _spore.attachPoint = sporeHoldPoint;

        // Re-enable all colliders
        foreach (var c in butterflySolidColliders) c.enabled = true;
        foreach (var c in lunaSolidColliders)      c.enabled = true;

        // Restore Luna physics
        if (_rbLuna != null)
        {
            _rbLuna.velocity     = Vector2.zero;
            _rbLuna.gravityScale = _lunaGravity;
        }

        // Reposition & camera
        luna.transform.position = butterfly.position + dismountOffset;
        vCam.Follow              = luna.transform;

        followAndFlip.enabled       = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);
        butterflyAnimator.speed     = animationSpeedNormal;

        // Jump off if requested
        if (jumpOff && _rbLuna != null)
            _rbLuna.velocity = new Vector2(_rbLuna.velocity.x, jumpForce);

        // Cooldown visuals
        butterflyRenderer.color = cooldownColor;
        _inCooldown            = true;
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

    IEnumerator ConsumeSporeVisual()
    {
        // let the spore attach briefly
        yield return new WaitForSeconds(0.3f);
        _spore.DestroyAttachedSpore();
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
            bonusSpeed       += speedBoost;
            teaRosePollenCount = 0;
        }
    }
}
