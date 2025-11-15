using UnityEngine;
using System.Collections;

public class GenericPlatformDip : MonoBehaviour
{
    [Header("Dip Settings")]
    public float dipAmount = 0.12f;
    public float dipSpeed = 8f;
    public float returnSpeed = 3f;
    public float holdTime = 0.15f;

    [Header("Player Tag")]
    public string playerTag = "Player";

    private Vector3 _originalLocalPos;
    private bool _isDipping;
    private int _playerContacts;
    private Coroutine _dipRoutine;
    private Transform _t;

    void Awake()
    {
        _t = transform;
        _originalLocalPos = _t.localPosition;
    }

    // ================================================================
    // UNIVERSAL DETECTION — WORKS FOR ANY COLLIDER OR TRIGGER ANYWHERE
    // ================================================================

    private void OnCollisionEnter2D(Collision2D c)
    {
        if (c.collider.CompareTag(playerTag))
        {
            Debug.Log("[Dip] Collision Enter with " + c.collider.name);
            _playerContacts++;
            StartDipIfNeeded();
        }
    }

    private void OnCollisionExit2D(Collision2D c)
    {
        if (c.collider.CompareTag(playerTag))
        {
            Debug.Log("[Dip] Collision Exit with " + c.collider.name);
            _playerContacts = Mathf.Max(0, _playerContacts - 1);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[Dip] Trigger Enter with " + other.name);
            _playerContacts++;
            StartDipIfNeeded();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            Debug.Log("[Dip] Trigger Exit with " + other.name);
            _playerContacts = Mathf.Max(0, _playerContacts - 1);
        }
    }

    // ================================================================
    void StartDipIfNeeded()
    {
        if (_isDipping) return;

        if (_dipRoutine != null)
            StopCoroutine(_dipRoutine);

        _dipRoutine = StartCoroutine(DipRoutine());
    }

    IEnumerator DipRoutine()
    {
        _isDipping = true;

        Vector3 start = _t.localPosition;
        Vector3 dipped = _originalLocalPos + Vector3.down * dipAmount;

        // DIP
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime * dipSpeed;
            _t.localPosition = Vector3.Lerp(start, dipped, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }
        _t.localPosition = dipped;

        // HOLD UNTIL PLAYER LEAVES
        float timer = 0f;
        while (_playerContacts > 0 || timer < holdTime)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        // RETURN
        t = 0f;
        Vector3 from = _t.localPosition;
        while (t < 1f)
        {
            t += Time.deltaTime * returnSpeed;
            _t.localPosition = Vector3.Lerp(from, _originalLocalPos, Mathf.SmoothStep(0f, 1f, t));
            yield return null;
        }

        _t.localPosition = _originalLocalPos;

        _isDipping = false;
        _dipRoutine = null;
    }
}
