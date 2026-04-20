using UnityEngine;
using UnityEngine.Events;

public class ButterflyRideableNPC : MonoBehaviour
{
    [Header("Ride Visual Offset")]
    public Vector3 mountedLocalOffset = Vector3.zero;

    [Header("Facing")]
    public bool matchMountFacing = true;
    public bool invertFacing = false;

    [Header("Optional")]
    public bool disablePhysicsWhileMounted = true;

    [Header("Events")]
    public UnityEvent onMounted;
    public UnityEvent onDismounted;

    protected Transform _originalParent;
    protected Rigidbody2D _rb2D;
    protected Collider2D[] _colliders;

    protected bool _isMounted = false;
    protected Transform _currentMountPoint;

    protected SpriteRenderer _sprite;
    protected SpriteRenderer _mountSprite;

    protected virtual void Awake()
    {
        _originalParent = transform.parent;
        _rb2D = GetComponent<Rigidbody2D>();
        _colliders = GetComponentsInChildren<Collider2D>(true);

        _sprite = GetComponentInChildren<SpriteRenderer>();
    }

    public virtual bool IsMounted()
    {
        return _isMounted;
    }

    public virtual void MountTo(Transform mountPoint)
    {
        if (mountPoint == null)
        {
            Debug.LogWarning("ButterflyRideableNPC: MountTo called with null mountPoint.");
            return;
        }

        _currentMountPoint = mountPoint;
        _isMounted = true;

        if (disablePhysicsWhileMounted && _rb2D != null)
        {
            _rb2D.velocity = Vector2.zero;
            _rb2D.angularVelocity = 0f;
            _rb2D.simulated = false;
        }

        transform.SetParent(mountPoint);
        transform.localPosition = mountedLocalOffset;

        // 🔑 Find butterfly sprite for facing
        if (matchMountFacing)
        {
            _mountSprite = mountPoint.GetComponentInParent<SpriteRenderer>();
            UpdateFacing();
        }

        onMounted.Invoke();
    }

    public virtual void DismountTo(Vector3 worldPosition, Transform newParent = null)
    {
        _isMounted = false;
        _currentMountPoint = null;
        _mountSprite = null;

        transform.SetParent(newParent != null ? newParent : _originalParent);
        transform.position = worldPosition;

        if (disablePhysicsWhileMounted && _rb2D != null)
        {
            _rb2D.simulated = true;
            _rb2D.velocity = Vector2.zero;
            _rb2D.angularVelocity = 0f;
        }

        onDismounted.Invoke();
    }

    protected virtual void LateUpdate()
    {
        if (_isMounted && matchMountFacing)
        {
            UpdateFacing();
        }
    }

    protected virtual void UpdateFacing()
    {
        if (_sprite == null || _mountSprite == null)
        {
            return;
        }

        bool flip = _mountSprite.flipX;

        if (invertFacing)
        {
            flip = !flip;
        }

        _sprite.flipX = flip;
    }
}