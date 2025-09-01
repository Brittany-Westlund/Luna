using System.Collections;
using UnityEngine;

public class AnemonePollenPickup : MonoBehaviour
{
    public float suppressionDuration = 10f; // Duration of suppression
    public string iconObjectName = "AnemonePollenLuna"; // The name of the icon GameObject in Luna's hierarchy

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        LunaHealthManager healthDecay = other.GetComponent<LunaHealthManager>();
        if (healthDecay != null)
        {
            healthDecay.SuppressDecay(suppressionDuration);
            Debug.Log("🌼 Health decay suppressed by Anemone Pollen!");
        }

        // Try to find and activate the icon
        Transform iconTransform = other.transform.Find(iconObjectName);
        if (iconTransform != null)
        {
            GameObject iconObject = iconTransform.gameObject;
            iconObject.SetActive(true);
            other.GetComponent<MonoBehaviour>().StartCoroutine(HideIconAfterDelay(iconObject, suppressionDuration));
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
