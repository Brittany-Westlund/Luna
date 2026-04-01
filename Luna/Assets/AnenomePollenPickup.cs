using UnityEngine;

public class AnemonePollenPickup : MonoBehaviour
{
    [Header("Effect Duration")]
    public float duration = 10f;

    [Header("Healing")]
    [Range(0f, 1f)]
    [Tooltip("Percent of max health restored over the full duration.")]
    public float healPercentOverDuration = 0.25f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        GameObject player = other.gameObject;

        LunaStatusEffects status = player.GetComponent<LunaStatusEffects>();
        if (status == null)
        {
            status = player.AddComponent<LunaStatusEffects>();
        }

        status.ApplyAnemoneEffect(duration, healPercentOverDuration);

        Debug.Log("🌼 Anemone pollen activated: Slumberdust immunity + heal over time.");

        Destroy(gameObject);
    }
}