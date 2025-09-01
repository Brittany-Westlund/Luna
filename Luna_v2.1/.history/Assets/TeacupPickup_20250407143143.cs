using UnityEngine;

public class TeacupPickup : MonoBehaviour
{
    public float pickupRange = 1.2f; // Optional distance check
    private bool hasBeenPickedUp = false;

    void Update()
    {
        if (hasBeenPickedUp) return;

        GameObject luna = GameObject.FindGameObjectWithTag("Player");
        if (luna == null) return;

        float distance = Vector3.Distance(transform.position, luna.transform.position);
        if (distance <= pickupRange && Input.GetKeyDown(KeyCode.E))
        {
            AttachToLuna(luna);
        }
    }

    private void AttachToLuna(GameObject luna)
    {
        hasBeenPickedUp = true;

        TeacupInventory inventory = luna.GetComponent<TeacupInventory>();
        if (inventory != null)
        {
            inventory.ReceiveTeacup(gameObject);
        }
        else
        {
            Debug.LogWarning("No TeacupInventory found on Luna.");
        }
    }
}
