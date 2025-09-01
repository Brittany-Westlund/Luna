// FlowerHolder.cs
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
            if (heldFlower == null)
                return null;
            FlowerPickup pickup = heldFlower.GetComponent<FlowerPickup>();
            if (pickup != null)
                return pickup.flowerType;
            return null;
        }
    }

    public void PickUpFlower(GameObject flower)
    {
        heldFlower = flower;

        SproutAndLightManager sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld    = true;
            sprout.isPlanted = false;
        }

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        Collider2D col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
    }

    public void DropFlower()
    {
        if (heldFlower == null) return;
        heldFlower.transform.SetParent(null);
        heldFlower = null;
    }
}
