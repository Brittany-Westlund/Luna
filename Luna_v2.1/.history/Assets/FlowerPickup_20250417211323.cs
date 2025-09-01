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
            Debug.Log($"Trying to pick up: {gameObject.name}");

            if (sprout != null)
            {
                // 🔧 Force state change on pickup
                sprout.isPlanted = false;
                sprout.isHeld = true;

                holder.PickUpFlowerInstance(this.gameObject);
                Debug.Log($"✅ Picked up: {gameObject.name}");
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} has no SproutAndLightManager");
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
