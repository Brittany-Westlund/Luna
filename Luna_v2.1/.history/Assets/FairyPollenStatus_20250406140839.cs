using UnityEngine;

public class FoxglovePollenPickup : MonoBehaviour
{
    public float duration = 30f; // Optional effect duration

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            // 1. Grant the ability
            FairyPollenStatus pollenStatus = other.GetComponent<FairyPollenStatus>();
            if (pollenStatus != null)
            {
                pollenStatus.GiveFairypetalPollen();
                Debug.Log("🧚‍♀️ Foxglove pollen acquired!");
                pollenStatus.Invoke("ClearFairypetalPollen", duration);
            }

            // 2. Activate the icon on Luna
            Transform iconTransform = other.transform.Find("FoxglovePollenIcon");
            if (iconTransform != null)
            {
                iconTransform.gameObject.SetActive(true);
            }
            else
            {
                Debug.LogWarning("🧩 Could not find 'FoxglovePollenIcon' on Luna.");
            }

            Destroy(gameObject);
        }
    }
}
