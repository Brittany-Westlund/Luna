using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

public class LunaSporeSystem : MonoBehaviour
{
    [Header("Spore Management")]
    public GameObject sporePrefab;
    public Transform attachPoint;
    public float slideSpeed = 1.5f;
    public float slideOffset = 0.1f;
    public float sporeHealthCost = 5f;
    public float healthReturnOnDetach = 5f;
    private bool healthLocked = false;

    [Header("Health")]
    public MMProgressBar healthBar;
    private Health _healthComponent;

    [Header("Aiding Sprouts")]
    public LayerMask sproutLayer;
    public float aidRadius = 1.0f;

    [Header("Wild Spore Pickup")]
    public LayerMask sporeLayer;
    public float pickupRadius = 1.0f;

    private GameObject activeSpore;
    private Coroutine slideCoroutine;
    private bool isSliding = false;
    public bool IsSliding => isSliding;
    public bool HasSporeAttached => activeSpore != null;

    [Header("Spore SFX")]
    public AudioSource pickupOrCreateSFX;  // 1: picking up or creating a spore
    public AudioSource growFlowerSFX;      // 2: using a spore to grow a flower
    public AudioSource storeSporeSFX;      // 3: storing (detaching) a spore

    void Start()
    {
        _healthComponent = GetComponent<Health>();
        if (_healthComponent == null) Debug.LogError("Health component not found!");
        if (healthBar == null) Debug.LogError("Health bar not assigned!");
        UpdateHealthBar();
    }

  void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isSliding)
        {
            if (activeSpore != null && IsNearSprout())
            {
                AidSprout(); // Uses spore to grow a flower
            }
            else if (activeSpore == null && IsNearWildSpore())
            {
                PickupWildSpore(); // Picks up wild spore
            }
            else if (activeSpore == null)
            {
                CreateSpore(); // Creates a new spore if no wild one found
            }
            else
            {
                DetachSpore(); // Drops spore if not near a sprout
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
            {
                return true;
            }
        }
        return false;
    }

    private bool IsNearWildSpore()
    {
        Collider2D sporeCollider = Physics2D.OverlapCircle(transform.position, pickupRadius, sporeLayer);
        return sporeCollider != null;
    }

    /// <summary>
    /// Attempts to spawn a spore at the attachPoint. 
    /// Returns the spawned GameObject (or null on failure).
    /// </summary>
    public GameObject CreateSpore()
    {
        // not enough health?
        if (_healthComponent.CurrentHealth < sporeHealthCost)
        {
            Debug.Log("Not enough health to create a spore!");
            return null;
        }

        // figure out where to spawn
        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        
        // instantiate & cache
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);
        
        // slide‐into‐place effect
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
        
        if (pickupOrCreateSFX != null) pickupOrCreateSFX.Play();
        
        // pay the health cost
        DeductHealth(sporeHealthCost);

        // give the caller a reference so they can Destroy() it later
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
            if (storeSporeSFX != null) storeSporeSFX.Play();

            RestoreHealth(healthReturnOnDetach);
            Debug.Log("Health restored by DetachSpore().");
        }
    }

    public void ResetSporeState()
    {
        Debug.Log("Resetting spore system state...");
        if (activeSpore != null)
        {
            Destroy(activeSpore);
            activeSpore = null;
        }
        isSliding = false;
        Debug.Log("Spore system fully reset.");
    }

    public void DestroyAttachedSpore()
    {
        if (activeSpore != null)
        {
            Destroy(activeSpore);
            activeSpore = null;
            Debug.Log("Attached spore destroyed.");
        }
    }

    private void DeductHealth(float amount)
    {
        _healthComponent.Damage(amount, gameObject, 0.5f, 0f, Vector3.zero);
        UpdateHealthBar();
    }

    public void SetHealthLock(bool state)
    {
        Debug.Log($"Health lock set to: {state}");
        healthLocked = state;
    }

    private void RestoreHealth(float amount)
    {
        if (healthLocked)
        {
            Debug.Log("Health gain prevented due to recent dismount.");
            return;
        }

        _healthComponent.CurrentHealth = Mathf.Min(
            _healthComponent.CurrentHealth + amount,
            _healthComponent.MaximumHealth
        );

        UpdateHealthBar();
        Debug.Log("Health AFTER restore: " + _healthComponent.CurrentHealth);
    }

    public void UpdateHealthBar()
    {
        if (healthBar != null && _healthComponent != null)
        {
            healthBar.UpdateBar(_healthComponent.CurrentHealth, 0f, _healthComponent.MaximumHealth);
        }
    }

    private void AidSprout()
    {
        Debug.Log("Looking for a sprout to aid...");
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, aidRadius, sproutLayer);

        foreach (var col in colliders)
        {
            SproutAndLightManager sprout = col.GetComponentInParent<SproutAndLightManager>();
            if (sprout != null && sprout.isPlayerNearby)
            {
                Debug.Log("Aiding sprout: " + col.name);
                sprout.ResetOnGrowth();
                
                if (growFlowerSFX != null) growFlowerSFX.Play();

                DestroyAttachedSpore();
                return;
            }
        }

        Debug.Log("No valid sprout found to aid.");
    }

    private void PickupWildSpore()
    {
        Debug.Log("Attempting to pick up a wild spore...");
        Collider2D sporeCollider = Physics2D.OverlapCircle(transform.position, pickupRadius, sporeLayer);

        if (sporeCollider != null)
        {
            Debug.Log($"Wild spore detected: {sporeCollider.name}");
            Destroy(sporeCollider.gameObject);

            Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
            activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);
            slideCoroutine = StartCoroutine(SlideSporeIntoPlace());

            Debug.Log("Wild spore picked up and attached!");

            if (pickupOrCreateSFX != null) pickupOrCreateSFX.Play();
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        isSliding = true;

        if (activeSpore == null)
        {
            Debug.LogWarning("🚨 SlideSporeIntoPlace: activeSpore is null at start of coroutine!");
            isSliding = false;
            yield break;
        }

        while (activeSpore != null && Vector3.Distance(activeSpore.transform.position, attachPoint.position) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                attachPoint.position,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        if (activeSpore != null)
        {
            activeSpore.transform.SetParent(attachPoint);
            activeSpore.transform.localPosition = Vector3.zero;
        }

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
}
