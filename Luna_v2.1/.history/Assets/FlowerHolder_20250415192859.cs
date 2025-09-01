using UnityEngine;
using System.Collections.Generic;

public class FlowerHolder : MonoBehaviour
{
    public Transform holdPoint;

    private GameObject heldFlower;
    private GardenSpot currentSpot;
    private Dictionary<GameObject, GameObject> planted = new Dictionary<GameObject, GameObject>();

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
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
                    PickUp(flower);
                    planted.Remove(currentSpot.gameObject);
                }
            }
        }
    }

    public bool HasFlower() => heldFlower != null;

    public void PickUp(GameObject flower)
    {
        heldFlower = flower;
        heldFlower.GetComponent<Collider2D>().enabled = false;

        // Keep all transforms intact
        heldFlower.transform.SetParent(holdPoint, worldPositionStays: true);
    }

    public void PlantFlower(GardenSpot spot)
    {
        if (heldFlower == null) return;

        Transform plantingPoint = spot.transform.Find("PlantingPoint");
        if (plantingPoint == null)
        {
            Debug.LogWarning("No planting point found!");
            return;
        }

        heldFlower.transform.SetParent(null);
        heldFlower.transform.position = plantingPoint.position;

        heldFlower.GetComponent<Collider2D>().enabled = true;

        planted[spot.gameObject] = heldFlower;
        heldFlower = null;
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
