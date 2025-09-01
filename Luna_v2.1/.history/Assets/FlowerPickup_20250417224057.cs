using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    private bool playerInRange = false;
    private FlowerHolder holder;
    private SproutAndLightManager sprout;
    public string flowerType = "Unknown";


    private void Start()
    {
        sprout = GetComponent<SproutAndLightManager>();
    }

    private void Update()
    {
        if (!playerInRange || holder == null) return;

        if (Input.GetKeyDown(KeyCode.X) && !holder.HasFlower)
        {
            Debug.Log($"🌸 Picking up flower: {gameObject.name}");
            holder.PickUpFlower(gameObject);
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
            playerInRange = false;
            holder = null;
        }
    }

    public class FlowerPickup : MonoBehaviour
    {
        public string flowerType = "Unknown";
    }

}
