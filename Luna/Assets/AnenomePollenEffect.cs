using System.Collections;
using UnityEngine;

public class AnemonePollenEffect : MonoBehaviour
{
    public float duration = 10f;
    public string iconPath = "AnemonePollenHoldPoint/AnemonePollenLuna";

    private GameObject iconInstance;
    private bool isEffectActive = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player") || isEffectActive) return;

        Transform iconTransform = other.transform.Find(iconPath);
        if (iconTransform != null)
        {
            iconInstance = iconTransform.gameObject;
            iconInstance.SetActive(true);
        }
        else
        {
            Debug.LogWarning("❗ Anemone pollen icon not found on player.");
        }

        LunaHealthManager healthManager = other.GetComponent<LunaHealthManager>();
        if (healthManager != null)
        {
            healthManager.SuppressDecay(duration);
        }

        isEffectActive = true;
        StartCoroutine(DisableIconAfterDuration());
        Destroy(gameObject);
    }

    private IEnumerator DisableIconAfterDuration()
    {
        yield return new WaitForSeconds(duration);

        if (iconInstance != null)
        {
            iconInstance.SetActive(false);
            Debug.Log("🌼 Anemone pollen icon auto-disabled.");
        }

        isEffectActive = false;
    }
}
