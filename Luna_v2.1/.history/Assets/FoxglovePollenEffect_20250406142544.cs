using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using System.Collections;

public class FoxglovePollenEffect : MonoBehaviour
{
    public GameObject foxglovePollenIcon; // Assign manually or find it on start
    public MMProgressBar healthBar;       // Optional: assign if you want visual update

    private bool isActive = false;

    public void ActivatePollenEffect(float duration)
    {
        isActive = true;

        if (foxglovePollenIcon != null)
        {
            foxglovePollenIcon.SetActive(true);
        }

        StopAllCoroutines();
        StartCoroutine(ExpirePollen(duration));
    }

    private IEnumerator ExpirePollen(float duration)
    {
        yield return new WaitForSeconds(duration);

        if (foxglovePollenIcon != null)
        {
            foxglovePollenIcon.SetActive(false);
        }

        // Restore Luna's health to full
        Health health = GetComponent<Health>();
        if (health != null)
        {
            health.SetHealth(health.MaximumHealth, gameObject);
        }

        if (healthBar != null && health != null)
        {
            healthBar.UpdateBar(health.CurrentHealth, 0f, health.MaximumHealth);
        }

        Debug.Log("🧚 Foxglove effect ended. Health restored.");
        isActive = false;
    }

    public bool IsPollenActive() => isActive;
}
