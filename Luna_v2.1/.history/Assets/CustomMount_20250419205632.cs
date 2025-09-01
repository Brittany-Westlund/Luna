using UnityEngine;
using Cinemachine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;                         // Ground Luna GameObject (holds this script)
    public SpriteRenderer flightSprite;             // Flying Luna sprite renderer
    public Transform sporeHoldPoint;
    public Transform flightSporeAttachPoint;

    [HideInInspector]
    public bool justDismounted = false;              // Indicates Luna just dismounted

    [Header("Movement & Flight")]
    public float flightSpeed = 4f;
    public float flightDuration = 5f;
    public float cooldownDuration = 3f;

    [Header("Effects & Camera")]
    public CinemachineVirtualCamera flightCam;
    public CinemachineVirtualCamera groundCam;
    public GameObject sparklePrefab;

    private Rigidbody2D _rb;
    private LunaSporeSystem _spore;
    private bool _isFlying = false;
    private float _flightTimer = 0f;
    private float _cooldownTimer = 0f;

    private Transform _flightOriginalParent;

    void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spore = luna.GetComponent<LunaSporeSystem>();
        flightSprite.enabled = false;
        sparklePrefab.SetActive(false);
        _flightOriginalParent = flightSprite.transform.parent;
    }

    void Update()
    {
        // Toggle flight
        if (Input.GetKeyDown(KeyCode.F) && _cooldownTimer <= 0f)
        {
            if (_isFlying) EndFlight();
            else StartFlight();
            return;
        }

        // Cooldown
        if (_cooldownTimer > 0f)
        {
            _cooldownTimer -= Time.deltaTime;
            return;
        }

        if (_isFlying)
        {
            _flightTimer += Time.deltaTime;
            HandleFlightMovement();
            if (_flightTimer >= flightDuration) EndFlight();
            if (Input.GetKeyDown(KeyCode.R))
            {
                _spore.CreateSpore();
                StartCoroutine(AutoDestroySpore());
            }
        }
    }

    void StartFlight()
    {
        _isFlying = true;
        _flightTimer = 0f;
        justDismounted = false;

        // Show flight sprite
        luna.SetActive(false);
        flightSprite.enabled = true;
        flightSprite.transform.SetParent(transform, true);

        sparklePrefab.SetActive(true);
        _spore.attachPoint = flightSporeAttachPoint;

        _rb.gravityScale = 0f;
        _rb.velocity = Vector2.zero;

        flightCam.gameObject.SetActive(true);
        groundCam.gameObject.SetActive(false);
    }

    void EndFlight()
    {
        _isFlying = false;
        _cooldownTimer = cooldownDuration;
        justDismounted = true;

        // Position and swap sprite
        luna.transform.position = transform.position;
        flightSprite.transform.SetParent(_flightOriginalParent, true);
        flightSprite.enabled = false;
        luna.SetActive(true);

        sparklePrefab.SetActive(false);
        _spore.attachPoint = sporeHoldPoint;

        _rb.gravityScale = 1f;

        flightCam.gameObject.SetActive(false);
        groundCam.gameObject.SetActive(true);
    }

    void HandleFlightMovement()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector2 dir = new Vector2(h, v).normalized;
        _rb.velocity = dir * flightSpeed;

        // Flip flight sprite
        if (h > 0.1f) flightSprite.transform.localScale = Vector3.one;
        else if (h < -0.1f) flightSprite.transform.localScale = new Vector3(-1,1,1);
    }

    IEnumerator AutoDestroySpore()
    {
        yield return new WaitForSeconds(0.5f);
        _spore.DestroyAttachedSpore();
    }
}
