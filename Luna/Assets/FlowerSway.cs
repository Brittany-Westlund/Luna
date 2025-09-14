using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class FlowerSway : MonoBehaviour
{
    public float swayAmount = 5f;
    public float swaySpeed = 2f;
    public Transform pivotPoint;

    private GameObject player;
    private CorgiController playerController;
    private float currentSwayAngle = 0f;
    private Coroutine swayCoroutine;
    private const float angleTolerance = 0.01f;

    public bool isBeingPickedUp = false;

    private Collider2D myTrigger;

    void Awake()
    {
        if (pivotPoint == null) pivotPoint = transform;
        myTrigger = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();
    }

    void Start()
    {
        TryRebindPlayer();
        KickIfOverlapping();
    }

    void OnEnable()
    {
        TryRebindPlayer();
        KickIfOverlapping();
    }

    void Update()
    {
        if (playerController == null || player == null || !player.activeInHierarchy)
            TryRebindPlayer();
    }

    private void TryRebindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player ? player.GetComponentInChildren<CorgiController>() : null;
    }

    private void KickIfOverlapping()
    {
        if (!isActiveAndEnabled || isBeingPickedUp || myTrigger == null || player == null) return;

        var playerCols = player.GetComponentsInChildren<Collider2D>(true);
        foreach (var pc in playerCols)
        {
            if (pc && myTrigger.IsTouching(pc))
            {
                StartSwayCoroutine(playerController && playerController.Speed.magnitude > 0.1f);
                return;
            }
        }
    }

    private IEnumerator UpdateSway(bool isPlayerMoving)
    {
        while (!isBeingPickedUp)
        {
            float moving = (playerController != null && playerController.Speed.magnitude > 0.1f) ? 1f : 0f;
            float targetSwayAngle = (isPlayerMoving || moving > 0f)
                ? Mathf.Sin(Time.time * swaySpeed) * swayAmount
                : 0f;

            currentSwayAngle = Mathf.Lerp(currentSwayAngle, targetSwayAngle, Time.deltaTime * swaySpeed);
            float angleDifference = currentSwayAngle - transform.localEulerAngles.z;
            transform.RotateAround(pivotPoint.position, Vector3.forward, angleDifference);

            if (Mathf.Abs(currentSwayAngle - targetSwayAngle) < angleTolerance)
                break;

            yield return null;
        }

        swayCoroutine = null;
    }

    private void StartSwayCoroutine(bool isPlayerMoving)
    {
        if (!isActiveAndEnabled || isBeingPickedUp) return;

        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
            swayCoroutine = null;
        }

        swayCoroutine = StartCoroutine(UpdateSway(isPlayerMoving));
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!isBeingPickedUp && other.GetComponentInParent<CorgiController>() != null)
        {
            StartSwayCoroutine(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (!isBeingPickedUp && other.CompareTag("Player"))
        {
            StartSwayCoroutine(false);
        }
    }

    public void DisableSwayOnPickup()
    {
        isBeingPickedUp = true;

        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
            swayCoroutine = null;
        }

        enabled = false;
    }

    // Optional external kick point (not required by GardenStickySlot now)
    public void ReactivateAfterReattach(bool assumePlayerMoving)
    {
        isBeingPickedUp = false;
        enabled = true;
        TryRebindPlayer();
        if (swayCoroutine == null)
            StartSwayCoroutine(assumePlayerMoving);
    }
}
