using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    public bool HasFlower => heldFlower != null;

    public GameObject GetHeldFlower() => heldFlower;

    public string CurrentFlowerType
    {
        get
        {
            var pickup = heldFlower?.GetComponent<FlowerPickup>();
            return pickup != null ? pickup.flowerType : "Unknown";
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        heldFlower = flower;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.SetActive(true);
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;

        heldFlower.transform.SetParent(null);
        heldFlower = null;
    }
}
