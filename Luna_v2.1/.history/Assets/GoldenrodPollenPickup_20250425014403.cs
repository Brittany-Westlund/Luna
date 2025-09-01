using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

public class GoldenrodPollenPickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float healPercentage = 0.25f; // Heals 25% of max health
    public AudioSource collectSFXSource;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        collectSFXSource.Play();
        
        Health health = other.GetComponent<Health>();
        if (health != null)
        {
            float healAmount = health.MaximumHealth * healPercentage;
            float newHealth = Mathf.Min(health.CurrentHealth + healAmount, health.MaximumHealth);
            health.SetHealth(newHealth, gameObject);
            Debug.Log($"🌼 Goldenrod healed Luna for {healAmount} (new health: {newHealth})");

            // Try to auto-find MMProgressBar
            MMProgressBar bar = other.GetComponentInChildren<MMProgressBar>();
            if (bar != null)
            {
                bar.UpdateBar(newHealth, 0f, health.MaximumHealth);
            }
        }

        Destroy(gameObject, collectSFXSource.clip.length);
    }

    public void PlayPickupSFX()
    {
        Debug.Log($"PlayPickupSFX called on {gameObject.name}");
        if (collectSFXSource)
        {
            collectSFXSource.Play();
        }
    }
}
