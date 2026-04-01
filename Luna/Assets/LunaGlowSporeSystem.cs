using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

[RequireComponent(typeof(Health))]
public class LunaGlowSporeSystem : MonoBehaviour
{
    [Header("Spore Management")]
    public GameObject sporePrefab;
    public Transform attachPoint;
    public float slideSpeed = 1.5f;
    public float slideOffset = 0.1f;
    public float sporeHealthCost = 3f; 
    public float healthReturnOnDetach = 4f;
    private bool healthLocked = false;

    [Header("Aiding Sprouts (Auto-Lit Mode)")]
    public LayerMask sproutLayer;
    public float aidRadius = 1.0f;

    [Header("Wild Spore Pickup")]
    public LayerMask sporeLayer;
    public float pickupRadius = 1.0f;

    [Header("Spore SFX")]
    public AudioSource pickupOrCreateSFX;
    public AudioSource growFlowerSFX;
    public AudioSource storeSporeSFX;
    public AudioSource moonLightSFX; 

    [Header("Moon Orb Visuals")]
    public Sprite moonOrbSprite;
    public float pulseAmplitude = 0.05f;
    public float pulseSpeed = 2f;
    public Color glowTint = new Color(1.2f, 1.3f, 1.6f, 1f);

    private GameObject activeSpore;
    private Coroutine slideCoroutine;
    private Coroutine pulseRoutine;
    private bool isSliding = false;
    public bool IsSliding => isSliding;
    public bool HasSporeAttached => activeSpore != null;

    private Health _healthComponent;
    private LunaStatusBarConnector _statusBarConnector;

