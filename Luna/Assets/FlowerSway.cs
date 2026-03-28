using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(Collider2D))]
public class FlowerSway : MonoBehaviour
{
    [Header("Sway")]
    public float swayAmount = 5f;
    public float swaySpeed = 2f;
    public Transform pivotPoint;
    public Transform[] swayTargets;

    [Header("State")]
    public bool isBeingPickedUp = false;

    private GameObject player;
    private CorgiController playerController;
    private Collider2D myTrigger;
    private Coroutine swayCoroutine;
    private bool playerInside = false;

    private Quaternion[] baseLocalRotations;

    private void Awake()
    {
        myTrigger = GetComponent<Collider2D>();

        if (pivotPoint == null)
            pivotPoint = transform;

        CacheBaseRotations();
    }

    private void Start()
    {
        TryRebindPlayer();
        KickIfOverlapping();
    }

    private void OnEnable()
    {
        if (pivotPoint == null)
            pivotPoint = transform;

        CacheBaseRotations();
        TryRebindPlayer();
        KickIfOverlapping();
    }

    private void Update()
    {
        if (player == null || !player.activeInHierarchy || playerController == null)
        {
            TryRebindPlayer();
        }
    }

    private void CacheBaseRotations()
    {
        if (swayTargets == null)
        {
            baseLocalRotations = new Quaternion[0];
            return;
        }

        baseLocalRotations = new Quaternion[swayTargets.Length];

        for (int i = 0; i < swayTargets.Length; i++)
        {
            if (swayTargets[i] != null)
                baseLocalRotations[i] = swayTargets[i].localRotation;
        }
    }

    private void TryRebindPlayer()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        playerController = player ? player.GetComponentInChildren<CorgiController>() : null;
    }

    private void KickIfOverlapping()
    {
        if (!isActiveAndEnabled || isBeingPickedUp || myTrigger == null || player == null)
            return;

        Collider2D[] playerColliders = player.GetComponentsInChildren<Collider2D>(true);
        foreach (Collider2D col in playerColliders)
        {
            if (col != null && myTrigger.IsTouching(col))
            {
                playerInside = true;
                StartSway();
                return;
            }
        }
    }

    private bool IsPlayerMoving()
    {
        return playerController != null && playerController.Speed.magnitude > 0.1f;
    }

    private void StartSway()
    {
        if (!isActiveAndEnabled || isBeingPickedUp || swayTargets == null || swayTargets.Length == 0)
            return;

        if (swayCoroutine != null)
            StopCoroutine(swayCoroutine);

        swayCoroutine = StartCoroutine(SwayRoutine());
    }

    private IEnumerator SwayRoutine()
    {
        while (!isBeingPickedUp)
        {
            float targetAngle = 0f;

            if (playerInside)
            {
                float motionMultiplier = IsPlayerMoving() ? 1f : 0.45f;
                targetAngle = Mathf.Sin(Time.time * swaySpeed) * swayAmount * motionMultiplier;
            }

            for (int i = 0; i < swayTargets.Length; i++)
            {
                if (swayTargets[i] == null)
                    continue;

                Quaternion targetRotation = baseLocalRotations[i] * Quaternion.Euler(0f, 0f, targetAngle);
                swayTargets[i].localRotation = Quaternion.Lerp(
                    swayTargets[i].localRotation,
                    targetRotation,
                    Time.deltaTime * swaySpeed
                );
            }

            if (!playerInside)
            {
                bool allClose = true;

                for (int i = 0; i < swayTargets.Length; i++)
                {
                    if (swayTargets[i] == null)
                        continue;

                    float angleDelta = Quaternion.Angle(swayTargets[i].localRotation, baseLocalRotations[i]);
                    if (angleDelta >= 0.05f)
                    {
                        allClose = false;
                        break;
                    }
                }

                if (allClose)
                {
                    ResetTargetsToBaseRotation();
                    break;
                }
            }

            yield return null;
        }

        swayCoroutine = null;
    }

    private void ResetTargetsToBaseRotation()
    {
        if (swayTargets == null || baseLocalRotations == null)
            return;

        for (int i = 0; i < swayTargets.Length; i++)
        {
            if (swayTargets[i] != null)
                swayTargets[i].localRotation = baseLocalRotations[i];
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isBeingPickedUp)
            return;

        if (other.GetComponentInParent<CorgiController>() != null)
        {
            playerInside = true;
            StartSway();
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (isBeingPickedUp)
            return;

        if (other.GetComponentInParent<CorgiController>() != null && swayCoroutine == null)
        {
            playerInside = true;
            StartSway();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (isBeingPickedUp)
            return;

        if (other.GetComponentInParent<CorgiController>() != null)
        {
            playerInside = false;
            StartSway();
        }
    }

    public void DisableSwayOnPickup()
    {
        isBeingPickedUp = true;
        playerInside = false;

        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
            swayCoroutine = null;
        }

        ResetTargetsToBaseRotation();
        enabled = false;
    }

    public void ReactivateAfterReattach(bool assumePlayerNearby)
    {
        isBeingPickedUp = false;
        enabled = true;

        if (pivotPoint == null)
            pivotPoint = transform;

        CacheBaseRotations();
        TryRebindPlayer();

        playerInside = assumePlayerNearby;

        if (swayCoroutine != null)
        {
            StopCoroutine(swayCoroutine);
            swayCoroutine = null;
        }

        StartSway();
    }
}