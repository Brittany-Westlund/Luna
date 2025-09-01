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

    // internals
    private Rigidbody2D  butterflyRb;
    private Collider2D   butterflyCollider;
    private Collider2D[] lunaColliders;

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

    // stores the true flight-facing direction
    private bool _isFacingRight;

    void Start()
    {
        butterflyRb       = butterfly.GetComponent<Rigidbody2D>();
        butterflyCollider = butterfly.GetComponent<Collider2D>();

        lunaRb            = luna.GetComponent<Rigidbody2D>();
        if (lunaRb != null) lunaInitialGravity = lunaRb.gravityScale;

        lunaGroundSprite = luna.GetComponent<SpriteRenderer>();
        sporeSystem      = luna.GetComponent<LunaSporeSystem>();
        flowerHolder     = luna.GetComponent<FlowerHolder>();
        lunaColliders    = luna.GetComponentsInChildren<Collider2D>();

        sporeSystem.attachPoint = sporeHoldPoint;

        lunaGroundSprite.enabled = true;
        lunaInFlight.SetActive(false);

        butterflyRenderer.color  = normalColor;
        butterflyRb.bodyType     = RigidbodyType2D.Kinematic;
        butterflyRb.gravityScale = 0f;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // initialize facing from current butterfly flip
        _isFacingRight = !butterflyRenderer.flipX;
    }

    void Update()
    {
        if (isInCooldown) return;

        // F = mount / extend / dismount
        if (Input.GetKeyDown(KeyCode.F))
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

        // in-flight controls
        float speed = baseSpeed + bonusSpeed + (hasTempBoost ? nextTempBoost : 0f);
        if (hasTempBoost)
        {
            hasTempBoost = false;
            teaRosePollenFX?.SetActive(false);
        }

        HandleMovement(speed);
        HandleFlip();
        HandleTimer();

        // R => create + immediate extend if eligible
        if (Input.GetKeyDown(KeyCode.R))
        {
            sporeSystem.CreateSpore();
            if (canBeExtended)
                ExtendFlightWithSpore();
        }

        // Space => jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // X => pick/plant mid-air
        if (Input.GetKeyDown(KeyCode.X))
            HandleAirFlowerInteract();
    }

    private void Mount()
    {
        justDismounted = false;

        // show flight sprite
        lunaGroundSprite.enabled = false;
        lunaInFlight.SetActive(true);

        // spore attaches to flight point
        sporeSystem.attachPoint = flightSporeAttachPoint;

        // ignore only butterfly-Luna collisions
        foreach (var c in lunaColliders)
            Physics2D.IgnoreCollision(butterflyCollider, c, true);

        // camera & FX
        vCam.Follow = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        // reset facing from current butterfly flip
        _isFacingRight = !butterflyRenderer.flipX;
        butterflyRenderer.flipX = !_isFacingRight;
        var ls = lunaInFlight.transform.localScale;
        ls.x = _isFacingRight ?  Mathf.Abs(ls.x) 
                              : -Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        // butterfly physics
        butterflyAnimator.speed   = animationSpeedFlying;
        butterflyRb.bodyType      = RigidbodyType2D.Dynamic;
        butterflyRb.gravityScale  = 0f;

        // reset flight timers/flags
        flightTimer      = 0f;
        warningTriggered = false;
        canBeExtended    = false;
        isFlying         = true;
    }

    private void Dismount(bool jumpOff)
    {
        // show ground sprite
        lunaInFlight.SetActive(false);
        lunaGroundSprite.enabled = true;

        // spore back to Luna hand
        sporeSystem.attachPoint = sporeHoldPoint;

        // **do not restore collisions yet**—wait until EndCooldown

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

        // apply jump impulse
        if (jumpOff && lunaRb != null)
            lunaRb.velocity = new Vector2(lunaRb.velocity.x, jumpForce);

        // set cooldown color immediately
        butterflyRenderer.color = cooldownColor;

        // begin cooldown before remount
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
            if (Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
            if (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)) hx =  1f;
        }
        butterfly.position += new Vector3(hx, hy, 0f) * speed * Time.deltaTime;
    }

    private void HandleFlip()
    {
        float hx = Input.GetAxis("Horizontal");
        if (Mathf.Abs(hx) < 0.01f)
        {
            if      (Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
            else if (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)) hx =  1f;
        }
        if (hx != 0f)
        {
            bool left = hx < 0f;
            _isFacingRight = !left;
            butterflyRenderer.flipX = left;
            var ls = lunaInFlight.transform.localScale;
            ls.x = left ? -Mathf.Abs(ls.x) : Mathf.Abs(ls.x);
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
        // restore collisions now that the cooldown is over
        foreach (var c in lunaColliders)
            Physics2D.IgnoreCollision(butterflyCollider, c, false);
        butterflyRenderer.color = normalColor;
    }

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
