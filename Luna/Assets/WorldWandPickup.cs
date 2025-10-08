using UnityEngine;

public class WorldWandPickup : MonoBehaviour
{
    [SerializeField] private string collectibleID = "Wand01";

    void Awake()
    {
        // 🌙 If wand was already collected in save file, destroy pickup immediately
        if (CollectibleManager.Instance != null &&
            CollectibleManager.Instance.HasCollected(collectibleID))
        {
            Debug.Log("🪶 Wand already collected — pickup destroyed.");
            Destroy(gameObject);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        var playerWand = other.GetComponent<WandForever>();
        if (playerWand == null)
        {
            Debug.LogWarning("⚠️ Player has no WandForever script!");
            return;
        }

        // 🌕 Activate Luna’s wand
        playerWand.UnlockWand();

        // 💾 Save to collectibles.json
        if (CollectibleManager.Instance != null)
        {
            CollectibleManager.Instance.MarkCollected(collectibleID);
            Debug.Log("✨ Wand collected and saved to collectibles.json!");
        }

        Destroy(gameObject);
    }
}


/* using UnityEngine;

public class WorldWandPickup : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // ✅ Find the WandForever script on Luna
        var playerWand = other.GetComponent<WandForever>();
        if (playerWand == null)
        {
            Debug.LogWarning("⚠️ Player has no WandForever script!");
            return;
        }

        // 🌕 Turn on Luna’s held wand
        playerWand.UnlockWand();

        // 🔥 Remove this pickup from the world
        Destroy(gameObject);

        Debug.Log("✨ Luna collided with the wand pickup — activated her child wand and destroyed pickup.");
    }

    void Awake()
    {
        // 🧹 Optional safeguard: don’t spawn if already has wand
        if (WandGlobalState.Instance != null && WandGlobalState.Instance.hasWand)
            Destroy(gameObject);
    }
}


/* using UnityEngine;

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
*/
