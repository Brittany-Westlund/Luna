// FoxglovePollenPickup.cs
using UnityEngine;

public class FoxglovePollenPickup : MonoBehaviour
{
    public float pollenDuration = 30f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Turn on Foxglove icon, turn off Anemone icon
        Transform foxgloveIcon = other.transform.Find("FoxglovePollenHoldPoint/FoxglovePollenLuna");
        Transform anemoneIcon  = other.transform.Find("AnemonePollenHoldPoint/AnemonePollenLuna");

        if (foxgloveIcon != null)
        {
            foxgloveIcon.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogWarning("🧚 FoxglovePollenLuna object not found on Player!");
        }

        if (anemoneIcon != null)
        {
            anemoneIcon.gameObject.SetActive(false);
        }

        // Activate Foxglove effect
        FoxglovePollenEffect effect = other.GetComponent<FoxglovePollenEffect>();
        if (effect != null)
        {
            effect.ActivatePollenEffect(pollenDuration);
        }

        Destroy(gameObject);
    }
}