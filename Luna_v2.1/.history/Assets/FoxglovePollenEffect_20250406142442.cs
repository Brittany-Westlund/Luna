using UnityEngine;

public class FoxglovePollenEffect : MonoBehaviour
{
    public GameObject foxglovePollenIcon; // Assign "FoxglovePollenLuna" manually if needed
    private bool isActive = false;

    public void ActivatePollenEffect(float duration)
    {
        isActive = true;

        if (foxglovePollenIcon != null)
        {
            foxglovePollenIcon.SetActive(true);
        }

        StopAllCoroutines(); // In case effect is already running
        StartCoroutine(ExpirePollen(duration));
    }

    private System.Collections.IEnumerator ExpirePollen(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (foxglovePollenIcon != null)
        {
            foxglovePollenIcon.SetActive(false);
        }

        // Optionally: Heal Luna here
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.SetHealth(health.MaximumHealth, gameObject);
        }

        Debug.Log("🌸 Foxglove pollen effect expired, health restored.");
        isActive = false;
    }

    public bool IsPollenActive() => isActive;
}
