using System.Collections;
using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

public class LunaStatusEffects : MonoBehaviour
{
    public bool IsSlumberdustImmune { get; private set; }

    [Header("Optional Auto-Find")]
    public string healthBarObjectName = "HealthBar";
    public string anemoneIconPath = "AnemonePollenHoldPoint/AnemonePollenLuna";

    private Coroutine immunityCoroutine;
    private Coroutine healCoroutine;
    private Coroutine iconCoroutine;

    public void ApplyAnemoneEffect(float duration, float healPercentOverDuration)
    {
        if (immunityCoroutine != null)
        {
            StopCoroutine(immunityCoroutine);
        }

        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
        }

        if (iconCoroutine != null)
        {
            StopCoroutine(iconCoroutine);
        }

        immunityCoroutine = StartCoroutine(ImmunityRoutine(duration));
        healCoroutine = StartCoroutine(HealOverTimeRoutine(duration, healPercentOverDuration));
        iconCoroutine = StartCoroutine(AnemoneIconRoutine(duration));
    }

    private IEnumerator ImmunityRoutine(float duration)
    {
        IsSlumberdustImmune = true;
        yield return new WaitForSeconds(duration);
        IsSlumberdustImmune = false;
        immunityCoroutine = null;
    }

    private IEnumerator HealOverTimeRoutine(float duration, float healPercentOverDuration)
    {
        Health health = GetComponent<Health>();
        MMProgressBar healthBar = FindHealthBarDeep();

        if (health == null)
        {
            healCoroutine = null;
            yield break;
        }

        float totalHeal = health.MaximumHealth * Mathf.Clamp01(healPercentOverDuration);
        float healedSoFar = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            float expectedTotalByNow = totalHeal * Mathf.Clamp01(elapsed / duration);
            float deltaToApply = expectedTotalByNow - healedSoFar;

            if (deltaToApply > 0f)
            {
                float newHealth = Mathf.Clamp(health.CurrentHealth + deltaToApply, 0f, health.MaximumHealth);
                health.SetHealth(newHealth, gameObject);

                if (healthBar != null)
                {
                    healthBar.UpdateBar(health.CurrentHealth, 0f, health.MaximumHealth);
                }

                healedSoFar += deltaToApply;
            }

            yield return null;
        }

        healCoroutine = null;
    }

    private IEnumerator AnemoneIconRoutine(float duration)
    {
        Transform iconTransform = transform.Find(anemoneIconPath);

        if (iconTransform == null)
        {
            Debug.LogWarning("[LunaStatusEffects] Anemone icon not found at path: " + anemoneIconPath);
            iconCoroutine = null;
            yield break;
        }

        GameObject icon = iconTransform.gameObject;
        icon.SetActive(true);

        yield return new WaitForSeconds(duration);

        if (icon != null)
        {
            icon.SetActive(false);
        }

        iconCoroutine = null;
    }

    private MMProgressBar FindHealthBarDeep()
    {
        GameObject[] all = Resources.FindObjectsOfTypeAll<GameObject>();

        for (int i = 0; i < all.Length; i++)
        {
            GameObject obj = all[i];

            if (obj == null)
            {
                continue;
            }

            if (obj.name != healthBarObjectName)
            {
                continue;
            }

            if (!obj.scene.IsValid())
            {
                continue;
            }

            MMProgressBar bar = obj.GetComponent<MMProgressBar>();
            if (bar != null)
            {
                return bar;
            }

            MMProgressBar[] children = obj.GetComponentsInChildren<MMProgressBar>(true);
            if (children.Length > 0)
            {
                return children[0];
            }
        }

        return null;
    }

    private void OnDisable()
    {
        IsSlumberdustImmune = false;

        if (immunityCoroutine != null)
        {
            StopCoroutine(immunityCoroutine);
            immunityCoroutine = null;
        }

        if (healCoroutine != null)
        {
            StopCoroutine(healCoroutine);
            healCoroutine = null;
        }

        if (iconCoroutine != null)
        {
            StopCoroutine(iconCoroutine);
            iconCoroutine = null;
        }

        Transform iconTransform = transform.Find(anemoneIconPath);
        if (iconTransform != null)
        {
            iconTransform.gameObject.SetActive(false);
        }
    }
}