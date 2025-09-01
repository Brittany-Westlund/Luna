using UnityEngine;
using MoreMountains.Tools;

public class LightMotePickup : MonoBehaviour
{
    public float lightRestorePercent = 0.25f; // 25% restore

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            MMProgressBar lightBar = FindObjectOfType<MMProgressBar>();

            if (lightBar != null)
            {
                float current = lightBar.BarProgress;
                float target = Mathf.Clamp01(current + lightRestorePercent);
                lightBar.UpdateBar01(target);

                Debug.Log("🌕 Light mote collected — light increased!");
            }
            else
            {
                Debug.LogWarning("No MMProgressBar found in the scene.");
            }

            Destroy(gameObject);
        }
    }
}
