using System.Collections;
using UnityEngine;

[RequireComponent(typeof(FlowerHolder))]
public class ButterflyFlowerAssist : MonoBehaviour
{
    [Header("Input")]
    public float inputCooldown = 0.2f;

    [Header("Luna")]
    public Transform lunaTransform;
    public FlowerHolder lunaFlowerHolder;

    [Header("Butterfly")]
    public Transform butterflyTransform;
    public Transform butterflyFlowerHoldPoint;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;
    public ButterflyFatigue butterflyFatigue;
    public ButterflyPerchAndFatigue butterflyPerchAndFatigue;

    [Header("Ranges")]
    public float lunaFlowerDetectRadius = 0.8f;
    public float butterflyInteractRadius = 1.1f;
    public float butterflyFetchSpeed = 3.0f;
    public float flowerPickupStopDistance = 0.08f;
    public float butterflyReturnStopDistance = 0.08f;

    [Header("Fatigue")]
    public int fetchFatigueCost = 1;
    public int swapFatigueCost = 0;
    public int handoffFatigueCost = 0;

    [Header("Rules")]
    public bool allowFetchFromPlantedFlowers = true;
    public bool requireButterflyToBeNearLunaForSwapOrTake = true;
    public bool allowTakeWhileFollowing = true;
    public bool debugLogs = true;

    private GameObject _butterflyHeldFlower;
    private bool _isBusy = false;
    private float _cooldownTimer = 0f;

    private void Awake()
    {
        if (lunaTransform == null)
            lunaTransform = transform;

        if (lunaFlowerHolder == null)
            lunaFlowerHolder = GetComponent<FlowerHolder>();

        if (butterflyTransform == null)
        {
            FollowAndFlip foundFollow = FindObjectOfType<FollowAndFlip>();
            if (foundFollow != null)
                butterflyTransform = foundFollow.transform;
        }

        if (followAndFlip == null && butterflyTransform != null)
            followAndFlip = butterflyTransform.GetComponent<FollowAndFlip>();

        if (butterflyFatigue == null && butterflyTransform != null)
            butterflyFatigue = butterflyTransform.GetComponent<ButterflyFatigue>();

        if (butterflyPerchAndFatigue == null && butterflyTransform != null)
            butterflyPerchAndFatigue = butterflyTransform.GetComponent<ButterflyPerchAndFatigue>();

        if (butterflyRenderer == null && butterflyTransform != null)
            butterflyRenderer = butterflyTransform.GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if (_cooldownTimer > 0f)
            _cooldownTimer -= Time.deltaTime;

        RefreshButterflyHeldFlowerReference();
    }

    public bool TryConsumeInteract()
    {
        if (_cooldownTimer > 0f || _isBusy)
            return false;

        if (TryTakeFromButterfly())
        {
            _cooldownTimer = inputCooldown;
            return true;
        }

        if (TrySwapWithButterfly())
        {
            _cooldownTimer = inputCooldown;
            return true;
        }

        return false;
    }

    public bool ButterflyCurrentlyHasFlower()
    {
        RefreshButterflyHeldFlowerReference();
        return _butterflyHeldFlower != null;
    }

    public bool LunaCanTakeFromButterflyNow()
    {
        if (_isBusy || _cooldownTimer > 0f)
            return false;

        if (lunaFlowerHolder == null)
            return false;

        if (lunaFlowerHolder.HasFlower)
            return false;

        if (!ButterflyCurrentlyHasFlower())
            return false;

        return IsButterflyAvailableForTakeOrSwap(IsNearButterfly(), IsButterflyFollowingOrSummoning());
    }

    public bool LunaCanSwapWithButterflyNow()
    {
        if (_isBusy || _cooldownTimer > 0f)
            return false;

        if (lunaFlowerHolder == null)
            return false;

        if (!lunaFlowerHolder.HasFlower)
            return false;

        if (!ButterflyCurrentlyHasFlower())
            return false;

        return IsButterflyAvailableForTakeOrSwap(IsNearButterfly(), IsButterflyFollowingOrSummoning());
    }

