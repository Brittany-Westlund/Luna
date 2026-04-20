using UnityEngine;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class FallingSkyHazard : MonoBehaviour
{
    [Header("Falling")]
    public float fallSpeed = 4f;
    public bool startFallingOnEnable = true;

    [Header("Reset Timing")]
    public float groundResetDelay = 0f;
    public float playerResetDelay = 0f;

    [Header("Damage")]
    public float damage = 1f;
    public float flickerDuration = 0.1f;
    public float invincibilityDuration = 0.5f;

    [Header("Targeting")]
    public string playerTag = "Player";
    public bool requirePlayerTag = true;

    [Header("Collision")]
    public bool damagePlayerOnHit = true;
    public LayerMask groundLayers;

    [Header("Audio")]
    public AudioClip playerHitSfx;
    public AudioClip groundHitSfx;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    protected Collider2D _collider;
    protected Rigidbody2D _rigidbody2D;
    protected Vector3 _startPosition;
    protected bool _isResetting = false;

    protected virtual void Awake()
    {
        _collider = GetComponent<Collider2D>();
        _collider.isTrigger = true;

        _rigidbody2D = GetComponent<Rigidbody2D>();
        _rigidbody2D.gravityScale = 0f;
        _rigidbody2D.bodyType = RigidbodyType2D.Kinematic;
        _rigidbody2D.simulated = true;
        _rigidbody2D.freezeRotation = true;

        _startPosition = transform.position;
    }

    protected virtual void OnEnable()
    {
        CancelInvoke(nameof(ResetToStart));

        if (startFallingOnEnable)
        {
            BeginFalling();
        }
    }

    public virtual void BeginFalling()
    {
        _isResetting = false;
        _rigidbody2D.velocity = Vector2.down * fallSpeed;
    }

    protected virtual void OnTriggerEnter2D(Collider2D other)
    {
        if (_isResetting)
        {
            return;
        }

        if (IsInLayerMask(other.gameObject.layer, groundLayers))
        {
            HandleGroundHit();
            return;
        }

        Health targetHealth = FindHealth(other);

        if (targetHealth != null && IsValidPlayerTarget(other))
        {
            HandlePlayerHit(targetHealth, other);
        }
    }

    protected virtual void HandleGroundHit()
    {
        _isResetting = true;

        PlaySfx(groundHitSfx);

        if (groundResetDelay <= 0f)
        {
            ResetToStart();
        }
        else
        {
            Invoke(nameof(ResetToStart), groundResetDelay);
        }
    }

    protected virtual void HandlePlayerHit(Health targetHealth, Collider2D other)
    {
        if (damagePlayerOnHit)
        {
            Vector3 damageDirection = (other.transform.position - transform.position).normalized;

            if (damageDirection == Vector3.zero)
            {
                damageDirection = Vector3.down;
            }

            targetHealth.Damage(
                damage,
                gameObject,
                flickerDuration,
                invincibilityDuration,
                damageDirection
            );
        }

        PlaySfx(playerHitSfx);

        _isResetting = true;

        if (playerResetDelay <= 0f)
        {
            ResetToStart();
        }
        else
        {
            Invoke(nameof(ResetToStart), playerResetDelay);
        }
    }

    protected virtual void ResetToStart()
    {
        transform.position = _startPosition;
        _rigidbody2D.velocity = Vector2.zero;
        _rigidbody2D.angularVelocity = 0f;
        _rigidbody2D.velocity = Vector2.down * fallSpeed;
        _isResetting = false;
    }

    protected virtual Health FindHealth(Collider2D other)
    {
        Health health = other.GetComponent<Health>();

        if (health == null)
        {
            health = other.GetComponentInParent<Health>();
        }

        return health;
    }

    protected virtual bool IsValidPlayerTarget(Collider2D other)
    {
        if (!requirePlayerTag)
        {
            return true;
        }

        if (other.CompareTag(playerTag))
        {
            return true;
        }

        if (other.transform.root.CompareTag(playerTag))
        {
            return true;
        }

        return false;
    }

    protected virtual bool IsInLayerMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    protected virtual void PlaySfx(AudioClip clip)
    {
        if (clip == null)
        {
            return;
        }

        AudioSource.PlayClipAtPoint(clip, transform.position, sfxVolume);
    }

    protected virtual void OnDisable()
    {
        CancelInvoke(nameof(ResetToStart));
    }
}