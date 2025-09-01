using UnityEngine;

public class FoxglovePollenPickup : MonoBehaviour
{
    public float pollenDuration = 30f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Deactivate Anemone pollen icon if active
        Transform anemoneIcon = other.transform.Find("AnemonePollenHoldPoint/AnemonePollenLuna");
        if (anemoneIcon != null)
        {
            anemoneIcon.gameObject.SetActive(false);
        }

        // Activate Foxglove pollen icon
        Transform foxgloveIcon = other.transform.Find("FoxglovePollenHoldPoint/FoxglovePollenLuna");
        if (foxgloveIcon != null)
        {
            foxgloveIcon.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("🧚 FoxglovePollenLuna object not found on Player!");
        }

        // Start effect logic
        FoxglovePollenEffect effect = other.GetComponent<FoxglovePollenEffect>();
        if (effect != null)
        {
            effect.ActivatePollenEffect(pollenDuration);
        }

        Destroy(gameObject);
    }
}
