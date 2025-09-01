using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;

public class GoldenrodPollenPickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float healPercentage = 0.25f; // Heals 25% of max health
    public AudioSource collectSFXSource;

    // Reference to the UI object for this pollen
    private GameObject _goldenrodPollenUIObject;

    void Start()
    {
        // Update the path and object name as needed!
        _goldenrodPollenUIObject = GameObject.Find("Canvas/UICamera/HUD/AvatarHead/GoldenrodPollenUIObject");
        if (_goldenrodPollenUIObject == null)
            Debug.LogWarning("Goldenrod pollen UI object not found!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Hide all SpriteRenderers on this object and its children
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;
       
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

        // Show the goldenrod pollen UI icon for 2 seconds
        if (_goldenrodPollenUIObject != null)
            StartCoroutine(ShowForSeconds(_goldenrodPollenUIObject, 2f));

        Destroy(gameObject, collectSFXSource.clip.length);
    }

    private IEnumerator ShowForSeconds(GameObject go, float seconds)
    {
        go.SetActive(true);
        yield return new WaitForSeconds(seconds);
        go.SetActive(false);
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