    void Start()
    {
        _healthComponent = GetComponent<Health>();
        if (_healthComponent == null)
            Debug.LogError("❌ Health component not found!");

        _statusBarConnector = GetComponent<LunaStatusBarConnector>() ??
                              FindObjectOfType<LunaStatusBarConnector>();

        if (_statusBarConnector == null)
            Debug.LogWarning("⚠️ No LunaStatusBarConnector found — health/light UI will not update.");

        UpdateHealthBar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S) && !isSliding)
        {
            if (activeSpore != null && IsNearSprout())
            {
                AidSproutAndLight();
            }
            else if (activeSpore == null && IsNearWildSpore())
            {
                PickupWildSporeAndInfuse();
            }
            else if (activeSpore == null)
            {
                CreateInfusedSpore();
            }
            else
            {
                DetachSpore();
            }
        }
    }

    private bool IsNearSprout()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aidRadius, sproutLayer);
        foreach (var col in colliders)
        {
            var sprout = col.GetComponentInParent<SproutAndLightManager>();
            if (sprout != null && sprout.isPlayerNearby)
                return true;
        }
        return false;
    }

    private bool IsNearWildSpore()
    {
        Collider2D sporeCollider = Physics2D.OverlapCircle(transform.position, pickupRadius, sporeLayer);
        return sporeCollider != null;
    }

    // 🌕 Create an automatically infused (lit) spore
    public GameObject CreateInfusedSpore()
    {
        if (_healthComponent == null || _healthComponent.CurrentHealth < sporeHealthCost)
        {
            Debug.Log("Not enough health to create a moon spore!");
            return null;
        }

        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);

        SetMoonOrbVisual(activeSpore);
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
        pickupOrCreateSFX?.Play();
        moonLightSFX?.Play();

        DeductHealth(sporeHealthCost);
        return activeSpore;
    }

    private void DetachSpore()
    {
        if (FindObjectOfType<ButterflyFlyHandler>()?.justDismounted == true)
        {
            Debug.Log("Skipping DetachSpore() because Luna just dismounted.");
            return;
        }

        if (activeSpore != null)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);
            slideCoroutine = StartCoroutine(SlideSporeOutAndDestroy());
            storeSporeSFX?.Play();
            RestoreHealth(healthReturnOnDetach);
        }
    }

    private void AidSproutAndLight()
    {
        Debug.Log("🌕 Aiding + lighting a sprout...");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aidRadius, sproutLayer);

        foreach (var col in colliders)
        {
            SproutAndLightManager sprout = col.GetComponentInParent<SproutAndLightManager>();
            if (sprout != null && sprout.isPlayerNearby)
            {
               sprout.ResetOnGrowth();

                // Find and enable the LitFlowerB child sprite
                Transform litChild = sprout.transform.Find("LitFlowerB");
                if (litChild != null)
                {
                    SpriteRenderer litRenderer = litChild.GetComponent<SpriteRenderer>();
                    if (litRenderer != null)
                    {
                        litRenderer.enabled = true;
                        Debug.Log($"Enabled LitFlowerB on {sprout.name}");
                    }
                }
                else
                {
                    Debug.LogWarning($"No LitFlowerB found under {sprout.name}");
                }

                growFlowerSFX?.Play();
                moonLightSFX?.Play();

                DestroyAttachedSpore();
                return;
            }
        }
    }

    private void PickupWildSporeAndInfuse()
    {
        Collider2D sporeCollider = Physics2D.OverlapCircle(transform.position, pickupRadius, sporeLayer);

        if (sporeCollider != null)
        {
            Destroy(sporeCollider.gameObject);

            Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
            activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);

            SetMoonOrbVisual(activeSpore);
            slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
            pickupOrCreateSFX?.Play();
            moonLightSFX?.Play();
        }
    }

    private void SetMoonOrbVisual(GameObject spore)
    {
        if (spore == null) return;

        SpriteRenderer sr = spore.GetComponentInChildren<SpriteRenderer>();
        if (sr != null)
        {
            if (moonOrbSprite != null)
                sr.sprite = moonOrbSprite;

            sr.color = glowTint;
        }

        Light lightComp = spore.GetComponentInChildren<Light>();
        if (lightComp != null)
        {
            lightComp.enabled = true;
            lightComp.intensity = 1.25f;
        }

        if (pulseRoutine != null) StopCoroutine(pulseRoutine);
        pulseRoutine = StartCoroutine(PulseEffect(spore.transform));
    }

    private IEnumerator PulseEffect(Transform target)
    {
        Vector3 baseScale = target.localScale;
        float timer = 0f;

        while (target != null)
        {
            timer += Time.deltaTime * pulseSpeed;
            float scaleOffset = Mathf.Sin(timer) * pulseAmplitude;
            target.localScale = baseScale * (1f + scaleOffset);
            yield return null;
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        isSliding = true;

        if (activeSpore == null)
        {
            isSliding = false;
            yield break;
        }

        // Cache transform to avoid missing reference spam
    Transform sporeTransform = activeSpore.transform;

        while (Vector3.Distance(activeSpore.transform.position, attachPoint.position) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                attachPoint.position,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        activeSpore.transform.SetParent(attachPoint);
        activeSpore.transform.localPosition = Vector3.zero;
        isSliding = false;
    }

    IEnumerator SlideSporeOutAndDestroy()
    {
        isSliding = true;
        Vector3 targetPosition = attachPoint.position + Vector3.up * slideOffset;

        while (activeSpore != null && Vector3.Distance(activeSpore.transform.position, targetPosition) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                targetPosition,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (activeSpore != null)
        {
            Destroy(activeSpore);
            activeSpore = null;
        }

        isSliding = false;
    }

    private void DeductHealth(float amount)
    {
        if (_healthComponent == null) return;
        _healthComponent.Damage(amount, gameObject, 0.5f, 0f, Vector3.zero);
        UpdateHealthBar();
    }

    private void RestoreHealth(float amount)
    {
        if (_healthComponent == null || healthLocked) return;
        _healthComponent.CurrentHealth = Mathf.Min(
            _healthComponent.CurrentHealth + amount,
            _healthComponent.MaximumHealth
        );
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (_statusBarConnector != null && _healthComponent != null)
            _statusBarConnector.UpdateHealthBar();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.8f, 0.8f, 1f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, aidRadius);
    }

    public void DestroyAttachedSpore()
    {
        if (activeSpore != null)
        {
            Destroy(activeSpore);
            activeSpore = null;
        }
    }
}
