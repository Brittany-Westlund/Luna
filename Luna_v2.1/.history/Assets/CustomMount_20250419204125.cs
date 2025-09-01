using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                        // Ground Luna GameObject
    public GameObject lunaInFlight;                // Separate flying Luna sprite
    public Transform butterfly;                    // Butterfly mount
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;
    public GameObject butterflySpeechBubble;
    public GameObject sparklePrefab;

    [Header("Luna SpriteRenderer")]
    public SpriteRenderer groundLunaRenderer;      // Renderer on luna
    public SpriteRenderer flightLunaRenderer;      // Renderer on lunaInFlight

    [Header("Flower Hold Points")]
    public Transform flowerHoldPoint;              // Where ground flower attaches
    public Transform flightFlowerHoldPoint;        // Where flower attaches in flight

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

    [HideInInspector] public bool justDismounted = false;

    [Header("Solid Colliders to Disable in Flight")]
    public Collider2D[] butterflySolidColliders;
    public Collider2D[] lunaSolidColliders;

    // Internals
    private LunaSporeSystem _spore;
    private FlowerHolder    _holder;
    private Rigidbody2D     _rbLuna;

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
        // Cache components
        _spore  = luna.GetComponent<LunaSporeSystem>();
        _holder = luna.GetComponent<FlowerHolder>();
        _rbLuna = luna.GetComponent<Rigidbody2D>();

        // Ensure both GameObjects active, use renderers for visibility
        luna.SetActive(true);
        lunaInFlight.SetActive(true);
        groundLunaRenderer.enabled = true;
        flightLunaRenderer.enabled = false;
        sparklePrefab?.SetActive(false);

        // Assign initial hold points
        _spore.attachPoint = sporeHoldPoint;
        _holder.holdPoint  = flowerHoldPoint;

        // Cache absolute x-scale
        _butterflyOrigScale   = butterfly.localScale;
        _butterflyOrigScale.x = Mathf.Abs(_butterflyOrigScale.x);
    }

    void Update()
    {
        // Toggle flight on F
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

        // Flight controls
        Vector2 dir = HandleMovement();
        HandleFacing(dir);
        HandleTimer();

        // Spore create/extend
        if (Input.GetKeyDown(KeyCode.R))
        {
            _spore.CreateSpore();
            StartCoroutine(AutoDestroySpore());
            if (_canExtend) ExtendFlight();
        }
        if (Input.GetButtonDown("Jump")) Dismount(true);
        if (Input.GetKeyDown(KeyCode.X)) HandleAirInteract();
    }

    void Mount()
    {
        justDismounted    = false;
        _isFlying         = true;
        _canExtend        = false;
        _warningTriggered = false;
        _flightTimer      = 0f;

        // Swap renderers
        groundLunaRenderer.enabled = false;
        flightLunaRenderer.enabled = true;
        // bring flight in front
        flightLunaRenderer.sortingLayerID = groundLunaRenderer.sortingLayerID;
        flightLunaRenderer.sortingOrder   = groundLunaRenderer.sortingOrder + 1;

        // Colliders off
        foreach (var c in butterflySolidColliders) c.enabled = false;
        foreach (var c in lunaSolidColliders)      c.enabled = false;

        // Camera & FX
        vCam.Follow               = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled     = false;
        sparklePrefab?.SetActive(true);

        // Flip initial
        HandleFacing(Vector2.zero);
        butterflyAnimator.speed = animationSpeedFlying;
        butterflyRenderer.color = normalColor;

        // Update attach points
        _spore.attachPoint  = flightSporeAttachPoint;
        _holder.holdPoint   = flightFlowerHoldPoint;
    }

    void Dismount(bool jumpOff)
    {
        // Swap renderers back
        flightLunaRenderer.enabled = false;
        groundLunaRenderer.enabled = true;

        // Position Luna
        luna.transform.position = butterfly.position + dismountOffset;
        justDismounted = true;
        _isFlying      = false;
        _inCooldown    = true;
        Invoke(nameof(EndCooldown), cooldownDuration);

        // Restore colliders
        foreach (var c in butterflySolidColliders) c.enabled = true;
        foreach (var c in lunaSolidColliders)      c.enabled = true;

        // Camera & FX
        vCam.Follow           = luna.transform;
        followAndFlip.enabled = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);

        // Maintain facing
        float s = _isFacingRight ? +1f : -1f;
        butterfly.localScale = new Vector3(
            _butterflyOrigScale.x * s,
            _butterflyOrigScale.y,
            _butterflyOrigScale.z
        );
        butterflyAnimator.speed = animationSpeedNormal;
        butterflyRenderer.color = cooldownColor;

        if (jumpOff && _rbLuna != null)
        {
            _rbLuna.velocity = new Vector2(_rbLuna.velocity.x, 0f);
            _rbLuna.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        // Revert attach points
        _spore.attachPoint  = sporeHoldPoint;
        _holder.holdPoint   = flowerHoldPoint;
    }

    Vector2 HandleMovement()
    {
        Vector2 dir = Vector2.zero;
        if (Input.GetKey(KeyCode.A)) dir.x = -1;
        if (Input.GetKey(KeyCode.D)) dir.x = 1;
        if (Input.GetKey(KeyCode.W)) dir.y = 1;
        if (Input.GetKey(KeyCode.S)) dir.y = -1;
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
        if (dir.x > 0) _isFacingRight = true;
        else if (dir.x < 0) _isFacingRight = false;
        float sign = _isFacingRight ? 1f : -1f;
        Vector3 scale = new Vector3(_butterflyOrigScale.x * sign,
                                    _butterflyOrigScale.y,
                                    _butterflyOrigScale.z);
        butterfly.localScale = scale;
        if (_isFlying)
            flightLunaRenderer.transform.localScale = scale;
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
        if (_flightTimer >= flightDuration) Dismount(false);
    }

    void ExtendFlight()
    {
        _flightTimer = Mathf.Max(0f, _flightTimer - warningTime);
        _spore.DestroyAttachedSpore();
        _canExtend          = false;
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

    // Pollen API
    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        _hasTempBoost  = true;
        _nextTempBoost = speedBoost;
        teaRosePollenFX?.SetActive(true);
        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed         += speedBoost;
            teaRosePollenCount = 0;
        }
    }
}
