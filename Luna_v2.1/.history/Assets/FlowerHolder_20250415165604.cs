using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    [Header("Held Flower Settings")]
    public Transform holdPoint;

    private GameObject heldFlower;
    private GardenSpot closestGarden;
    private List<GardenSpot> nearbyGardens = new List<GardenSpot>();

    private bool canInteract = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;

    private void Update()
    {
        FindClosestGarden();

        if (canInteract && closestGarden != null && Input.GetKeyDown(KeyCode.X))
        {
            Transform plantingPoint = closestGarden.transform.Find("PlantingPoint");
            GameObject flowerInSpot = GetPlantedFlower(plantingPoint);

            if (heldFlower == null)
            {
                if (flowerInSpot != null)
                {
                    PickUp(flowerInSpot);
                }
            }
            else
            {
                if (flowerInSpot != null) Remove(flowerInSpot);
                Plant(heldFlower, plantingPoint);
            }

            canInteract = false;
        }

        if (!canInteract)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= interactionCooldown)
            {
                canInteract = true;
                cooldownTimer = 0f;
            }
        }
    }

    private void PickUp(GameObject flower)
    {
        heldFlower = flower;

        Transform pivot = flower.transform.Find("pivot");
        if (pivot != null)
        {
            Vector3 worldOffset = flower.transform.position - pivot.position;
            flower.transform.SetParent(holdPoint);
            flower.transform.position = holdPoint.position + worldOffset;
        }
        else
        {
            flower.transform.SetParent(holdPoint);
            flower.transform.localPosition = Vector3.zero;
        }

        flower.transform.localScale = Vector3.one;
        flower.transform.localRotation = Quaternion.identity;
        flower.GetComponent<Collider2D>().enabled = false;
    }

    private void Plant(GameObject flower, Transform plantingPoint)
    {
        heldFlower = null;

        Transform pivot = flower.transform.Find("pivot");
        if (pivot != null)
        {
            Vector3 worldOffset = flower.transform.position - pivot.position;
            flower.transform.SetParent(plantingPoint);
            flower.transform.position = plantingPoint.position + worldOffset;
        }
        else
        {
            flower.transform.SetParent(plantingPoint);
            flower.transform.position = plantingPoint.position;
        }

        flower.transform.localScale = Vector3.one;
        flower.transform.localRotation = Quaternion.identity;
        flower.GetComponent<Collider2D>().enabled = true;
    }

    private void Remove(GameObject flower)
    {
        flower.transform.SetParent(null);
        flower.SetActive(true);
    }

    private GameObject GetPlantedFlower(Transform plantingPoint)
    {
        if (plantingPoint.childCount == 0) return null;
        return plantingPoint.GetChild(0).gameObject;
    }

    private void FindClosestGarden()
    {
        closestGarden = null;
        float closestDistance = float.MaxValue;

        foreach (GardenSpot garden in nearbyGardens)
        {
            Transform point = garden.transform.Find("PlantingPoint");
            if (point == null) continue;

            float distance = Vector2.Distance(transform.position, point.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestGarden = garden;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null && !nearbyGardens.Contains(garden))
                nearbyGardens.Add(garden);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Garden"))
        {
            GardenSpot garden = other.GetComponent<GardenSpot>();
            if (garden != null)
                nearbyGardens.Remove(garden);
        }
    }
}
