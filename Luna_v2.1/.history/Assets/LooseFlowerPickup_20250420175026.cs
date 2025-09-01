using UnityEngine;
using System.Collections;

[RequireComponent(typeof(FlowerHolder))]
public class LooseFlowerPickup : MonoBehaviour
{
    [Tooltip("How far Luna or the butterfly can reach to pick up free sprouts.")]
    public float pickupRadius = 1f;

    [Tooltip("Seconds to override any ResetOnGrowth teleport.")]
    public float lockTime = 0.5f;

    private FlowerHolder             _holder;
    private Transform                _groundHoldPoint;
    private ButterflyFlyHandler      _flyHandler;
    private FlowerInteractionManager _gardenMgr;

    void Awake()
    {
        _holder          = GetComponent<FlowerHolder>();
        _groundHoldPoint = _holder.holdPoint;
        _flyHandler      = FindObjectOfType<ButterflyFlyHandler>();
        _gardenMgr       = GetComponent<FlowerInteractionManager>();
    }

    void Update()
    {
        bool isGround = (_holder.holdPoint == _groundHoldPoint);

        // toggle garden planting
        if (_gardenMgr != null)
            _gardenMgr.enabled = isGround;

        // only respond if X pressed and not already holding a flower
        if (!Input.GetKeyDown(KeyCode.X) || _holder.HasFlower)
            return;

        // choose search center
        Vector3 center = isGround
            ? transform.position
            : (_flyHandler?.butterfly.position ?? transform.position);

        // find sprouts
        var hits = Physics2D.OverlapCircleAll(center, pickupRadius);

        if (isGround)
        {
            // let garden script take priority
            foreach (var col in hits)
                if (col.CompareTag("Garden"))
                    return;
        }

        // pick up the first loose sprout
        foreach (var col in hits)
        {
            if (col.CompareTag("Sprout"))
            {
                Pickup(col.gameObject);
                return;
            }
        }
    }

    private void Pickup(GameObject go)
    {
        // record where we want it locked
        Vector3 desiredPos = go.transform.position;

        // hand off to FlowerHolder
        _holder.PickUpFlower(go);

        // parent under the hold‑point, preserving world transform
        var heldGo = _holder.GetHeldFlower();
        Transform heldT = heldGo.transform;
        heldT.SetParent(_holder.holdPoint, true);
        heldT.localPosition = Vector3.zero;

        // start a coroutine to lock its position for a bit
        StartCoroutine(LockPosition(heldT, desiredPos, lockTime));
    }

    private IEnumerator LockPosition(Transform t, Vector3 pos, float duration)
    {
        float end = Time.time + duration;
        while (Time.time < end && t != null)
        {
            t.position = pos;
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        bool isGround = (_holder != null && _holder.holdPoint == _groundHoldPoint);
        Vector3 center = isGround
            ? transform.position
            : (FindObjectOfType<ButterflyFlyHandler>()?.butterfly.position ?? transform.position);

        Gizmos.color = isGround ? Color.yellow : Color.cyan;
        Gizmos.DrawWireSphere(center, pickupRadius);
    }
}
