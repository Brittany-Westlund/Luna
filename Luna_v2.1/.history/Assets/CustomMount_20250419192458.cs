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

    // track direction purely by input
    bool _isFacingRight = true;

    void Start()
    {
        _butterflyRb  = butterfly.GetComponent<Rigidbody2D>();
        _butterflyCol = butterfly.GetComponent<Collider2D>();
        _lunaRb       = luna.GetComponent<Rigidbody2D>();
        if (_lunaRb != null) _lunaGravity = _lunaRb.gravityScale;

        _lunaSprite = luna.GetComponent<SpriteRenderer>();
        _spore      = luna.GetComponent<LunaSporeSystem>();
        _holder     = luna.GetComponent<FlowerHolder>();
        _lunaCols   = luna.GetComponentsInChildren<Collider2D>();

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
        if (inCooldown) return;

        // F = mount/extend/dismount
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isFlying)
            {
                if (_spore.HasSporeAttached && canBeExtended)
                    ExtendFlight();
                else
                    Dismount(false);
            }
            else Mount();
            return;
        }

        if (!isFlying) return;

        // flying controls
        float speed = baseSpeed + bonusSpeed + (hasTempBoost ? nextTempBoost : 0f);
        if (hasTempBoost)
        {
            hasTempBoost = false;
            teaRosePollenFX?.SetActive(false);
        }

        HandleMovement(speed);
        HandleFlip();     // now with explicit A/D overrides
        HandleTimer();

        // R = create + maybe extend
        if (Input.GetKeyDown(KeyCode.R))
        {
            _spore.CreateSpore();
            if (canBeExtended) ExtendFlight();
        }

        // Space = jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // X = mid‑air pick/plant
        if (Input.GetKeyDown(KeyCode.X))
            HandleAirInteract();
    }

    void Mount()
    {
        justDismounted = false;

        _lunaSprite.enabled    = false;
        lunaInFlight.SetActive(true);
        _spore.attachPoint     = flightSporeAttachPoint;
        foreach (var c in _lunaCols)
            Physics2D.IgnoreCollision(_butterflyCol, c, true);

        vCam.Follow = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // force facing to current input‑tracked direction
        butterflyRenderer.flipX = !_isFacingRight;
        var ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ?  Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        butterflyAnimator.speed   = animationSpeedFlying;
        _butterflyRb.bodyType     = RigidbodyType2D.Dynamic;
        _butterflyRb.gravityScale = 0f;

        flightTimer      = 0f;
        warningTriggered = false;
        canBeExtended    = false;
        isFlying         = true;
    }

    void Dismount(bool jumpOff)
    {
        lunaInFlight.SetActive(false);
        _lunaSprite.enabled    = true;
        _spore.attachPoint     = sporeHoldPoint;
        foreach (var c in _lunaCols)
            Physics2D.IgnoreCollision(_butterflyCol, c, false);

        if (_lunaRb != null)
        {
            _lunaRb.velocity     = Vector2.zero;
            _lunaRb.gravityScale = _lunaGravity;
        }

        luna.transform.position = butterfly.position + dismountOffset;
        vCam.Follow = luna.transform;
        followAndFlip.enabled       = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);
        butterflyAnimator.speed     = animationSpeedNormal;

        if (jumpOff && _lunaRb != null)
            _lunaRb.velocity = new Vector2(_lunaRb.velocity.x, jumpForce);

        butterflyRenderer.color = cooldownColor;
        inCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        isFlying = false;
    }

    void HandleMovement(float speed)
    {
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");
        if (Mathf.Abs(hx) < 0.01f)
        {
            if (Input.GetKey(KeyCode.A)) hx = -1f;
            if (Input.GetKey(KeyCode.D)) hx =  1f;
        }
        butterfly.position += new Vector3(hx, hy, 0f) * speed * Time.deltaTime;
    }

    void HandleFlip()
    {
        // explicit overrides
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            _isFacingRight = true;
        else if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            _isFacingRight = false;
        // apply flip
        butterflyRenderer.flipX = !_isFacingRight;
        var ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ? Mathf.Abs(ls.x) : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;
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
