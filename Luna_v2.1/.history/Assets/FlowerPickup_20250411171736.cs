using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public GameObject flowerVisual;
    public string flowerType;

    private bool playerInRange = false;
    private bool nearGarden = false;

    private FlowerHolder holder;
    private Transform gardenSpot;

    private void Update()
    {
        if (playerInRange && holder != null && Input.GetKeyDown(KeyCode.X))
        {
            if (!holder.HasFlower())
            {
                holder.PickUpFlower(this);
            }
            else if (nearGarden)
            {
                Debug.Log("🪴 Attempting to plant...");
                holder.PlantFlower(gardenSpot.position);
                Debug.Log("🌼 Planted " + holder.currentFlowerType + " in garden!");
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Collided with: {other.name}, Tag: {other.tag}");

        if (other.CompareTag("Player"))
        {
            holder = other.GetComponent<FlowerHolder>();
            playerInRange = true;
        }
        else if (other.CompareTag("Garden"))
        {
            nearGarden = true;
            gardenSpot = other.transform;
            Debug.Log("🌱 Garden detected!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            holder = null;
            playerInRange = false;
        }
        else if (other.CompareTag("Garden"))
        {
            nearGarden = false;
            gardenSpot = null;
        }
    }

    public void DisableFlowerVisual()
    {
        FlowerSway sway = GetComponent<FlowerSway>();
        if (sway != null) sway.DisableSwayOnPickup();
        if (flowerVisual != null) flowerVisual.SetActive(false);
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
