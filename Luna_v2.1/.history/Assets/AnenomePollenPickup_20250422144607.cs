// AnemonePollenPickup.cs
using System.Collections;
using UnityEngine;

public class AnemonePollenPickup : MonoBehaviour
{
    public float suppressionDuration = 10f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Suppress health decay
        LunaHealthManager healthDecay = other.GetComponent<LunaHealthManager>();
        if (healthDecay != null)
        {
            healthDecay.SuppressDecay(suppressionDuration);
            Debug.Log("🌼 Health decay suppressed by Anemone Pollen!");
        }

        // Turn on Anemone icon, turn off Foxglove icon
        Transform anemoneIcon = other.transform.Find("AnemonePollenHoldPoint/AnemonePollenLuna");
        Transform foxgloveIcon = other.transform.Find("FoxglovePollenHoldPoint/FoxglovePollenLuna");

        if (anemoneIcon != null)
        {
            anemoneIcon.gameObject.SetActive(true);
            other.GetComponent<MonoBehaviour>().StartCoroutine(HideIconAfterDelay(anemoneIcon.gameObject, suppressionDuration));
        }
        else
        {
            Debug.LogWarning("Anemone pollen icon not found on player!");
        }

        if (foxgloveIcon != null)
        {
            foxgloveIcon.gameObject.SetActive(false);
        }

        Destroy(gameObject);
    }

    private IEnumerator HideIconAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (icon != null) icon.SetActive(false);
    }
}
