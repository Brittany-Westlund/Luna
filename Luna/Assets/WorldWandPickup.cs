using UnityEngine;

public class WorldWandPickup : MonoBehaviour
{
    private bool canPickup = false;
    private WandForever playerWand; // cache the Luna’s WandForever

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = true;
            playerWand = other.GetComponent<WandForever>();

            if (playerWand == null)
                Debug.LogWarning("⚠️ Player has no WandForever script!");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            canPickup = false;
            playerWand = null;
        }
    }

    void Update()
    {
        if (canPickup && playerWand != null && Input.GetKeyDown(KeyCode.Q))
        {
            // Unlock the held wand on the player
            playerWand.UnlockWand();

            // Destroy world pickup forever
            Destroy(gameObject);

            Debug.Log("✨ World wand picked up and destroyed forever.");
        }
    }

    void Awake()
    {
        // If Luna already has the wand, don’t spawn this pickup
        if (WandGlobalState.Instance != null && WandGlobalState.Instance.hasWand)
        {
            Destroy(gameObject);
        }
    }
}

