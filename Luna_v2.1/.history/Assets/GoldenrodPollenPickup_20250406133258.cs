using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

public class GoldenrodPollenPickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float healPercentage = 0.25f; // Heals 25% of max health
    public MMProgressBar healthBar;      // Optional: assign this if you want to update manually

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Health health = other.GetComponent<Health>();
            if (health != null)
            {
                float healAmount = health.MaximumHealth * healPercentage;
                float newHealth = Mathf.Min(health.CurrentHealth + healAmount, health.MaximumHealth);
                health.SetHealth(newHealth, gameObject);
                Debug.Log($"🌼 Goldenrod healed Luna for {healAmount} (new health: {newHealth})");

                // Update MMProgressBar if it's assigned
                if (healthBar != null)
                {
                    healthBar.UpdateBar(newHealth, 0f, health.MaximumHealth);
                }
                else
                {
                    // Try auto-detecting a bar on Luna
                    MMProgressBar autoBar = other.GetComponentInChildren<MMProgressBar>();
                    if (autoBar != null)
                    {
                        autoBar.UpdateBar(newHealth, 0f, health.MaximumHealth);
                    }
                }
            }

            Destroy(gameObject);
        }
    }
}
