using UnityEngine;

public class MotePersistence : MonoBehaviour
{
    [SerializeField] private string collectibleID = "Mote01";

    void Awake()
    {
        // If it’s already collected, don’t spawn this mote again
        if (CollectibleManager.Instance != null &&
            CollectibleManager.Instance.HasCollected(collectibleID))
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        // This runs when the mote is actually collected and destroyed by the wand
        if (CollectibleManager.Instance != null &&
            !CollectibleManager.Instance.HasCollected(collectibleID))
        {
            CollectibleManager.Instance.MarkCollected(collectibleID);
            Debug.Log($"💾 Mote '{collectibleID}' marked as collected.");
        }
    }
}
