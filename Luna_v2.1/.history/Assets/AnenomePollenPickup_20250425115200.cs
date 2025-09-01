// AnemonePollenPickup.cs
using UnityEngine;
using System.Collections;

public class AnemonePollenPickup : MonoBehaviour
{
    public float suppressionDuration = 10f;
    public AudioSource collectSFXSource;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        collectSFXSource.Play();

        LunaHealthManager healthDecay = other.GetComponent<LunaHealthManager>();
        if (healthDecay != null)
        {
            healthDecay.SuppressDecay(suppressionDuration);
            Debug.Log("🌼 Anemone pollen activated.");
        }

        Transform iconTransform = other.transform.Find("AnemonePollenHoldPoint/AnemonePollenLuna");
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(true);
            other.GetComponent<MonoBehaviour>().StartCoroutine(HideIconAfterDelay(iconTransform.gameObject, suppressionDuration));
        }
        else
        {
            Debug.LogWarning("AnemonePollenLuna not found under Player.");
        }
        
        Destroy(gameObject);
    }

    private IEnumerator HideIconAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (icon != null)
        {
            icon.SetActive(false);
        }
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