    public bool CanFetchSpecificFlower(FlowerPickup targetFlower)
    {
        if (_isBusy || _cooldownTimer > 0f)
            return false;

        if (targetFlower == null)
            return false;

        if (lunaFlowerHolder == null || !lunaFlowerHolder.HasFlower)
            return false;

        if (ButterflyCurrentlyHasFlower())
            return false;

        if (ButterflyUnavailableForFetch())
            return false;

        if (targetFlower.IsHeld)
            return false;

        if (!allowFetchFromPlantedFlowers && targetFlower.CurrentGardenSpot != null)
            return false;

        return true;
    }

    public bool TryTakeFromButterfly()
    {
        if (!LunaCanTakeFromButterflyNow())
            return false;

        GiveButterflyFlowerToLuna();
        _cooldownTimer = inputCooldown;
        return true;
    }

    public bool TrySwapWithButterfly()
    {
        if (!LunaCanSwapWithButterflyNow())
            return false;

        SwapFlowers();
        _cooldownTimer = inputCooldown;
        return true;
    }

    public bool TryFetchSpecificFlower(FlowerPickup targetFlower)
    {
        if (!CanFetchSpecificFlower(targetFlower))
            return false;

        StartCoroutine(FetchFlowerRoutine(targetFlower));
        _cooldownTimer = inputCooldown;
        return true;
    }

    private void RefreshButterflyHeldFlowerReference()
    {
        if (butterflyFlowerHoldPoint == null)
            return;

        if (butterflyFlowerHoldPoint.childCount <= 0)
        {
            _butterflyHeldFlower = null;
            return;
        }

        Transform child = butterflyFlowerHoldPoint.GetChild(0);
        _butterflyHeldFlower = child != null ? child.gameObject : null;
    }

    private bool IsNearButterfly()
    {
        if (lunaTransform == null || butterflyTransform == null)
            return false;

        return Vector2.Distance(lunaTransform.position, butterflyTransform.position) <= butterflyInteractRadius;
    }

    private bool IsButterflyFollowingOrSummoning()
    {
        if (followAndFlip == null)
            return false;

        return followAndFlip.IsFollowing() || followAndFlip.IsSummoning();
    }

    private bool IsButterflyAvailableForTakeOrSwap(bool nearButterfly, bool butterflyFollowing)
    {
        if (!requireButterflyToBeNearLunaForSwapOrTake)
            return true;

        if (nearButterfly)
            return true;

        if (allowTakeWhileFollowing && butterflyFollowing)
            return true;

        return false;
    }

    private bool ButterflyUnavailableForFetch()
    {
        if (butterflyFatigue != null && butterflyFatigue.IsExhausted())
            return true;

        if (butterflyPerchAndFatigue != null && butterflyPerchAndFatigue.IsPerched())
            return true;

        if (butterflyPerchAndFatigue != null && butterflyPerchAndFatigue.IsPerching())
            return true;

        return false;
    }

    public GameObject GetButterflyHeldFlower()
    {
        RefreshButterflyHeldFlowerReference();
        return _butterflyHeldFlower;
    }

    public bool IsBusy()
    {
        return _isBusy;
    }

