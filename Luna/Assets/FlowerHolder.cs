using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    [Header("Hold")]
    [SerializeField] public Transform holdPoint;

    [Header("Audio")]
    public AudioSource pickupSFXSource;
    public AudioSource plantSFXSource;

    private GameObject heldFlower;

    public bool HasFlower => heldFlower != null;

    public GameObject GetHeldFlower() => heldFlower;

    public string CurrentFlowerType
    {
        get
        {
            if (heldFlower == null)
                return null;

            FlowerPickup pickup = heldFlower.GetComponent<FlowerPickup>();
            return pickup != null ? pickup.flowerType : null;
        }
    }

    private void Awake()
    {
        if (holdPoint == null)
        {
            GameObject go = new GameObject("HoldPoint");
            holdPoint = go.transform;
            holdPoint.SetParent(transform, false);
            holdPoint.localPosition = new Vector3(0f, 0.75f, 0f);
            holdPoint.localRotation = Quaternion.identity;
            holdPoint.localScale = Vector3.one;
        }
    }

    public bool PickUpFlower(GameObject flower)
    {
        if (flower == null)
        {
            Debug.LogWarning("❌ PickUpFlower called with null flower.");
            return false;
        }

        if (heldFlower != null)
        {
            Debug.Log("❌ Cannot pick up — already holding a flower.");
            return false;
        }

        Debug.Log($"🌼 PickUpFlower called on {flower.name}");

        SproutAndLightManager sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        FlowerSway sway = flower.GetComponent<FlowerSway>();
        if (sway != null)
        {
            sway.DisableSwayOnPickup();
        }

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = false;
        }

        flower.transform.SetParent(holdPoint, true);
        flower.transform.localPosition = Vector3.zero;
        flower.transform.localRotation = Quaternion.identity;

        heldFlower = flower;

        PlayPickupSFX();
        return true;
    }

    /// <summary>
    /// Drops the currently held flower into the world.
    /// This is kept for compatibility with older scripts that already call DropFlower().
    /// </summary>
    public void DropFlower()
    {
        DropFlowerToWorld();
    }

    /// <summary>
    /// Drops the currently held flower into the world and clears the held reference.
    /// Re-enables collider and sway.
    /// </summary>
    public GameObject DropFlowerToWorld()
    {
        if (heldFlower == null)
            return null;

        GameObject flower = heldFlower;
        heldFlower = null;

        SproutAndLightManager sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(null, true);

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null)
        {
            col.enabled = true;
        }

        FlowerSway sway = flower.GetComponent<FlowerSway>();
        if (sway != null)
        {
            sway.ReactivateAfterReattach(false);
        }

        PlayPlantSFX();
        return flower;
    }

    /// <summary>
    /// Clears the held-flower reference without unparenting or moving the flower.
    /// Use this when some other system already reparented the flower, such as planting into a garden.
    /// </summary>
    public GameObject ReleaseHeldFlowerReference()
    {
        if (heldFlower == null)
            return null;

        GameObject flower = heldFlower;
        heldFlower = null;

        SproutAndLightManager sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = false;
        }

        PlayPlantSFX();
        return flower;
    }

    public void ClearHeldFlower()
    {
        heldFlower = null;
    }

    public void PlayPickupSFX()
    {
        Debug.Log($"PlayPickupSFX called on {gameObject.name}");
        if (pickupSFXSource != null)
        {
            pickupSFXSource.Play();
        }
    }

    public void PlayPlantSFX()
    {
        Debug.Log($"PlayPlantSFX called on {gameObject.name}");
        if (plantSFXSource != null)
        {
            plantSFXSource.Play();
        }
    }
}