using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(FlowerHolder))]
public class FlowerInteractionManager : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private KeyCode primaryInteractKey = KeyCode.F;
    [SerializeField] private KeyCode secondaryInteractKey = KeyCode.X;

    [Header("Hold Point")]
    public Transform holdPoint;

    [Header("Interaction")]
    [SerializeField] private float inputCooldownDuration = 0.25f;
    [SerializeField] private float initialGardenCheckRadius = 0.5f;
    [SerializeField] private bool debugLogs = true;

    [Header("Flower Pickup Range")]
    [SerializeField] private float maxFlowerPickupDistance = 0.8f;

    [Header("Planting Range")]
    [SerializeField] private float maxPlantDistance = 0.65f;
    [SerializeField] private bool usePlantingPointForDistance = true;

    [Header("Garden Cleanup")]
    [SerializeField] private float maxGardenRetentionDistance = 2.0f;

    [Header("Butterfly Assist")]
    [SerializeField] private ButterflyFlowerAssist butterflyFlowerAssist;

    private FlowerHolder flowerHolder;
    private GardenSpot currentGarden;
    private FlowerPickup currentNearbyFlower;
    private TeapotReceiver teapotReceiver;

    private readonly List<GardenSpot> nearbyGardens = new List<GardenSpot>();
    private readonly Dictionary<GardenSpot, int> gardenOverlapCounts = new Dictionary<GardenSpot, int>();

    private readonly List<FlowerPickup> nearbyFlowers = new List<FlowerPickup>();
    private readonly Dictionary<FlowerPickup, int> flowerOverlapCounts = new Dictionary<FlowerPickup, int>();

    private float inputCooldown = 0f;

    private void Awake()
    {
        flowerHolder = GetComponent<FlowerHolder>();

        if (holdPoint != null)
        {
            flowerHolder.holdPoint = holdPoint;
        }
        else
        {
            holdPoint = flowerHolder.holdPoint;
        }

        if (butterflyFlowerAssist == null)
            butterflyFlowerAssist = GetComponent<ButterflyFlowerAssist>();

        if (debugLogs)
            Debug.Log($"[FlowerInteractionManager] Awake on {gameObject.name}");
    }

    private void OnEnable()
    {
        StartCoroutine(DetectInitialGardenOverlap());
    }

    private IEnumerator DetectInitialGardenOverlap()
    {
        yield return new WaitForFixedUpdate();

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, initialGardenCheckRadius);
        foreach (Collider2D col in hits)
        {
            if (col == null || !col.CompareTag("Garden"))
                continue;

            GardenSpot gs = col.GetComponent<GardenSpot>();
            if (gs == null)
                continue;

            if (!nearbyGardens.Contains(gs))
                nearbyGardens.Add(gs);

            if (!gardenOverlapCounts.ContainsKey(gs))
                gardenOverlapCounts[gs] = 1;

            if (debugLogs)
                Debug.Log($"[FlowerInteractionManager] Initial garden overlap: {gs.name}");
        }
    }

    private void Update()
    {
        if (inputCooldown > 0f)
            inputCooldown -= Time.deltaTime;

        UpdateGardenSelection();
        UpdateNearbyFlowerSelection();
        UpdateGardenFeedback();

        bool interactPressed = Input.GetKeyDown(primaryInteractKey) || Input.GetKeyDown(secondaryInteractKey);
        if (!interactPressed)
            return;

        if (debugLogs)
        {
            Debug.Log(
                $"[FlowerInteractionManager] Interact pressed | " +
                $"HasFlower={flowerHolder.HasFlower} | " +
                $"CurrentGarden={(currentGarden != null ? currentGarden.name : "NULL")} | " +
                $"NearbyFlower={(currentNearbyFlower != null ? currentNearbyFlower.name : "NULL")} | " +
                $"Teapot={(teapotReceiver != null ? teapotReceiver.name : "NULL")}"
            );
        }

        if (inputCooldown > 0f)
        {
            if (debugLogs)
                Debug.Log("[FlowerInteractionManager] Input blocked by cooldown.");
            return;
        }

        if (teapotReceiver != null)
        {
            HandleTeapotInput();
            inputCooldown = inputCooldownDuration;
            return;
        }

        // 1) Plant takes priority if Luna is holding and can plant.
        if (flowerHolder.HasFlower && currentGarden != null && IsWithinPlantingDistance(currentGarden))
        {
            TryPlantToGarden();
            inputCooldown = inputCooldownDuration;
            return;
        }

        // 2) If Luna is empty and butterfly has one, taking it back wins.
        if (!flowerHolder.HasFlower && butterflyFlowerAssist != null && butterflyFlowerAssist.LunaCanTakeFromButterflyNow())
        {
            butterflyFlowerAssist.TryTakeFromButterfly();
            inputCooldown = inputCooldownDuration;
            return;
        }

        // 3) If both are holding flowers, swap wins before pickup/fetch.
        if (flowerHolder.HasFlower && butterflyFlowerAssist != null && butterflyFlowerAssist.LunaCanSwapWithButterflyNow())
        {
            butterflyFlowerAssist.TrySwapWithButterfly();
            inputCooldown = inputCooldownDuration;
            return;
        }

        // 4) If Luna is empty, normal local pickup wins.
        if (!flowerHolder.HasFlower)
        {
            TryPickUpFlower();
            inputCooldown = inputCooldownDuration;
            return;
        }

        // 5) Luna already has a flower and is pressing F on a specific nearby flower:
        // butterfly should fetch THAT flower only.
        if (flowerHolder.HasFlower &&
            currentNearbyFlower != null &&
            butterflyFlowerAssist != null &&
            butterflyFlowerAssist.CanFetchSpecificFlower(currentNearbyFlower))
        {
            butterflyFlowerAssist.TryFetchSpecificFlower(currentNearbyFlower);
            inputCooldown = inputCooldownDuration;
            return;
        }

        // 6) Fallback planting attempt.
        TryPlantToGarden();
        inputCooldown = inputCooldownDuration;
    }

    private void HandleTeapotInput()
    {
        if (flowerHolder.HasFlower)
        {
            teapotReceiver.AddFlowerToTeapot(flowerHolder);
        }
        else if (teapotReceiver.HasAnyIngredients())
        {
            teapotReceiver.RetrieveLastFlower(flowerHolder);
        }
        else if (debugLogs)
        {
            Debug.Log("[FlowerInteractionManager] Teapot active, but nothing to add or retrieve.");
        }
    }

    private void UpdateGardenSelection()
    {
        GardenSpot best = null;
        float minDistance = float.MaxValue;

        for (int i = nearbyGardens.Count - 1; i >= 0; i--)
        {
            GardenSpot gs = nearbyGardens[i];

            if (gs == null)
            {
                nearbyGardens.RemoveAt(i);
                continue;
            }

            float distanceToGarden = GetDistanceToGarden(gs);

            bool hasValidOverlapCount = gardenOverlapCounts.ContainsKey(gs) && gardenOverlapCounts[gs] > 0;
            bool isClearlyTooFarAway = distanceToGarden > maxGardenRetentionDistance;

            if (!hasValidOverlapCount || isClearlyTooFarAway)
            {
                ForceRemoveGarden(gs);

                if (debugLogs && isClearlyTooFarAway)
                    Debug.Log($"[FlowerInteractionManager] Force-removed stale garden {gs.name} at distance {distanceToGarden:F2}");

                continue;
            }

            if (distanceToGarden < minDistance)
            {
                minDistance = distanceToGarden;
                best = gs;
            }
        }

        currentGarden = best;
    }

    private void UpdateGardenFeedback()
    {
        for (int i = 0; i < nearbyGardens.Count; i++)
        {
            GardenSpot gs = nearbyGardens[i];
            if (gs == null)
                continue;

            bool isClosest = gs == currentGarden;
            gs.SetHighlight(isClosest);
        }
    }

    private void UpdateNearbyFlowerSelection()
    {
        FlowerPickup best = null;
        float minDistance = float.MaxValue;

        for (int i = nearbyFlowers.Count - 1; i >= 0; i--)
        {
            FlowerPickup fp = nearbyFlowers[i];

            if (fp == null)
            {
                nearbyFlowers.RemoveAt(i);
                continue;
            }

            bool hasValidOverlapCount = flowerOverlapCounts.ContainsKey(fp) && flowerOverlapCounts[fp] > 0;
            float distanceToFlower = Vector2.Distance(transform.position, fp.transform.position);
            bool isTooFarAway = distanceToFlower > maxFlowerPickupDistance;

            if (!hasValidOverlapCount || isTooFarAway || fp.IsHeld)
            {
                if (!hasValidOverlapCount || isTooFarAway)
                {
                    nearbyFlowers.RemoveAt(i);
                    flowerOverlapCounts.Remove(fp);

                    if (currentNearbyFlower == fp)
                        currentNearbyFlower = null;
                }

                continue;
            }

            if (distanceToFlower < minDistance)
            {
                minDistance = distanceToFlower;
                best = fp;
            }
        }

        currentNearbyFlower = best;
    }

    private float GetDistanceToGarden(GardenSpot garden)
    {
        if (garden == null)
            return float.MaxValue;

        if (usePlantingPointForDistance)
        {
            Transform plantingPoint = garden.GetPlantingPoint();
            if (plantingPoint != null)
                return Vector2.Distance(transform.position, plantingPoint.position);
        }

        return Vector2.Distance(transform.position, garden.transform.position);
    }

    private bool IsWithinPlantingDistance(GardenSpot garden)
    {
        if (garden == null)
            return false;

        return GetDistanceToGarden(garden) <= maxPlantDistance;
    }

    private bool IsWithinFlowerPickupDistance(FlowerPickup flower)
    {
        if (flower == null)
            return false;

        return Vector2.Distance(transform.position, flower.transform.position) <= maxFlowerPickupDistance;
    }

    private void TryPickUpFlower()
    {
        if (debugLogs)
        {
            Debug.Log(
                $"[FlowerInteractionManager] TryPickUpFlower | " +
                $"NearbyFlower={(currentNearbyFlower != null ? currentNearbyFlower.name : "NULL")} | " +
                $"CurrentGarden={(currentGarden != null ? currentGarden.name : "NULL")}"
            );
        }

        if (currentNearbyFlower != null)
        {
            if (!IsWithinFlowerPickupDistance(currentNearbyFlower))
            {
                if (debugLogs)
                    Debug.Log($"[FlowerInteractionManager] Cannot pick up flower: too far away. Dist={Vector2.Distance(transform.position, currentNearbyFlower.transform.position):F2}, Max={maxFlowerPickupDistance:F2}");
                return;
            }

            GameObject flowerObject = currentNearbyFlower.gameObject;

            GardenSpot plantedSpot = currentNearbyFlower.CurrentGardenSpot;
            if (plantedSpot != null)
            {
                plantedSpot.ClearPlantedFlower();

                if (debugLogs)
                    Debug.Log($"[FlowerInteractionManager] Clearing planted reference from garden {plantedSpot.name} for flower {flowerObject.name}");
            }

            SproutAndLightManager sm = flowerObject.GetComponent<SproutAndLightManager>();
            if (sm != null)
            {
                sm.isHeld = false;
                sm.isPlanted = false;
            }

            flowerHolder.PickUpFlower(flowerObject);
            return;
        }

        if (currentGarden != null)
        {
            GameObject plantedFlower = currentGarden.GetPlantedFlower();
            if (plantedFlower != null)
            {
                float dist = Vector2.Distance(transform.position, plantedFlower.transform.position);
                if (dist > maxFlowerPickupDistance)
                {
                    if (debugLogs)
                        Debug.Log($"[FlowerInteractionManager] Cannot pick up planted flower: too far away. Dist={dist:F2}, Max={maxFlowerPickupDistance:F2}");
                    return;
                }

                currentGarden.ClearPlantedFlower();

                SproutAndLightManager sm = plantedFlower.GetComponent<SproutAndLightManager>();
                if (sm != null)
                {
                    sm.isHeld = false;
                    sm.isPlanted = false;
                }

                flowerHolder.PickUpFlower(plantedFlower);

                if (debugLogs)
                    Debug.Log($"[FlowerInteractionManager] Picked up planted flower {plantedFlower.name} from garden {currentGarden.name}");

                return;
            }
        }

        if (debugLogs)
            Debug.Log("[FlowerInteractionManager] No flower available to pick up.");
    }

    private void TryPlantToGarden()
    {
        if (currentGarden == null)
        {
            if (debugLogs)
                Debug.Log("[FlowerInteractionManager] Cannot plant: no nearby garden.");
            return;
        }

        if (!IsWithinPlantingDistance(currentGarden))
        {
            if (debugLogs)
                Debug.Log($"[FlowerInteractionManager] Cannot plant: too far from garden. Dist={GetDistanceToGarden(currentGarden):F2}, Max={maxPlantDistance:F2}");
            return;
        }

        if (!flowerHolder.HasFlower)
        {
            if (debugLogs)
                Debug.Log("[FlowerInteractionManager] Cannot plant: holder has no flower.");
            return;
        }

        GameObject held = flowerHolder.GetHeldFlower();
        if (held == null)
        {
            if (debugLogs)
                Debug.LogWarning("[FlowerInteractionManager] Holder says HasFlower, but GetHeldFlower returned null.");
            return;
        }

        GameObject old = currentGarden.GetPlantedFlower();
        if (old == held)
            old = null;

        currentGarden.ClearPlantedFlower();

        Transform plantingPoint = currentGarden.GetPlantingPoint();
        if (plantingPoint == null)
        {
            Debug.LogWarning($"[FlowerInteractionManager] Garden {currentGarden.name} has no planting point.");
            return;
        }

        Vector3 originalWorldScale = held.transform.lossyScale;

        held.transform.SetParent(plantingPoint, false);
        held.transform.localPosition = Vector3.zero;
        held.transform.localRotation = Quaternion.identity;
        ApplyLocalScaleForDesiredWorldScale(held.transform, originalWorldScale);

        Collider2D heldCol = held.GetComponent<Collider2D>();
        if (heldCol != null)
            heldCol.enabled = true;

        SproutAndLightManager heldSM = held.GetComponent<SproutAndLightManager>();
        if (heldSM != null)
        {
            heldSM.isPlanted = true;
            heldSM.isHeld = false;
            heldSM.ResetInitialPosition();
        }

        FlowerSway heldSway = held.GetComponent<FlowerSway>();
        if (heldSway != null)
        {
            heldSway.ReactivateAfterReattach(false);
        }

        currentGarden.SetPlantedFlower(held);
        flowerHolder.ReleaseHeldFlowerReference();

        if (debugLogs)
            Debug.Log($"[FlowerInteractionManager] Planted {held.name} into {currentGarden.name}");

        if (old != null)
        {
            SproutAndLightManager oldSM = old.GetComponent<SproutAndLightManager>();
            if (oldSM != null)
            {
                oldSM.isPlanted = false;
                oldSM.isHeld = false;
            }

            old.transform.SetParent(null, true);
            flowerHolder.PickUpFlower(old);

            if (debugLogs)
                Debug.Log($"[FlowerInteractionManager] Swapped out old flower {old.name}");
        }
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

    private void ForceRemoveGarden(GardenSpot gs)
    {
        if (gs == null)
            return;

        gs.SetHighlight(false);

        nearbyGardens.Remove(gs);
        gardenOverlapCounts.Remove(gs);

        if (currentGarden == gs)
            currentGarden = null;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
            if (gs == null)
                return;

            if (!gardenOverlapCounts.ContainsKey(gs))
                gardenOverlapCounts[gs] = 0;

            gardenOverlapCounts[gs]++;

            if (!nearbyGardens.Contains(gs))
            {
                nearbyGardens.Add(gs);

                if (debugLogs)
                    Debug.Log($"[FlowerInteractionManager] Entered garden trigger: {gs.name}");
            }

            return;
        }

        if (other.CompareTag("Teapot"))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
            return;
        }

        FlowerPickup flower = other.GetComponent<FlowerPickup>() ?? other.GetComponentInParent<FlowerPickup>();
        if (flower != null)
        {
            if (!flowerOverlapCounts.ContainsKey(flower))
                flowerOverlapCounts[flower] = 0;

            flowerOverlapCounts[flower]++;

            if (!nearbyFlowers.Contains(flower))
            {
                nearbyFlowers.Add(flower);

                if (debugLogs)
                    Debug.Log($"[FlowerInteractionManager] Entered flower trigger: {flower.name}");
            }
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Teapot"))
        {
            teapotReceiver = other.GetComponent<TeapotReceiver>();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot gs = other.GetComponent<GardenSpot>();
            if (gs == null)
                return;

            if (gardenOverlapCounts.ContainsKey(gs))
            {
                gardenOverlapCounts[gs]--;

                if (gardenOverlapCounts[gs] <= 0)
                {
                    ForceRemoveGarden(gs);

                    if (debugLogs)
                        Debug.Log($"[FlowerInteractionManager] Fully exited garden: {gs.name}");
                }
            }
            else
            {
                ForceRemoveGarden(gs);
            }

            return;
        }

        if (other.CompareTag("Teapot"))
        {
            teapotReceiver = null;
            return;
        }

        FlowerPickup flower = other.GetComponent<FlowerPickup>() ?? other.GetComponentInParent<FlowerPickup>();
        if (flower != null && flowerOverlapCounts.ContainsKey(flower))
        {
            flowerOverlapCounts[flower]--;

            if (flowerOverlapCounts[flower] <= 0)
            {
                flowerOverlapCounts.Remove(flower);
                nearbyFlowers.Remove(flower);

                if (currentNearbyFlower == flower)
                    currentNearbyFlower = null;

                if (debugLogs)
                    Debug.Log($"[FlowerInteractionManager] Fully exited flower: {flower.name}");
            }
        }
    }
}