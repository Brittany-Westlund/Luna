using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    [HideInInspector] public Transform holdPoint;
    private GameObject heldFlower;

    public bool HasFlower
    {
        get { return heldFlower != null; }
    }

    public GameObject GetHeldFlower()
    {
        return heldFlower;
    }

    public string CurrentFlowerType
    {
        get
        {
            if (heldFlower == null) return null;
            var pickup = heldFlower.GetComponent<FlowerPickup>();
            return (pickup != null) ? pickup.flowerType : null;
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        if (heldFlower != null) return;  // only one at a time

        heldFlower = flower;
        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld    = true;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;
        heldFlower.transform.SetParent(null);
        heldFlower = null;
    }
}
