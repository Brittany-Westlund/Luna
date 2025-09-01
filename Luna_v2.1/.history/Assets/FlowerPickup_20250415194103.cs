using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    public string flowerType;

    private FlowerHolder holder;
    private bool inRange = false;

    void Update()
    {
        if (inRange && holder != null && Input.GetKeyDown(KeyCode.X))
        {
            if (!holder.HasFlower())
            {
                holder.PickUpFlowerInstance(this.gameObject);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            holder = other.GetComponent<FlowerHolder>();
            inRange = holder != null;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && other.GetComponent<FlowerHolder>() == holder)
        {
            inRange = false;
            holder = null;
        }
    }
}
