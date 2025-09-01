using UnityEngine;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;
    public GameObject currentFlowerObject;
    public string currentFlowerType;

    public bool HasFlower() => currentFlowerObject != null;

    public void PickUpFlower(FlowerPickup flower)
    {
        currentFlowerType = flower.flowerType;
        currentFlowerObject = Instantiate(flower.gameObject, holdPoint.position, Quaternion.identity, holdPoint);
        flower.DisableFlowerVisual();
        flower.DestroySelf();
    }

    public void DropFlower()
    {
        if (currentFlowerObject != null)
        {
            Destroy(currentFlowerObject);
            currentFlowerObject = null;
            currentFlowerType = "";
        }
    }
}