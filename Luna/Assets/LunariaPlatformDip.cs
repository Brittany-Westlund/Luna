using UnityEngine;
using System.Collections;

/// <summary>
/// Makes a platform dip slightly when the Player stands on it,
/// then return after a short hold. Works with both collisions and triggers.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class LunariaPlatformDip : MonoBehaviour
{
    [Header("Dip Settings")]
    [Tooltip("How far the visual moves downward (local Y).")]
    public float dipAmount = 0.12f;

    [Tooltip("How quickly it moves down (higher = faster).")]
    public float dipSpeed = 8f;

    [Tooltip("How quickly it returns up (higher = faster).")]
    public float returnSpeed = 3f;

    [Tooltip("How long to hold at the dipped position before rising.")]
    public float holdTime = 0.15f;

    [Header("Target To Move")]
    [Tooltip("Assign a child transform to move visuals only. If left empty, moves this transform.")]
    public Transform visual;

    [Header("Player Detection")]
    public string playerTag = "Player";

    // State
    private Vector3 _originalLocalPos;
    private bool _isDipping;
    private int _playerContacts;           // track enter/exit balance
    private Coroutine _dipRoutine;
    private Collider2D _col;

    void Awake()
    {
        _col = GetComponent<Collider2D>();

        if (visual == null) visual = transform;
        _originalLocalPos = visual.localPosition;

        // Warn if no Rigidbody2D on player will be present (collisions need one on either side)
        // We can’t check player here, but we can warn if *we* have a Rigidbody2D dynamic (not recommended).
        var rb = GetComponent<Rigidbody2D>();
        if (rb != null && rb.bodyType == RigidbodyType2D.Dynamic)
        {
            Debug.LogWarning("[LunariaPlatformDip] This platform has a Dynamic Rigidbody2D. " +
                             "Consider Static (collider only) or Kinematic if you want to move it yourself.");
        }
    }

    // --- Collision path (non-trigger collider) ---
    void OnCollisionEnter2D(Collision2D c)
    {
        if (!_col || _col.isTrigger) return;
        if (!c.collider.CompareTag(playerTag)) return;

        _playerContacts++;
        TryStartDip();
    }

    void OnCollisionExit2D(Collision2D c)
    {
        if (!_col || _col.isTrigger) return;
        if (!c.collider.CompareTag(playerTag)) return;

        _playerContacts = Mathf.Max(0, _playerContacts - 1);
        TryStopDipIfNoContacts();
    }

    // Optional: keep alive if staying (helps with one-frame bounces)
    void OnCollisionStay2D(Collision2D c)
    {
        if (!_col || _col.isTrigger) return;
        if (!c.collider.CompareTag(playerTag)) return;

        // Ensure we counted contact
        if (_playerContacts <= 0) _playerContacts = 1;
    }

    // --- Trigger path (trigger collider) ---
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!_col || !_col.isTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        _playerContacts++;
        TryStartDip();
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!_col || !_col.isTrigger) return;
        if (!other.CompareTag(playerTag)) return;

        _playerContacts = Mathf.Max(0, _playerContacts - 1);
        TryStopDipIfNoContacts();
    }

    void TryStartDip()
    {
        if (_isDipping) return;
        if (_dipRoutine != null) StopCoroutine(_dipRoutine);
        _dipRoutine = StartCoroutine(DipRoutine());
    }

    void TryStopDipIfNoContacts()
    {
        // We let the coroutine handle returning once no one is standing.
        // Nothing to do here except maintain _playerContacts.
    }

    private IEnumerator DipRoutine()
    {
        _isDipping = true;

        // Move down
        Vector3 start = visual.localPosition;
        Vector3 dipped = _originalLocalPos + new Vector3(0f, -dipAmount, 0f);
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * dipSpeed;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            visual.localPosition = Vector3.Lerp(start, dipped, eased);
            yield return null;
        }
        visual.localPosition = dipped;

        // Hold while player is on; ensure at least holdTime
        float timer = 0f;
        while (timer < holdTime || _playerContacts > 0)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // Return up
        t = 0f;
        Vector3 from = visual.localPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            float eased = Mathf.SmoothStep(0f, 1f, t);
            visual.localPosition = Vector3.Lerp(from, _originalLocalPos, eased);
            yield return null;
        }
        visual.localPosition = _originalLocalPos;

        _isDipping = false;
        _dipRoutine = null;
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Transform v = visual != null ? visual : transform;
        Vector3 basePos = Application.isPlaying ? _originalLocalPos : (v != null ? v.localPosition : Vector3.zero);
        Vector3 worldStart = (v != null ? v.parent : transform).TransformPoint(basePos);
        Vector3 worldEnd = (v != null ? v.parent : transform).TransformPoint(basePos + new Vector3(0f, -dipAmount, 0f));

        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(worldStart, worldEnd);
        Gizmos.DrawSphere(worldStart, 0.02f);
        Gizmos.DrawSphere(worldEnd, 0.02f);
    }
#endif
}
