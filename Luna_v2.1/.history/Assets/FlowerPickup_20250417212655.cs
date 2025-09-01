using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public string flowerType;

    private bool playerInRange = false;
    private FlowerHolder holder;
    private SproutAndLightManager sprout;

    private void Start()
    {
        sprout = GetComponent<SproutAndLightManager>();
    }

    private void Update()
    {
        if (!playerInRange || holder == null || holder.HasFlower) return;

        if (Input.GetKeyDown(KeyCode.X))
        {
            // Only allow pickup if this flower is loose in the world
            if (sprout == null || !sprout.isPlanted)
            {
                Debug.Log($"🌸 Picking up loose flower: {gameObject.name}");
                holder.PickUpFlowerInstance(this.gameObject);
            }
            else
            {
                Debug.Log($"🪴 {gameObject.name} is planted and cannot be picked up directly.");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        holder = other.GetComponent<FlowerHolder>();
        playerInRange = holder != null;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<FlowerHolder>() == holder)
        {
            holder = null;
            playerInRange = false;
        }
    }
}
