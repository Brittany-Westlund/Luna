using UnityEngine;
using MoreMountains.Tools;

public class LightMotePickup : MonoBehaviour
{
    [SerializeField] private string collectibleID = "Mote01"; // unique ID for saving
    public float lightRestorePercent = 0.25f;
    public string lightBarObjectName = "LightBar"; // Make sure this matches your light bar's GameObject name

    void Awake()
    {
        // 🧩 Don’t spawn if already collected
        if (CollectibleManager.Instance != null &&
            CollectibleManager.Instance.HasCollected(collectibleID))
        {
            Destroy(gameObject);
            return;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject barObj = GameObject.Find(lightBarObjectName);

        if (barObj != null)
        {
            MMProgressBar lightBar = barObj.GetComponent<MMProgressBar>();

            if (lightBar != null)
            {
                float current = lightBar.BarProgress;
                float target = Mathf.Clamp01(current + lightRestorePercent);
                lightBar.UpdateBar01(target);
                Debug.Log("🌕 Light mote collected — light increased!");
            }
            else
            {
                Debug.LogWarning("LightBar object found but MMProgressBar component missing.");
            }
        }
        else
        {
            Debug.LogWarning($"Could not find GameObject named '{lightBarObjectName}' in scene.");
        }

        // 🪄 NEW: mark it collected before destroying
        if (CollectibleManager.Instance != null)
            CollectibleManager.Instance.MarkCollected(collectibleID);

        Destroy(gameObject);
    }
}


/* using UnityEngine;
using MoreMountains.Tools;

public class LightMotePickup : MonoBehaviour
{
    public float lightRestorePercent = 0.25f;
    public string lightBarObjectName = "LightBar"; // Make sure this matches your light bar's GameObject name

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameObject barObj = GameObject.Find(lightBarObjectName);

            if (barObj != null)
            {
                MMProgressBar lightBar = barObj.GetComponent<MMProgressBar>();

                if (lightBar != null)
                {
                    float current = lightBar.BarProgress;
                    float target = Mathf.Clamp01(current + lightRestorePercent);
                    lightBar.UpdateBar01(target);
                    Debug.Log("🌕 Light mote collected — light increased!");
                }
                else
                {
                    Debug.LogWarning("LightBar object found but MMProgressBar component missing.");
                }
            }
            else
            {
                Debug.LogWarning($"Could not find GameObject named '{lightBarObjectName}' in scene.");
            }

            Destroy(gameObject);
        }
    }
}
*/