using UnityEngine;

public class TeaRosePollenPickup : MonoBehaviour
{
    public float tempSpeedBoost = 2f;           // Temporary extra speed
    public int requiredForPermanent = 25;       // How many pickups for permanent bonus

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Butterfly"))
        {
            ButterflyFlyHandler flyHandler = other.GetComponent<ButterflyFlyHandler>();
            if (flyHandler != null)
            {
                flyHandler.ApplyTeaRosePollen(tempSpeedBoost, requiredForPermanent);
            }

            Destroy(gameObject); // Remove pollen after pickup
        }
    }
}
