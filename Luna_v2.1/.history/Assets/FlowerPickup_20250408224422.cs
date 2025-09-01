using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public GameObject flowerVisual;
    public string flowerType;

    private bool playerInRange = false;
    private FlowerHolder holder;

    private void Update()
    {
        if (playerInRange && holder != null && Input.GetKeyDown(KeyCode.X))
        {
            if (!holder.HasFlower())
            {
                holder.PickUpFlower(this);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            holder = other.GetComponent<FlowerHolder>();
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            holder = null;
            playerInRange = false;
        }
    }

    public void DisableFlowerVisual()
    {
        // 👇 Tell the sway script to stop running coroutines
        FlowerSway sway = GetComponent<FlowerSway>();
        if (sway != null)
        {
            sway.isBeingPickedUp = true;
        }

        if (flowerVisual != null)
        {
            flowerVisual.SetActive(false);
        }
    }

    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
