using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;
    private string heldFlowerType;

    public bool HasFlower => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public string CurrentFlowerType => heldFlowerType;


    public void PickUpFlower(GameObject flower)
    {
        if (HasFlower || flower == null) return;

        heldFlower = flower;
        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        flower.transform.SetParent(holdPoint);
        flower.transform.localPosition = Vector3.zero;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null)
        {
            sprout.isHeld = true;
            sprout.isPlanted = false;
        }

        var col = flower.GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        flower.SetActive(true);
    }

    public void DropFlower()
    {
        heldFlower = null;
        heldFlowerType = "";
    }
}
