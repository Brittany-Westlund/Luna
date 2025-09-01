using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private Transform heldPivot;
    private string heldFlowerType = "";

    private GardenSpot currentSpot;
    private Dictionary<GameObject, GameObject> planted = new Dictionary<GameObject, GameObject>();

    private float cooldown = 0.1f;
    private float cooldownTimer = 0f;
    private bool canInteract = true;

    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (cooldownTimer >= cooldown) canInteract = true;

        if (Input.GetKeyDown(KeyCode.X) && canInteract)
        {
            if (heldFlower != null && currentSpot != null)
            {
                PlantFlower(currentSpot);
            }
            else if (heldFlower == null && currentSpot != null)
            {
                GameObject flower = GetPlanted(currentSpot);
                if (flower != null)
                {
                    PickUpFlowerInstance(flower);
                    planted.Remove(currentSpot.gameObject);
                }
            }

            cooldownTimer = 0f;
            canInteract = false;
        }
    }

    public bool HasFlower() => heldFlower != null;
    public GameObject GetHeldFlower() => heldFlower;
    public string currentFlowerType => heldFlowerType;

    public void DropFlower()
    {
        heldFlower = null;
        heldPivot = null;
        heldFlowerType = "";
    }

    public void PickUpFlowerInstance(GameObject flower)
    {
        heldFlower = flower;
        heldPivot = flower.transform.Find("pivot");

        if (heldPivot == null)
        {
            Debug.LogError("❌ 'pivot' child not found on: " + flower.name);
            return;
        }

        heldFlowerType = flower.GetComponent<FlowerPickup>()?.flowerType ?? "Unknown";

        flower.GetComponent<Collider2D>().enabled = false;

        var sprout = flower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = true;

        // Align flower so pivot matches holdPoint
        Vector3 offset = flower.transform.position - heldPivot.position;
        flower.transform.position = holdPoint.position + offset;

        flower.transform.SetParent(holdPoint, true);
    }

    public void PlantFlower(GardenSpot spot)
    {
        if (heldFlower == null || heldPivot == null) return;

        Transform plantingPoint = spot.transform.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogError("❌ No 'PlantingPoint' found under: " + spot.name);
            return;
        }

        heldFlower.transform.SetParent(null);

        Vector3 offset = heldFlower.transform.position - heldPivot.position;
        heldFlower.transform.position = plantingPoint.position + offset;

        heldFlower.GetComponent<Collider2D>().enabled = true;

        var sprout = heldFlower.GetComponent<SproutAndLightManager>();
        if (sprout != null) sprout.isHeld = false;

        planted[spot.gameObject] = heldFlower;

        DropFlower();
    }

    private GameObject GetPlanted(GardenSpot spot)
    {
        planted.TryGetValue(spot.gameObject, out GameObject flower);
        return flower;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        GardenSpot spot = other.GetComponent<GardenSpot>();
        if (spot != null)
        {
            currentSpot = spot;
            currentSpot.SetHighlight(true);
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        GardenSpot spot = other.GetComponent<GardenSpot>();
        if (spot != null && spot == currentSpot)
        {
            currentSpot.SetHighlight(false);
            currentSpot = null;
        }
    }
}
