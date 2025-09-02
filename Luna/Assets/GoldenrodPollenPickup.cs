using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;

public class GoldenrodPollenPickup : MonoBehaviour
{
    [Range(0f, 1f)]
    public float healPercentage = 0.25f; // Heals 25% of max health
    public float suppressionDuration = 3f;
    public AudioSource collectSFXSource;

    // Reference to the UI object for this pollen
    private GameObject _goldenrodPollenUIObject;

    void Start()
{
    Debug.Log("GoldenrodPollenPickup Start running!");
    var avatarHead = GameObject.Find("AvatarHead");
    if (avatarHead != null)
    {
        Debug.Log("AvatarHead found! Children:");
        foreach (Transform child in avatarHead.transform)
            Debug.Log(child.name);

        _goldenrodPollenUIObject = avatarHead.transform.Find("GoldenrodPollenUIObject")?.gameObject;
        if (_goldenrodPollenUIObject == null)
            Debug.LogWarning("Goldenrod pollen UI object not found!");
        else
            Debug.Log("Goldenrod pollen UI object found!");
    }
    else
    {
        Debug.LogWarning("AvatarHead not found!");
    }
}

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        
        // Hide all SpriteRenderers on this object and its children
        foreach (var sr in GetComponentsInChildren<SpriteRenderer>())
            sr.enabled = false;
       
        collectSFXSource.Play();
        
        
        Transform iconTransform = other.transform.Find("GoldenrodPollenHoldPoint/GoldenrodPollenLuna");
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(true);
            other.GetComponent<MonoBehaviour>().StartCoroutine(HideIconAfterDelay(iconTransform.gameObject, suppressionDuration));
        }
        else
        {
            Debug.LogWarning("GoldenrodPollenLuna not found under Player.");
        }
        
        
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

        // Show the goldenrod pollen UI icon for 3 seconds
        if (_goldenrodPollenUIObject != null)
            StartCoroutine(ShowForSeconds(_goldenrodPollenUIObject, 3f));

        Destroy(gameObject, collectSFXSource.clip.length);
    }

    private IEnumerator HideIconAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (icon != null)
        {
            icon.SetActive(false);
        }
    }

    private IEnumerator ShowForSeconds(GameObject go, float seconds)
    {
        Debug.Log("Enabling Goldenrod pollen UI object!");
        go.SetActive(true);
        yield return new WaitForSeconds(seconds);
        go.SetActive(false);
        Debug.Log("Disabling Goldenrod pollen UI object!");
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
