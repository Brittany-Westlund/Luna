using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                   // Luna root (has LunaSporeSystem)
    public GameObject lunaInFlight;           // Flight‐mode sprite under the butterfly
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
    public float bonusSpeed       = 0f;        // permanent boost
    public float jumpForce        = 5f;        // upward impulse
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
    public Transform sporeHoldPoint;          // child of Luna on ground
    public Transform flightSporeAttachPoint;  // child of butterfly in flight

    [Header("Tea Rose Pollen")]
    public GameObject teaRosePollenFX;
    public int   teaRosePollenCount   = 0;
    public float speedBoostPerPollen  = 1f;
    public int   pollenThreshold      = 3;

    // required by LunaSporeSystem
    [HideInInspector] public bool justDismounted = false;

    // internals
    private Rigidbody2D     butterflyRb;
    private Collider2D      butterflyCollider;
    private Collider2D[]    lunaColliders;

    private Rigidbody2D     lunaRb;
    private float           lunaInitialGravity;
    private SpriteRenderer  lunaGroundSprite;
    private LunaSporeSystem sporeSystem;
    private FlowerHolder    flowerHolder;

    private float flightTimer      = 0f;
    private bool  isFlying         = false;
    private bool  warningTriggered = false;
    private bool  canBeExtended    = false;
    private bool  hasTempBoost     = false;
    private float nextTempBoost    = 0f;
    private bool  isInCooldown     = false;

    // direction flag
    private bool _isFacingRight = true;

    void Start()
    {
        // cache physics
        butterflyRb       = butterfly.GetComponent<Rigidbody2D>();
        butterflyCollider = butterfly.GetComponent<Collider2D>();

        lunaRb = luna.GetComponent<Rigidbody2D>();
        if (lunaRb != null) lunaInitialGravity = lunaRb.gravityScale;

        // cache visuals & systems
        lunaGroundSprite = luna.GetComponent<SpriteRenderer>();
        sporeSystem      = luna.GetComponent<LunaSporeSystem>();
        flowerHolder     = luna.GetComponent<FlowerHolder>();
        lunaColliders    = luna.GetComponentsInChildren<Collider2D>();

        // initial spore attach
        sporeSystem.attachPoint = sporeHoldPoint;

        // initial state
        lunaGroundSprite.enabled = true;
        lunaInFlight.SetActive(false);
        butterflyRenderer.color  = normalColor;
        butterflyRb.bodyType     = RigidbodyType2D.Kinematic;
        butterflyRb.gravityScale = 0f;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // set initial facing
        _isFacingRight = !butterflyRenderer.flipX;
    }

    void Update()
    {
        // Mount/Dismount/Extend
        if (Input.GetKeyDown(KeyCode.F) && !isInCooldown)
        {
            if (isFlying)
            {
                if (sporeSystem.HasSporeAttached && canBeExtended)
                    ExtendFlightWithSpore();
                else
                    Dismount(false);
            }
            else Mount();
            return;
        }

        if (!isFlying) return;

        // In flight:
        float speed = baseSpeed + bonusSpeed + (hasTempBoost ? nextTempBoost : 0f);
        if (hasTempBoost)
        {
            hasTempBoost = false;
            teaRosePollenFX?.SetActive(false);
        }

        HandleMovement(speed);
        HandleFlip();
        HandleTimer();

        // R: create & immediately extend if eligible
        if (Input.GetKeyDown(KeyCode.R))
        {
            sporeSystem.CreateSpore();
            if (canBeExtended)
                ExtendFlightWithSpore();
        }

        // Space: jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // X: pick/plant mid‑air
        if (Input.GetKeyDown(KeyCode.X))
            HandleAirFlowerInteract();
    }

    private void Mount()
    {
        justDismounted = false;

        // visuals
        lunaGroundSprite.enabled = false;
        lunaInFlight.SetActive(true);

        // spore attach
        sporeSystem.attachPoint = flightSporeAttachPoint;

        // ignore only butterfly↔Luna collisions
        foreach (var c in lunaColliders)
            Physics2D.IgnoreCollision(butterflyCollider, c, true);

        // camera & FX
        vCam.Follow = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // apply stored facing
        butterflyRenderer.flipX = !_isFacingRight;
        var ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight
            ?  Mathf.Abs(ls.x)
            : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        // physics
        butterflyAnimator.speed   = animationSpeedFlying;
        butterflyRb.bodyType      = RigidbodyType2D.Dynamic;
        butterflyRb.gravityScale  = 0f;

        // reset state
        flightTimer      = 0f;
        warningTriggered = false;
        canBeExtended    = false;
        isFlying         = true;
    }

    private void Dismount(bool jumpOff)
    {
        // visuals
        lunaInFlight.SetActive(false);
        lunaGroundSprite.enabled = true;

        // spore attach back
        sporeSystem.attachPoint = sporeHoldPoint;

        // restore collisions
        foreach (var c in lunaColliders)
            Physics2D.IgnoreCollision(butterflyCollider, c, false);

        // restore Luna physics
        if (lunaRb != null)
        {
            lunaRb.velocity     = Vector2.zero;
            lunaRb.gravityScale = lunaInitialGravity;
        }

        // reposition & camera
        luna.transform.position = butterfly.position + dismountOffset;
        vCam.Follow = luna.transform;
        followAndFlip.enabled = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);
        butterflyAnimator.speed = animationSpeedNormal;

        // jump impulse
        if (jumpOff && lunaRb != null)
            lunaRb.velocity = new Vector2(lunaRb.velocity.x, jumpForce);

        // cooldown
        isInCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        isFlying = false;
    }

    private void HandleMovement(float speed)
    {
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");
        if (Mathf.Abs(hx) < 0.01f)
        {
            if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) hx =  1f;
        }
        butterfly.position += new Vector3(hx, hy, 0f) * speed * Time.deltaTime;
    }

    private void HandleFlip()
    {
        float hx = Input.GetAxis("Horizontal");
        if (Mathf.Abs(hx) < 0.01f)
        {
            if      (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
            else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) hx =  1f;
        }
        if (hx != 0f)
        {
            bool facingLeft = hx < 0f;
            _isFacingRight = !facingLeft;
            butterflyRenderer.flipX = facingLeft;
            var ls = lunaInFlight.transform.localScale;
            ls.x = facingLeft
                ? -Mathf.Abs(ls.x)
                :  Mathf.Abs(ls.x);
            lunaInFlight.transform.localScale = ls;
        }
    }

    private void HandleTimer()
    {
        flightTimer += Time.deltaTime;
        if (!warningTriggered && flightTimer >= flightDuration - warningTime)
        {
            warningTriggered = true;
            canBeExtended    = true;
            StartCoroutine(FlashWarningColor());
        }
        if (flightTimer >= flightDuration)
            Dismount(false);
    }

    private void ExtendFlightWithSpore()
    {
        flightTimer = Mathf.Max(0f, flightTimer - warningTime);
        sporeSystem.DestroyAttachedSpore();
        canBeExtended = false;
        butterflyRenderer.color = normalColor;
    }

    private void HandleAirFlowerInteract()
    {
        var hits = Physics2D.OverlapCircleAll(butterfly.position, 1f);
        foreach (var col in hits)
        {
            if (col.CompareTag("Flower") && !flowerHolder.HasFlower)
            {
                flowerHolder.PickUpFlower(col.gameObject);
                return;
            }
            var spot = col.GetComponent<GardenSpot>();
            if (spot != null && flowerHolder.HasFlower)
            {
                var f = flowerHolder.GetHeldFlower();
                f.transform.SetParent(spot.transform, false);
                f.transform.localPosition = Vector3.zero;
                flowerHolder.DropFlower();
                var spr = f.GetComponent<SproutAndLightManager>();
                if (spr != null) spr.isPlanted = true;
                return;
            }
        }
    }

    private IEnumerator FlashWarningColor()
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
        isInCooldown = false;
        butterflyRenderer.color = normalColor;
    }

    /// <summary>
    /// Called by TeaRosePollenPickup to apply pollen boosts.
    /// </summary>
    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        hasTempBoost   = true;
        nextTempBoost  = speedBoost;
        teaRosePollenFX?.SetActive(true);

        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed       += speedBoost;
            teaRosePollenCount = 0;
        }
    }
}
