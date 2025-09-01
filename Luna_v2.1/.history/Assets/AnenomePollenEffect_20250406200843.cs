using UnityEngine;

public class AnemonePollenEffect : MonoBehaviour
{
    public float duration = 10f;
    public string iconObjectName = "AnemonePollenLuna"; // Must match the icon's name in Luna
    private GameObject icon;
    private bool isActive = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActive)
        {
            Transform iconTransform = other.transform.Find(iconObjectName);
            if (iconTransform != null)
            {
                icon = iconTransform.gameObject;
                icon.SetActive(true);
                isActive = true;
                Debug.Log("🌿 Anemone Pollen collected — decay suppressed!");

                LunaHealthManager healthManager = other.GetComponent<LunaHealthManager>();
                if (healthManager != null)
                {
                    healthManager.SuppressDecay(duration);
                }

                StartCoroutine(EndEffectAfterTime());
            }

            Destroy(gameObject); // Remove the pollen pickup
        }
    }

    private System.Collections.IEnumerator EndEffectAfterTime()
    {
        yield return new WaitForSeconds(duration);

        if (icon != null)
        {
            icon.SetActive(false);
            Debug.Log("🌿 Anemone effect ended — icon hidden.");
        }

        isActive = false;
    }
}
