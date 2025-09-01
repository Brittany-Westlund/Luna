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

            if (sprout != null && !sprout.isPlanted)
            {
                Debug.Log($"Picking up: {gameObject.name}");
                holder.PickUpFlowerInstance(this.gameObject);
            }
            else
            {
                Debug.Log($"{gameObject.name} isPlanted = {sprout?.isPlanted}");
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
