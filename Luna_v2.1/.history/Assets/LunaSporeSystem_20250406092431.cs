using UnityEngine;
using System.Collections;
using MoreMountains.Tools; // For MMProgressBar
using MoreMountains.CorgiEngine; // For Corgi's Health component

public class LunaSporeSystem : MonoBehaviour
{
    [Header("Spore Management")]
    public GameObject sporePrefab; // Prefab to instantiate
    public Transform attachPoint; // Attachment point for the spore
    public float slideSpeed = 1.5f; // Speed of sliding animation
    public float slideOffset = 0.1f; // Offset above the attach point
    public float sporeHealthCost = 5f; // Health cost for creating a spore
    public float healthReturnOnDetach = 5f; // Health returned when a spore is detached
    private bool healthLocked = false; // Prevents health gain when true


    [Header("Health")]
    public MMProgressBar healthBar; // Progress bar for health display
    private Health _healthComponent; // Reference to the health component

    [Header("Aiding Sprouts")]
    public LayerMask sproutLayer; // Layer for detecting sprouts
    public float aidRadius = 1.0f; // Radius to detect sprouts for aid

    [Header("Wild Spore Pickup")]
    public LayerMask sporeLayer; // Layer for detecting wild spores
    public float pickupRadius = 1.0f; // Radius to detect wild spores

    private GameObject activeSpore; // Reference to the active spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine
    private bool isSliding = false; // Sliding state
    public bool IsSliding => isSliding; // Public getter for sliding state
    public bool HasSporeAttached => activeSpore != null; // True if a spore is attached

    void Start()
    {
        _healthComponent = GetComponent<Health>();
        if (_healthComponent == null)
        {
            Debug.LogError("Health component not found!");
        }

        if (healthBar == null)
        {
            Debug.LogError("Health bar not assigned!");
        }

        UpdateHealthBar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            if (activeSpore == null)
            {
                CreateSpore();
            }
            else if (!IsSliding)
            {
                DetachSpore();
            }
        }

        if (Input.GetButtonDown("Aid") && activeSpore != null)
        {
            AidSprout();
        }

        if (Input.GetKeyDown(KeyCode.G) && activeSpore == null)
        {
            PickupWildSpore();
        }
    }

    private void CreateSpore()
    {
        if (_healthComponent.CurrentHealth < sporeHealthCost)
        {
            Debug.Log("Not enough health to create a spore!");
            return;
        }

        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
        DeductHealth(sporeHealthCost);
    }

    private void DetachSpore()
{
    // Prevent detachment right after dismount
    if (FindObjectOfType<ButterflyFlyHandler>().justDismounted)
    {
        Debug.Log("Skipping DetachSpore() because Luna just dismounted.");
        return;
    }

    if (activeSpore != null)
    {
        if (slideCoroutine != null) StopCoroutine(slideCoroutine);

        slideCoroutine = StartCoroutine(SlideSporeOutAndDestroy());

        RestoreHealth(healthReturnOnDetach);
        Debug.Log("Health restored by DetachSpore().");
    }
}



public void ResetSporeState()
{
    Debug.Log("Resetting spore system state...");

    // Ensure no spore is being detached
    if (activeSpore != null)
    {
        Destroy(activeSpore);
        activeSpore = null;
    }

    // Reset sliding state
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

   Debug.Log($"RestoreHealth CALLED! Amount: {amount}, Current Health: {_healthComponent.CurrentHealth}");
    Debug.Log($"RestoreHealth was called from: {System.Environment.StackTrace}");

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
                DestroyAttachedSpore();
                return; // Only aid one
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
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        isSliding = true;

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
        while (Vector3.Distance(activeSpore.transform.position, targetPosition) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                targetPosition,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        Destroy(activeSpore);
        activeSpore = null;
        isSliding = false;
    }
}
