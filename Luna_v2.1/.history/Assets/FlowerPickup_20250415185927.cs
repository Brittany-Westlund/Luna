using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public string flowerType;

    [HideInInspector]
    public Vector3 originalScale;

    private bool playerInRange = false;
    private FlowerHolder holder;

    private void Awake()
    {
        originalScale = transform.localScale;
    }

    private void Update()
    {
        if (!playerInRange || holder == null) return;

        if (Input.GetKeyDown(KeyCode.X) && !holder.HasFlower())
        {
            holder.PickUpFlowerInstance(this.gameObject);
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
