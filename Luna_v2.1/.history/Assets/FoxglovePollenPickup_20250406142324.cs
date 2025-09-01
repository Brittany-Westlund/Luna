using UnityEngine;

public class FoxglovePollenPickup : MonoBehaviour
{
    public float pollenDuration = 30f; // How long before the effect expires

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Find the icon container
            Transform foxgloveIcon = other.transform.Find("FoxglovePollenLuna");
            if (foxgloveIcon != null)
            {
                foxgloveIcon.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("🧚 FoxglovePollenLuna object not found on Player!");
            }

            // 2. Start coroutine to expire the effect after time
            FoxglovePollenEffect effect = other.GetComponent<FoxglovePollenEffect>();
            if (effect != null)
            {
                effect.ActivatePollenEffect(pollenDuration);
            }

            // 3. Destroy the pollen pickup
            Destroy(gameObject);
        }
    }
}
