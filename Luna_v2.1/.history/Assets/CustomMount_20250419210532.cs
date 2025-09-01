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

    // internals
    Rigidbody2D   _butterflyRb;
    Collider2D    _butterflyCol;
    Collider2D[]  _lunaCols;
    Rigidbody2D   _lunaRb;
    float         _lunaGravity;
    SpriteRenderer _lunaSprite;
    LunaSporeSystem _spore;
    FlowerHolder    _holder;

    float flightTimer;
    bool  isFlying;
    bool  warningTriggered;
    bool  canBeExtended;
    bool  hasTempBoost;
    float nextTempBoost;
    bool  inCooldown;

    // last-pressed facing direction
    bool _isFacingRight = true;

    void Start()
    {
        _butterflyRb  = butterfly.GetComponent<Rigidbody2D>();
        _butterflyCol = butterfly.GetComponent<Collider2D>();

        _lunaRb = luna.GetComponent<Rigidbody2D>();
        if (_lunaRb != null) _lunaGravity = _lunaRb.gravityScale;

        _lunaSprite = luna.GetComponent<SpriteRenderer>();
        _spore      = luna.GetComponent<LunaSporeSystem>();
        _holder     = luna.GetComponent<FlowerHolder>();
        _lunaCols   = luna.GetComponentsInChildren<Collider2D>();

        // ground attach by default
        _spore.attachPoint = sporeHoldPoint;

        _lunaSprite.enabled    = true;
        lunaInFlight.SetActive(false);
        butterflyRenderer.color = normalColor;
        _butterflyRb.bodyType   = RigidbodyType2D.Kinematic;
        _butterflyRb.gravityScale = 0f;

        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        _isFacingRight = !butterflyRenderer.flipX;
    }

    void Update()
    {
        // 1) Mount / Extend / Dismount on F
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (!inCooldown)
            {
                if (isFlying)
                {
                    if (_spore.HasSporeAttached && canBeExtended) ExtendFlight();
                    else                                           Dismount(false);
                }
                else
                {
                    Mount();
                }
            }
            return;
        }

        // 2) If flying, handle all movement & in‑flight inputs
        if (isFlying)
        {
            // Movement
            HandleMovement();

            // Facing override
            HandleFacing();

            // Timer + warning
            HandleTimer();

            // R: spawn + immediate extend
            if (Input.GetKeyDown(KeyCode.R))
            {
                _spore.CreateSpore();
                if (canBeExtended) ExtendFlight();
            }

            // Space: jump off
            if (Input.GetButtonDown("Jump"))
                Dismount(true);

            // X: mid‑air pick/plant
            if (Input.GetKeyDown(KeyCode.X))
                HandleAirInteract();
        }
    }

    void HandleMovement()
    {
        float hx = 0f, hy = 0f;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) hx = 1f;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    hy = 1f;
        else if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) hy = -1f;

        Vector3 delta = new Vector3(hx, hy, 0f);
        if (delta.sqrMagnitude > 0f)
        {
            delta.Normalize();
            float speed = baseSpeed + bonusSpeed + (hasTempBoost ? nextTempBoost : 0f);
            if (hasTempBoost)
            {
                hasTempBoost = false;
                teaRosePollenFX?.SetActive(false);
            }
            butterfly.position += delta * speed * Time.deltaTime;
        }
    }

    void HandleFacing()
    {
        // explicit A/D overrides
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _isFacingRight = true;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _isFacingRight = false;

        // apply flip
        butterflyRenderer.flipX = !_isFacingRight;
        Vector3 ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;
    }

    void Mount()
    {
        justDismounted = false;

        // visuals
        _lunaSprite.enabled    = false;
        lunaInFlight.SetActive(true);

        // attach point
        _spore.attachPoint = flightSporeAttachPoint;

        // ignore butterfly↔Luna collisions
        foreach (var c in _lunaCols)
            Physics2D.IgnoreCollision(_butterflyCol, c, true);

        // camera & FX
        vCam.Follow               = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled     = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // enforce input‑tracked facing
        butterflyRenderer.flipX = !_isFacingRight;
        Vector3 ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ?  Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        // physics
        butterflyAnimator.speed   = animationSpeedFlying;
        _butterflyRb.bodyType     = RigidbodyType2D.Dynamic;
        _butterflyRb.gravityScale = 0f;

        // reset state
        flightTimer      = 0f;
        warningTriggered = false;
        canBeExtended    = false;
        isFlying         = true;
    }

    void Dismount(bool jumpOff)
    {
        // visuals
        lunaInFlight.SetActive(false);
        _lunaSprite.enabled = true;

        // spore back to Luna
        _spore.attachPoint = sporeHoldPoint;

        // restore collisions immediately
        foreach (var c in _lunaCols)
            Physics2D.IgnoreCollision(_butterflyCol, c, false);

        // restore Luna physics
        if (_lunaRb != null)
        {
            _lunaRb.velocity     = Vector2.zero;
            _lunaRb.gravityScale = _lunaGravity;
        }

        // reposition & camera
        luna.transform.position    = butterfly.position + dismountOffset;
        vCam.Follow                = luna.transform;
        followAndFlip.enabled      = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);
        butterflyAnimator.speed    = animationSpeedNormal;

        // jump impulse
        if (jumpOff && _lunaRb != null)
            _lunaRb.velocity = new Vector2(_lunaRb.velocity.x, jumpForce);

        // cooldown color
        butterflyRenderer.color = cooldownColor;
        inCooldown            = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        isFlying = false;
    }

    void HandleTimer()
    {
        flightTimer += Time.deltaTime;
        if (!warningTriggered && flightTimer >= flightDuration - warningTime)
        {
            warningTriggered = true;
            canBeExtended    = true;
            StartCoroutine(FlashWarning());
        }
        if (flightTimer >= flightDuration)
            Dismount(false);
    }

    void ExtendFlight()
    {
        flightTimer = Mathf.Max(0f, flightTimer - warningTime);
        _spore.DestroyAttachedSpore();
        canBeExtended = false;
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
        inCooldown = false;
        butterflyRenderer.color = normalColor;
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        hasTempBoost  = true;
        nextTempBoost = speedBoost;
        teaRosePollenFX?.SetActive(true);

        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed       += speedBoost;
            teaRosePollenCount = 0;
        }
    }
}
