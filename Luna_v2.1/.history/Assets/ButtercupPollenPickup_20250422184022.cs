using UnityEngine;

public class ButtercupPollenPickup : MonoBehaviour
{
    public int fatigueReductionAmount = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Butterfly"))
        {
            ButterflyFatigue fatigue = other.GetComponent<ButterflyFatigue>();
            if (fatigue != null)
            {
                fatigue.ReduceFatigue(fatigueReductionAmount);
            }

            ButterflyFlyHandler flyHandler = other.GetComponent<ButterflyFlyHandler>();
            if (flyHandler != null)
            {
                flyHandler.ShowButtercupPollenIcon(); // 👈 show icon
            }

            Destroy(gameObject); // pollen disappears
        }
    }
}
