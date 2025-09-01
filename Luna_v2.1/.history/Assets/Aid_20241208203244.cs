using UnityEngine;

public class Aid : MonoBehaviour
{
    public LayerMask sproutLayer; // Layer to detect sprouts
    public float aidRadius = 1.0f; // Radius to detect sprouts for aid
    private PresentSpore presentSpore; // Reference to the PresentSpore script

    private void Start()
    {
        // Find and reference the PresentSpore script on Luna
        presentSpore = GetComponent<PresentSpore>();
        if (presentSpore == null)
        {
            Debug.LogError("PresentSpore component not found on Luna! Ensure it is attached.");
        }
    }

    private void Update()
    {
        // Aid sprouts when the Aid key is pressed and a spore is attached
        if (Input.GetButtonDown("Aid") && presentSpore != null && presentSpore.HasSporeAttached)
        {
            AidSprout();
        }
    }

    private void AidSprout()
    {
        // Detect nearby sprouts within the aid radius
        Collider2D sproutCollider = Physics2D.OverlapCircle(transform.position, aidRadius, sproutLayer);

        if (sproutCollider != null)
        {
            AidingSprouts sprout = sproutCollider.GetComponent<AidingSprouts>();
            if (sprout != null)
            {
                sprout.Grow(); // Trigger the sprout growth
                presentSpore.DestroyAttachedSpore(); // Remove the attached spore after aiding
                Debug.Log("Aided a sprout!");
            }
        }
        else
        {
            Debug.Log("No sprout nearby to aid.");
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualize the aid radius in the editor for debugging
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, aidRadius);
    }
}
