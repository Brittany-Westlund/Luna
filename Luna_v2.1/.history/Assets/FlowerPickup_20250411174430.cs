using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    [Header("Flower Settings")]
    public GameObject flowerVisual;
    public string flowerType;

    private bool playerInRange = false;
    private FlowerHolder holder;

    private void Update()
    {
        if (!playerInRange || holder == null) return;

        if (Input.GetKeyDown(KeyCode.X) && !holder.HasFlower())
        {
            holder.PickUpFlower(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        holder = other.GetComponent<FlowerHolder>();
        if (holder != null)
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (other.GetComponent<FlowerHolder>() == holder)
        {
            holder = null;
            playerInRange = false;
        }
    }

    /// <summary>
    /// Disables the visual and swaying effect when the flower is picked.
    /// </summary>
    public void DisableFlowerVisual()
    {
        FlowerSway sway = GetComponent<FlowerSway>();
        if (sway != null)
        {
            sway.DisableSwayOnPickup();
        }

        if (flowerVisual != null)
        {
            flowerVisual.SetActive(false);
        }
    }

    /// <summary>
    /// Destroys the flower GameObject.
    /// </summary>
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
