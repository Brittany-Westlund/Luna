using UnityEngine;

public class GardenSpot : MonoBehaviour
{
    public GameObject grassObject;
    public Transform plantingPoint;
    private GameObject plantedFlower;
    private SpriteRenderer grassRenderer;

    private void Start()
    {
        if (grassObject != null)
            grassRenderer = grassObject.GetComponent<SpriteRenderer>();

        if (plantingPoint == null)
            plantingPoint = transform;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        FlowerHolder holder = other.GetComponent<FlowerHolder>();
        if (holder == null || !holder.HasFlower) return;

        if (Input.GetKeyDown(KeyCode.X))
        {
            GameObject current = holder.GetHeldFlower();
            if (current == null) return;

            if (plantedFlower != null)
            {
                // Swap
                holder.PickUpFlower(plantedFlower);
            }

            // Plant the new one
            plantedFlower = current;
            plantedFlower.transform.position = plantingPoint.position;
            plantedFlower.transform.SetParent(transform);

            var sprout = plantedFlower.GetComponent<SproutAndLightManager>();
            if (sprout != null)
            {
                sprout.isPlanted = true;
                sprout.isHeld = false;
            }

            var col = plantedFlower.GetComponent<Collider2D>();
            if (col != null) col.enabled = true;

            plantedFlower.SetActive(true);
            holder.DropFlower();
        }
    }

    public GameObject GetPlantedFlower() => plantedFlower;
}