    private IEnumerator FetchFlowerRoutine(FlowerPickup targetFlower)
    {
        if (targetFlower == null || butterflyTransform == null || butterflyFlowerHoldPoint == null)
            yield break;

        _isBusy = true;

        bool followComponentWasEnabled = followAndFlip != null && followAndFlip.enabled;
        bool wasFollowingBeforeFetch = followAndFlip != null && followAndFlip.IsFollowing();

        Vector3 returnPosition = butterflyTransform.position;

        if (followAndFlip != null)
            followAndFlip.enabled = false;

        while (targetFlower != null &&
               Vector2.Distance(butterflyTransform.position, targetFlower.transform.position) > flowerPickupStopDistance)
        {
            Vector3 targetPos = targetFlower.transform.position;
            targetPos.z = butterflyTransform.position.z;

            FaceToward(targetPos.x);
            butterflyTransform.position = Vector3.MoveTowards(
                butterflyTransform.position,
                targetPos,
                butterflyFetchSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (targetFlower != null)
        {
            PrepareFlowerForPickup(targetFlower.gameObject, targetFlower);
            AttachFlowerToButterfly(targetFlower.gameObject);
            ApplyButterflyFatigue(fetchFatigueCost);

            if (debugLogs)
                Debug.Log($"[ButterflyFlowerAssist] Butterfly fetched specific flower {targetFlower.name}");
        }

        while (Vector2.Distance(butterflyTransform.position, returnPosition) > butterflyReturnStopDistance)
        {
            FaceToward(returnPosition.x);
            butterflyTransform.position = Vector3.MoveTowards(
                butterflyTransform.position,
                returnPosition,
                butterflyFetchSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (followAndFlip != null)
        {
            followAndFlip.enabled = followComponentWasEnabled;

            if (followComponentWasEnabled)
            {
                if (wasFollowingBeforeFetch)
                    followAndFlip.SnapBackToFollowWithoutSummon();
                else
                    followAndFlip.CaptureCurrentIdleHeight();

                followAndFlip.ForceFaceTowardLuna();
            }
        }

        FaceTowardLuna();
        RefreshButterflyHeldFlowerReference();
        _isBusy = false;
    }

    private void GiveButterflyFlowerToLuna()
    {
        RefreshButterflyHeldFlowerReference();

        if (_butterflyHeldFlower == null || lunaFlowerHolder == null)
            return;

        bool butterflyWasFollowing = IsButterflyFollowingOrSummoning();

        GameObject flowerToGive = _butterflyHeldFlower;
        _butterflyHeldFlower = null;

        PrepareLooseHeldFlowerForLunaPickup(flowerToGive);
        lunaFlowerHolder.PickUpFlower(flowerToGive);

        RefreshButterflyHeldFlowerReference();
        ApplyButterflyFatigue(handoffFatigueCost);

        if (followAndFlip != null)
        {
            if (butterflyWasFollowing)
                followAndFlip.SnapBackToFollowWithoutSummon();
            else
                followAndFlip.CaptureCurrentIdleHeight();

            followAndFlip.ForceFaceTowardLuna();
        }

        FaceTowardLuna();

        if (debugLogs)
            Debug.Log($"[ButterflyFlowerAssist] Luna took butterfly flower {flowerToGive.name}");
    }

    private void SwapFlowers()
    {
        RefreshButterflyHeldFlowerReference();

        if (lunaFlowerHolder == null)
            return;

        GameObject lunaFlower = lunaFlowerHolder.GetHeldFlower();
        GameObject butterflyFlower = _butterflyHeldFlower;

        if (lunaFlower == null || butterflyFlower == null)
            return;

        bool butterflyWasFollowing = IsButterflyFollowingOrSummoning();

        lunaFlowerHolder.ReleaseHeldFlowerReference();

        _butterflyHeldFlower = null;

        PrepareLooseHeldFlowerForLunaPickup(butterflyFlower);
        lunaFlowerHolder.PickUpFlower(butterflyFlower);

        PrepareLooseHeldFlowerForButterflyPickup(lunaFlower);
        AttachFlowerToButterfly(lunaFlower);

        RefreshButterflyHeldFlowerReference();
        ApplyButterflyFatigue(swapFatigueCost);

        if (followAndFlip != null)
        {
            if (butterflyWasFollowing)
                followAndFlip.SnapBackToFollowWithoutSummon();
            else
                followAndFlip.CaptureCurrentIdleHeight();

            followAndFlip.ForceFaceTowardLuna();
        }

        FaceTowardLuna();

        if (debugLogs)
            Debug.Log($"[ButterflyFlowerAssist] Swapped Luna flower {lunaFlower.name} with butterfly flower {butterflyFlower.name}");
    }

    private void AttachFlowerToButterfly(GameObject flowerObject)
    {
        if (flowerObject == null || butterflyFlowerHoldPoint == null)
            return;

        Vector3 originalWorldScale = flowerObject.transform.lossyScale;

        flowerObject.transform.SetParent(butterflyFlowerHoldPoint, false);
        flowerObject.transform.localPosition = Vector3.zero;
        flowerObject.transform.localRotation = Quaternion.identity;
        ApplyLocalScaleForDesiredWorldScale(flowerObject.transform, originalWorldScale);

        Collider2D flowerCol = flowerObject.GetComponent<Collider2D>();
        if (flowerCol != null)
            flowerCol.enabled = true;

        _butterflyHeldFlower = flowerObject;
    }

    private void PrepareFlowerForPickup(GameObject flowerObject, FlowerPickup flowerPickup)
    {
        if (flowerObject == null)
            return;

        if (flowerPickup != null && flowerPickup.CurrentGardenSpot != null)
            flowerPickup.CurrentGardenSpot.ClearPlantedFlower();

        SproutAndLightManager sm = flowerObject.GetComponent<SproutAndLightManager>();
        if (sm != null)
        {
            sm.isHeld = false;
            sm.isPlanted = false;
        }

        FlowerSway sway = flowerObject.GetComponent<FlowerSway>();
        if (sway != null)
            sway.ReactivateAfterReattach(false);
    }

    private void PrepareLooseHeldFlowerForLunaPickup(GameObject flowerObject)
    {
        if (flowerObject == null)
            return;

        SproutAndLightManager sm = flowerObject.GetComponent<SproutAndLightManager>();
        if (sm != null)
        {
            sm.isHeld = false;
            sm.isPlanted = false;
        }

        FlowerSway sway = flowerObject.GetComponent<FlowerSway>();
        if (sway != null)
            sway.ReactivateAfterReattach(false);
    }

    private void PrepareLooseHeldFlowerForButterflyPickup(GameObject flowerObject)
    {
        if (flowerObject == null)
            return;

        SproutAndLightManager sm = flowerObject.GetComponent<SproutAndLightManager>();
        if (sm != null)
        {
            sm.isHeld = false;
            sm.isPlanted = false;
        }

        FlowerSway sway = flowerObject.GetComponent<FlowerSway>();
        if (sway != null)
            sway.ReactivateAfterReattach(false);
    }

    private void ApplyButterflyFatigue(int amount)
    {
        if (butterflyFatigue == null || amount <= 0)
            return;

        for (int i = 0; i < amount; i++)
        {
            if (butterflyFatigue.IsExhausted())
                break;

            butterflyFatigue.ApplyFatigue();
        }
    }

    private void FaceToward(float worldX)
    {
        if (butterflyRenderer == null || butterflyTransform == null)
            return;

        float dx = worldX - butterflyTransform.position.x;

        if (dx > 0.02f)
            butterflyRenderer.flipX = true;
        else if (dx < -0.02f)
            butterflyRenderer.flipX = false;
    }

    private void FaceTowardLuna()
    {
        if (lunaTransform == null)
            return;

        FaceToward(lunaTransform.position.x);
    }

    private void ApplyLocalScaleForDesiredWorldScale(Transform target, Vector3 desiredWorldScale)
    {
        Vector3 parentLossy = Vector3.one;

        if (target.parent != null)
            parentLossy = target.parent.lossyScale;

        float x = SafeDivide(desiredWorldScale.x, parentLossy.x);
        float y = SafeDivide(desiredWorldScale.y, parentLossy.y);
        float z = SafeDivide(desiredWorldScale.z, parentLossy.z);

        target.localScale = new Vector3(x, y, z);
    }

    private float SafeDivide(float a, float b)
    {
        if (Mathf.Abs(b) < 0.0001f)
            return a;

        float result = a / b;

        if (float.IsNaN(result) || float.IsInfinity(result))
            return a;

        return result;
    }

    private void OnDrawGizmosSelected()
    {
        Transform center = lunaTransform != null ? lunaTransform : transform;

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(center.position, lunaFlowerDetectRadius);

        if (butterflyTransform != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(butterflyTransform.position, butterflyInteractRadius);
        }
    }
}