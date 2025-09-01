using UnityEngine;

public class AnemonePollenPickup : MonoBehaviour
{
    public float suppressionDuration = 10f;
    public GameObject anemoneIcon; // optional, drag the Luna icon prefab

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            LunaHealthManager healthManager = other.GetComponent<LunaHealthManager>();
            if (healthManager != null)
            {
                healthManager.SuppressDecay(suppressionDuration);
            }

            if (anemoneIcon != null)
            {
                anemoneIcon.SetActive(true);
                other.GetComponent<MonoBehaviour>().StartCoroutine(HideIconAfterDelay(anemoneIcon, suppressionDuration));
            }

            Destroy(gameObject);
        }
    }

    private System.Collections.IEnumerator HideIconAfterDelay(GameObject icon, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (icon != null) icon.SetActive(false);
    }
}
