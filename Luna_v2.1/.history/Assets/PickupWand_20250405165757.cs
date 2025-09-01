using UnityEngine;

public class PickupWand : MonoBehaviour
{
    [Header("Pickup Settings")]
    public Transform holdPoint; // Where the wand should be placed when picked up (on Luna)
    public KeyCode pickupKey = KeyCode.E;
    public string playerTag = "Player";

    private bool canPickup = false;
    private GameObject player;
    private bool isPickedUp = false;

    private void Update()
    {
        if (canPickup && !isPickedUp && Input.GetKeyDown(pickupKey))
        {
            PickUpWand();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            player = other.gameObject;
            canPickup = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            canPickup = false;
        }
    }

    private void PickUpWand()
    {
        if (holdPoint == null && player != null)
        {
            // Try to find a hold point on the player
            holdPoint = player.transform.Find("WandHoldPoint");
        }

        if (holdPoint != null)
        {
            transform.SetParent(holdPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            // Disable the collider so it doesn't re-trigger
            Collider2D col = GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            isPickedUp = true;
        }
        else
        {
            Debug.LogWarning("No hold point found for wand!");
        }
    }
}
