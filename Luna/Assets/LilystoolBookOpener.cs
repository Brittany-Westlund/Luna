using UnityEngine;

public class OpenBookTrigger : MonoBehaviour
{
    [Header("Interaction")]
    public KeyCode openKey = KeyCode.R;

    [Header("Distance Settings")]
    public float interactionRadius = 2f;

    private Transform player;
    private SpriteRenderer bookRenderer;

    void Awake()
    {
        // Get sprite renderer
        bookRenderer = GetComponent<SpriteRenderer>();

        if (bookRenderer == null)
        {
            Debug.LogWarning("OpenBookTrigger: No SpriteRenderer found.");
            return;
        }

        // Start hidden
        bookRenderer.enabled = false;

        // Find player automatically
        GameObject p = GameObject.FindGameObjectWithTag("Player");

        if (p != null)
            player = p.transform;
        else
            Debug.LogWarning("OpenBookTrigger: Player with tag 'Player' not found.");
    }

    void Update()
    {
        if (player == null) return;

        float distance = Vector2.Distance(player.position, transform.position);
        bool playerInRange = distance <= interactionRadius;

        // Toggle book
        if (playerInRange && Input.GetKeyDown(openKey))
        {
            bookRenderer.enabled = !bookRenderer.enabled;
        }

        // Auto close when leaving radius
        if (!playerInRange && bookRenderer.enabled)
        {
            bookRenderer.enabled = false;
        }
    }

    // Draw interaction radius in editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }

    public void ForceClose()
    {
        if (bookRenderer != null)
        {
            bookRenderer.enabled = false;
        }
    }
}