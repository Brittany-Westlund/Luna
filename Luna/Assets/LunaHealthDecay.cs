using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using MoreMountains.CorgiEngine;

[DisallowMultipleComponent]
public class LunaHealthManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image healthBarFill;

    [Header("Health Source")]
    [SerializeField] private Health lunaHealth;

    [Header("Decay Suppression")]
    [SerializeField] private bool decaySuppressed = false;

    private Coroutine suppressDecayRoutine;
    private bool triedAutoFind = false;

    private void Awake()
    {
        ResolveHealthReference();
        ResolveHealthBarReference();
    }

    private IEnumerator Start()
    {
        if (healthBarFill == null)
        {
            yield return null;
            ResolveHealthBarReference();
        }

        SyncFromHealthComponent();
    }

    private void Update()
    {
        SyncFromHealthComponent();
    }

    private void ResolveHealthReference()
    {
        if (lunaHealth == null)
            lunaHealth = GetComponent<Health>();

        if (lunaHealth == null)
            lunaHealth = GetComponentInParent<Health>();

        if (lunaHealth == null)
            lunaHealth = GetComponentInChildren<Health>(true);
    }

    private void ResolveHealthBarReference()
    {
        if (healthBarFill != null || triedAutoFind)
            return;

        triedAutoFind = true;

        GameObject hud = GameObject.Find("HUD");

        if (hud == null)
        {
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas != null)
            {
                Transform hudTransform = canvas.transform.Find("HUD");
                if (hudTransform != null)
                    hud = hudTransform.gameObject;
            }
        }

        if (hud != null)
        {
            Transform healthBar = hud.transform.Find("HealthBar");
            if (healthBar != null)
            {
                Image[] images = healthBar.GetComponentsInChildren<Image>(true);

                if (images != null && images.Length > 0)
                {
                    Image best = null;

                    for (int i = 0; i < images.Length; i++)
                    {
                        if (images[i] == null)
                            continue;

                        if (images[i].type == Image.Type.Filled)
                        {
                            best = images[i];
                            break;
                        }
                    }

                    if (best == null)
                    {
                        if (images.Length > 1)
                            best = images[1];
                        else
                            best = images[0];
                    }

                    healthBarFill = best;
                }
            }
        }

        if (healthBarFill == null)
        {
            Debug.LogWarning("LunaHealthManager: Could not find healthBarFill.");
        }
    }

    private void SyncFromHealthComponent()
    {
        if (healthBarFill == null || lunaHealth == null)
            return;

        if (lunaHealth.MaximumHealth <= 0f)
            return;

        float normalized = lunaHealth.CurrentHealth / lunaHealth.MaximumHealth;
        healthBarFill.fillAmount = Mathf.Clamp01(normalized);
    }

    // Keeps compatibility with scripts that manually drive the bar.
    public void SetHealth(float value)
    {
        if (healthBarFill != null)
        {
            healthBarFill.fillAmount = Mathf.Clamp01(value);
        }
        else
        {
            Debug.LogWarning("LunaHealthManager: healthBarFill missing in SetHealth().");
        }
    }

    // Keeps compatibility with scripts like AnenomePollenEffect.
    public void SuppressDecay(float duration)
    {
        if (suppressDecayRoutine != null)
            StopCoroutine(suppressDecayRoutine);

        suppressDecayRoutine = StartCoroutine(SuppressDecayRoutine(duration));
    }

    private IEnumerator SuppressDecayRoutine(float duration)
    {
        decaySuppressed = true;
        yield return new WaitForSeconds(duration);
        decaySuppressed = false;
        suppressDecayRoutine = null;
    }

    public bool IsDecaySuppressed()
    {
        return decaySuppressed;
    }
}