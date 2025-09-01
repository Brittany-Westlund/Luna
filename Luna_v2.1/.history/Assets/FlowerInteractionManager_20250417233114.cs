using UnityEngine;

public class FlowerInteractionManager : MonoBehaviour
{
    public Transform holdPoint;
    private GameObject heldFlower;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (heldFlower != null)
            {
                TryPlantFlower();
            }
            else
            {
                TryPickUpFlower();
            }
        }
    }

    private void TryPickUpFlower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Sprout"))
            {
                GameObject flower = hit.gameObject;
                SproutAndLightManager sprout = flower.GetComponent<SproutAndLightManager>();
                if (sprout != null && !sprout.isHeld)
                {
                    if (sprout.isPlanted)
                    {
                        // Unplant from any garden
                        GardenSpot garden = flower.transform.parent?.GetComponent<GardenSpot>();
                        if (garden != null)
                        {
                            garden.UnassignFlower();
                        }
                    }

                    sprout.isHeld = true;
                    sprout.isPlanted = false;

                    flower.transform.SetParent(holdPoint);
                    flower.transform.localPosition = Vector3.zero;

                    Collider2D col = flower.GetComponent<Collider2D>();
                    if (col != null) col.enabled = false;

                    heldFlower = flower;
                    Debug.Log($"🌸 Picked up {flower.name}");
                    return;
                }
            }
        }
    }

    private void TryPlantFlower()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.6f);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Garden"))
            {
                GardenSpot garden = hit.GetComponent<GardenSpot>();
                if (garden != null)
                {
                    if (garden.HasPlantedFlower)
                    {
                        // Swap flowers
                        GameObject existing = garden.PickUp();
                        PickUpNewFlower(existing);
                    }

                    heldFlower.transform.SetParent(garden.transform);
                    heldFlower.transform.position = garden.GetPlantingPoint().position;

                    SproutAndLightManager sprout = heldFlower.GetComponent<SproutAndLightManager>();
                    if (sprout != null)
                    {
                        sprout.isPlanted = true;
                        sprout.isHeld = false;
                    }

                    Collider2D col = heldFlower.GetComponent<Collider2D>();
                    if (col != null) col.enabled = true;

                    garden.AssignFlower(heldFlower);

                    Debug.Log($"🪴 Planted {heldFlower.name} in {garden.name}");
                    heldFlower = null;
                    return;
                }
            }
        }

        Debug.Log("❌ No garden nearby to plant.");
    }

    private void PickUpNewFlower(GameObject flower)
    {
        heldFlower = flower;

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
    }
}
