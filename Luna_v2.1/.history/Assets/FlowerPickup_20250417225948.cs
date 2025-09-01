using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public string flowerType = "Unknown";
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
            Debug.Log($"🌸 Picking up flower: {gameObject.name}");
            holder.PickUpFlower(gameObject);
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
        if (other.CompareTag("Player") && other.GetComponent<FlowerHolder>() == holder)
        {
            playerInRange = false;
            holder = null;
        }
    }
}
