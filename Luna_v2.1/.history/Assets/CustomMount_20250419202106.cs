using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                   
    public Transform butterfly;               
    public Transform lunaFlightAttachPoint;   // new: where Luna sits while flying
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

    [Header("Butterfly Colors")]
    public Color normalColor   = Color.white;
    public Color warningColor  = Color.yellow;
    public Color cooldownColor = Color.gray;

    [Header("Spore Attach Points")]
    public Transform sporeHoldPoint;
    public Transform flightSporeAttachPoint;

    [HideInInspector] public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    // internals
    private LunaSporeSystem _spore;
    private FlowerHolder    _holder;
    private Rigidbody2D     _rbLuna;
    private Transform       _lunaOriginalParent;

    private float  _flightTimer;
    private bool   _isFlying;
    private bool   _warningTriggered;
    private bool   _canExtend;
    private bool   _hasTempBoost;
    private float  _nextTempBoost;
    private bool   _inCooldown;

    private Vector3 _butterflyOrigScale;
    private bool    _isFacingRight = true;

    void Start()
    {
        // cache components
        _spore               = luna.GetComponent<LunaSporeSystem>();
        _holder              = luna.GetComponent<FlowerHolder>();
        _rbLuna              = luna.GetComponent<Rigidbody2D>();
        _lunaOriginalParent  = luna.transform.parent;

        // initial visuals
        sparklePrefab?.SetActive(false);
        groundState();

        // initial spore attach
        _spore.attachPoint = sporeHoldPoint;

        // scale caching
        _butterflyOrigScale = butterfly.localScale;
        _butterflyOrigScale.x = Mathf.Abs(_butterflyOrigScale.x);
    }

    void Update()
    {
        // F = mount / extend / dismount
        if (Input.GetKeyDown(KeyCode.F) && !_inCooldown)
        {
            if (_isFlying)
            {
                if (_spore.HasSporeAttached && _canExtend)
                    ExtendFlight();
                else
                    Dismount(false);
            }
            else Mount();
            return;
        }

        if (!_isFlying) return;

        // flying controls
        Vector2 dir = HandleMovement();
        HandleFacing(dir);
        HandleTimer();

        // R: always spawn + schedule destroy, extend only if allowed
        if (Input.GetKeyDown(KeyCode.R))
        {
            _spore.CreateSpore();
            StartCoroutine(AutoDestroySpore());
            if (_canExtend) ExtendFlight();
        }

        // Jump off
        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        // Midair flower
        if (Input.GetKeyDown(KeyCode.X))
            HandleAirInteract();
    }

    void Mount()
    {
        justDismounted    = false;
        _isFlying         = true;
        _canExtend        = false;
        _warningTriggered = false;
        _flightTimer      = 0f;

        // parent Luna to butterfly for flight
        luna.transform.SetParent(butterfly);
        luna.transform.localPosition = lunaFlightAttachPoint.localPosition;

        // disable ground colliders so Luna doesn't collide
        foreach (var c in lunaSolidColliders) c.enabled = false;

        // camera & FX
        vCam.Follow               = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled     = false;
        sparklePrefab?.SetActive(true);

        HandleFacing(Vector2.zero);
        butterflyAnimator.speed   = animationSpeedFlying;
        butterflyRenderer.color   = normalColor;

        // reattach spores to butterfly
        _spore.attachPoint = flightSporeAttachPoint;
    }

    void Dismount(bool jumpOff)
    {
        // unparent & place Luna next to butterfly
        luna.transform.SetParent(_lunaOriginalParent);
        luna.transform.position = butterfly.position + dismountOffset;

        justDismounted = true;
        _isFlying      = false;
        _inCooldown    = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        groundState();

        // camera & FX
        vCam.Follow           = luna.transform;
        followAndFlip.enabled = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);

        // reset facing on butterfly
        float s = _isFacingRight ? +1f : -1f;
        butterfly.localScale = new Vector3(
            _butterflyOrigScale.x * s,
            _butterflyOrigScale.y,
            _butterflyOrigScale.z
        );

        butterflyAnimator.speed   = animationSpeedNormal;
        butterflyRenderer.color   = cooldownColor;

        if (jumpOff && _rbLuna != null)
        {
            _rbLuna.velocity = new Vector2(_rbLuna.velocity.x, 0f);
            _rbLuna.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // detach spore back to ground
        _spore.attachPoint = sporeHoldPoint;
    }

    Vector2 HandleMovement()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))  dir.x = -1;
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) dir.x = +1;
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))    dir.y = +1;
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))  dir.y = -1;

        if (dir != Vector2.zero)
        {
            dir.Normalize();
            float speed = baseSpeed + bonusSpeed + (_hasTempBoost ? _nextTempBoost : 0f);
            if (_hasTempBoost)
            {
                _hasTempBoost = false;
                sparklePrefab?.SetActive(false);
            }
            butterfly.position += (Vector3)(dir * speed * Time.deltaTime);
        }
        return dir;
    }

    void HandleFacing(Vector2 dir)
    {
        if (dir.x > 0.01f)       _isFacingRight = true;
        else if (dir.x < -0.01f) _isFacingRight = false;

        float sign = _isFacingRight ? +1f : -1f;
        Vector3 scale = new Vector3(
            _butterflyOrigScale.x * sign,
            _butterflyOrigScale.y,
            _butterflyOrigScale.z
        );
        butterfly.localScale = scale;
        luna.transform.localScale = scale;
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
        _canExtend           = false;
        butterflyRenderer.color = normalColor;
    }

    IEnumerator AutoDestroySpore()
    {
        yield return new WaitForSeconds(0.5f);
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

    void groundState()
    {
        // enable ground colliders & sprite
        foreach (var c in butterflySolidColliders) c.enabled = true;
        foreach (var c in lunaSolidColliders)      c.enabled = true;
        // make sure Luna's sprite is visible on ground
        luna.GetComponent<SpriteRenderer>().enabled = true;
    }
}
