using UnityEngine;

public class ButtercupPollenPickup : MonoBehaviour
{
    public int fatigueReductionAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Butterfly")) // <- Butterfly must have this tag
        {
            ButterflyFatigue fatigue = other.GetComponent<ButterflyFatigue>();

            if (fatigue != null)
            {
                fatigue.ReduceFatigue(fatigueReductionAmount);
                Destroy(gameObject); // ✅ Pollen disappears after use
            }
        }
    }
}
