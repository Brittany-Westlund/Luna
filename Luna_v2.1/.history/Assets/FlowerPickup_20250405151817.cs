using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public GameObject flowerVisual; // Optional visual
    public string flowerType; // Name/type ("Moonpetal", etc.)

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FlowerHolder holder = other.GetComponent<FlowerHolder>();
            if (holder != null && !holder.HasFlower())
            {
                holder.PickUpFlower(this);
            }
        }
    }

    public void DisableFlowerVisual()
    {
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
