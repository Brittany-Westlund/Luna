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

        // Deactivate Foxglove pollen icon if active
        Transform foxgloveIcon = other.transform.Find("FoxglovePollenHoldPoint/FoxglovePollenLuna");
        if (foxgloveIcon != null)
        {
            foxgloveIcon.gameObject.SetActive(false);
        }

        // Activate Anemone pollen icon
        Transform anemoneIcon = other.transform.Find("AnemonePollenHoldPoint/AnemonePollenLuna");
        if (anemoneIcon != null)
        {
            anemoneIcon.gameObject.SetActive(true);
            other.GetComponent<MonoBehaviour>().StartCoroutine(HideIconAfterDelay(anemoneIcon.gameObject, suppressionDuration));
        }
        else
        {
            Debug.LogWarning("Anemone pollen icon not found on player!");
        }

        Destroy(gameObject);
    }

    private IEnumerator HideIconAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (icon != null) icon.SetActive(false);
    }
}
