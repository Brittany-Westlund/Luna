using UnityEngine;

public class WildSporePickup : MonoBehaviour
{
    public PresentSpore presentSpore;
    public bool IsHoldingSpore => presentSpore != null && presentSpore.HasSporeAttached;


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G) && presentSpore != null && !presentSpore.HasSporeAttached)
        {
            HandlePickup();
        }
    }

    private void HandlePickup()
    {
        Collider2D sporeCollider = Physics2D.OverlapCircle(transform.position, 0.5f, LayerMask.GetMask("Spore"));
        if (sporeCollider != null)
        {
            Debug.Log("Picking up wild spore.");
            Destroy(sporeCollider.gameObject); // Destroy wild spore
            presentSpore.CreateSpore(); // Create attached spore
        }
        else
        {
            Debug.Log("No spore nearby to pick up.");
        }
    }
}
