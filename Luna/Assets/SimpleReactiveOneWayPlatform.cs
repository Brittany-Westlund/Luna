using System.Collections;
using UnityEngine;

public class SimpleReactiveOneWayPlatform : MonoBehaviour
{
    [Header("Points")]
    public Transform topPoint;
    public Transform bottomPoint;

    [Header("Timing")]
    public float delayBeforeMove = 1.5f;
    public float moveDuration = 2f;

    [Header("Trigger")]
    public string playerTag = "Player";
    public bool parentRiderToPlatform = true;

    private bool _isMoving = false;
    private bool _isAtBottom = false;
    private bool _isArmed = true;

    private Transform _currentRider;

    private void Start()
    {
        if (topPoint != null)
            transform.position = topPoint.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!_isArmed) return;
        if (_isMoving) return;
        if (_isAtBottom) return;
        if (!other.CompareTag(playerTag)) return;

        _currentRider = other.transform;

        if (parentRiderToPlatform)
            _currentRider.SetParent(transform);

        _isArmed = false;
        StartCoroutine(MovePlatformDown());
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (_currentRider != null && other.transform == _currentRider)
        {
            if (parentRiderToPlatform)
                _currentRider.SetParent(null);

            _currentRider = null;

            if (_isAtBottom && !_isMoving)
                ResetPlatform();
        }
    }

    private IEnumerator MovePlatformDown()
    {
        _isMoving = true;

        yield return new WaitForSeconds(delayBeforeMove);

        Vector3 startPos = topPoint.position;
        Vector3 endPos = bottomPoint.position;

        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;

        _isMoving = false;
        _isAtBottom = true;
    }

    private void ResetPlatform()
    {
        transform.position = topPoint.position;
        _isAtBottom = false;
        _isArmed = true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (topPoint && bottomPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(topPoint.position, 0.1f);

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(bottomPoint.position, 0.1f);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(topPoint.position, bottomPoint.position);
        }
    }
#endif
}
