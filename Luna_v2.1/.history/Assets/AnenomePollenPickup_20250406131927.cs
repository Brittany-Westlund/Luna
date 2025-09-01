using UnityEngine;

public class AnemonePollenPickup : MonoBehaviour
{
    public float suppressionDuration = 10f; // Duration of suppression

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LunaHealthManager healthDecay = other.GetComponent<LunaHealthManager>();
            if (healthDecay != null)
            {
                healthDecay.SuppressDecay(suppressionDuration);
                Debug.Log("🌼 Health decay suppressed by Anemone Pollen!");
            }

            Destroy(gameObject);
        }
    }
}
