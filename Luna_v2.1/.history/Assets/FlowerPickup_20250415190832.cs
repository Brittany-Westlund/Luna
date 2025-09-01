using UnityEngine;

public class FlowerPickup : MonoBehaviour
{
    [Header("Flower Type")]
    public string flowerType;

    [Header("Assign the visual child (like 'pivot' or sprite root)")]
    public Transform visual;

    [HideInInspector]
    public Vector3 originalVisualScale;

    private bool playerInRange = false;
    private FlowerHolder holder;

    private void Awake()
    {
        if (visual == null) visual = transform; // fallback if not assigned
        originalVisualScale = visual.localScale;
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
