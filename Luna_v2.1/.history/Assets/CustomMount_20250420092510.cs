using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                     // ground‐state Luna
    public GameObject lunaInFlight;             // flight sprite, child of butterfly
    public Transform butterfly;
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;         // only on ground
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

    [Header("Butterfly Colors")]
    public Color normalColor   = Color.white;
    public Color warningColor  = Color.yellow;
    public Color cooldownColor = Color.gray;

    [Header("Spore Attach Points")]
    public Transform sporeHoldPoint;
    public Transform flightSporeAttachPoint;

    [Header("Tea Rose Pollen")]
    public GameObject teaRosePollenFX;
    public int   teaRosePollenCount   = 0;
    public float speedBoostPerPollen  = 1f;
    public int   pollenThreshold      = 3;

    [HideInInspector]
    public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    // internals
    private LunaSporeSystem _spore;
    private FlowerHolder    _holder;
    private Rigidbody2D     _rbLuna;
    private float           _lunaGravity;
    private SpriteRenderer  _sprLuna;

    private float  _flightTimer;
    private bool   _isFlying;
    private bool   _warningTriggered;
    private bool   _canExtend;
    private bool   _hasTempBoost;
    private float  _nextTempBoost;
    private bool   _inCooldown;

    private Vector3 _butterflyOrigScale;
    private Vector3 _lunaOrigScale;
    private bool    _isFacingRight = true;

    void Start()
    {
        // cache systems
        _spore    = luna.GetComponent<LunaSporeSystem>();
        _holder   = luna.GetComponent<FlowerHolder>();
        _rbLuna   = luna.GetComponent<Rigidbody2D>();
        if (_rbLuna != null) _lunaGravity = _rbLuna.gravityScale;
        _sprLuna  = luna.GetComponent<SpriteRenderer>();

        // initial spore attach
        _spore.attachPoint = sporeHoldPoint;

        // visuals
        _sprLuna.enabled       = true;
        lunaInFlight.SetActive(false);
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);

        // cache absolute original scales
        _butterflyOrigScale = butterfly.localScale;
        _butterflyOrigScale.x = Mathf.Abs(_butterflyOrigScale.x);
        _lunaOrigScale = lunaInFlight.transform.localScale;
        _lunaOrigScale.x     = Mathf.Abs(_lunaOrigScale.x);

        // start facing right
        _isFacingRight = true;
    }

    void Update()
    {
        // Mount / Dismount on F
        if (Input.GetKeyDown(KeyCode.F) && !_inCooldown)
        {
            if (_isFlying)
            {
                Dismount(false);
            }
            else
            {
                Mount();
            }
            return;
        }

        if (!_isFlying) return;

        // Flight controls
        Vector2 dir = HandleMovement();
        HandleFacing(dir);
        HandleTimer();

        // R: spawn + extend + destroy
        if (Input.GetKeyDown(KeyCode.R))
        {
            _spore.CreateSpore();
            ExtendFlight();
            StartCoroutine(ConsumeSpore());
        }

        // Jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // Mid‑air interact
        if (Input.GetKeyDown(KeyCode.X))
            HandleAirInteract();
    }

    Vector2 HandleMovement()
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
        return dir;
    }

    void HandleFacing(Vector2 dir)
    {
        // flip based on movement direction
        if (dir.x > 0.01f)       _isFacingRight = true;
        else if (dir.x < -0.01f) _isFacingRight = false;

        float sign = _isFacingRight ? +1f : -1f;
        butterfly.localScale = new Vector3(
            _butterflyOrigScale.x * sign,
            _butterflyOrigScale.y,
            _butterflyOrigScale.z
        );
    }

    void Mount()
    {
        justDismounted    = false;
        _isFlying         = true;
        _canExtend        = false;
        _warningTriggered = false;
        _flightTimer      = 0f;

        // visuals
        _sprLuna.enabled = false;
        lunaInFlight.SetActive(true);
        _spore.attachPoint = flightSporeAttachPoint;

        // disable colliders to stop bobbing
        foreach (var c in butterflySolidColliders) c.enabled = false;
        foreach (var c in lunaSolidColliders)      c.enabled = false;

        // camera & FX
        vCam.Follow               = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled     = false;
        sparklePrefab?.SetActive(true);

        // animation & tint
        butterflyAnimator.speed = animationSpeedFlying;
        butterflyRenderer.color = normalColor;
    }

    void Dismount(bool jumpOff)
    {
        // snap Luna to avoid bob
        luna.transform.position = butterfly.position + dismountOffset;

        _isFlying   = false;
        _inCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

       // **Enforce the correct facing** immediately on dismount:
        float sign = _isFacingRight ? +1f : -1f;
        butterfly.localScale = new Vector3(
        _butterflyOrigScale.x * sign,
        _butterflyOrigScale.y,
        _butterflyOrigScale.z);
       
        // visuals back
        lunaInFlight.SetActive(false);
        _sprLuna.enabled = true;

        // restore colliders
        foreach (var c in butterflySolidColliders) c.enabled = true;
        foreach (var c in lunaSolidColliders)      c.enabled = true;

        _spore.attachPoint = sporeHoldPoint;

        // camera & FX
        vCam.Follow               = luna.transform;
        followAndFlip.enabled     = true;
        // wait one frame before turning FollowAndFlip back on
        StartCoroutine(ReenableFollowNextFrame());
        sparklePrefab?.SetActive(false);

       IEnumerator ReenableFollowNextFrame()
        {
            yield return null;                   // wait one frame
            followAndFlip.enabled = true;
        }
       
        // reset animation & tint
        butterflyAnimator.speed  = animationSpeedNormal;
        butterflyRenderer.color  = cooldownColor;

        // jump off
        if (jumpOff && _rbLuna != null)
        {
            _rbLuna.velocity = Vector2.zero;
            _rbLuna.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
    }

    void HandleTimer()
    {
        _flightTimer += Time.deltaTime;
        if (!_warningTriggered && _flightTimer >= flightDuration - warningTime)
        {
            _warningTriggered = true;
            _canExtend       = true;
            StartCoroutine(FlashWarning());
        }
        if (_flightTimer >= flightDuration)
            Dismount(false);
    }

    void ExtendFlight()
    {
        _flightTimer = Mathf.Max(0f, _flightTimer - warningTime);
        _spore.DestroyAttachedSpore();
        _canExtend              = false;
        butterflyRenderer.color  = normalColor;
    }

    IEnumerator ConsumeSpore()
    {
        yield return new WaitForSeconds(0.3f);
        _spore.DestroyAttachedSpore();
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
        _inCooldown             = false;
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

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost          = true;
        _nextTempBoost         = speedBoostPerPollen;
        teaRosePollenFX?.SetActive(true);

        if (teaRosePollenCount >= pollenThreshold)
        {
            bonusSpeed          += speedBoostPerPollen;
            teaRosePollenCount  = 0;
        }
    }

    IEnumerator ReenableFollowNextFrame()
    {
        // wait until the next frame
        yield return null;
        followAndFlip.enabled = true;
    }

}
