using UnityEngine;

public class TeacupPickup : MonoBehaviour
{
    private bool isPlayerNearby = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
        }
    }

    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(KeyCode.E)) // Customize interaction key as needed
        {
            TeacupInventory inventory = GameObject.FindWithTag("Player")?.GetComponent<TeacupInventory>();
            if (inventory != null)
            {
                inventory.ReceiveTeacup(gameObject);
            }
        }
    }
}
