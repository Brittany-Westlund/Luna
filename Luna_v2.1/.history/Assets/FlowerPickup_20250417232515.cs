using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public string flowerType = "Unknown";

    [HideInInspector] public bool isPickedUp = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isPickedUp)
        {
            FlowerHolder holder = other.GetComponent<FlowerHolder>();
            if (holder != null && !holder.HasFlower)
            {
                holder.PickUpFlower(gameObject);
            }
        }
    }
}
