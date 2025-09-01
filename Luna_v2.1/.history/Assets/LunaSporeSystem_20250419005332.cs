// LunaSporeSystem.cs
using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

public class LunaSporeSystem : MonoBehaviour
{
    [Header("Spore Management")]
    public GameObject sporePrefab;
    public Transform attachPoint;
    public float slideSpeed        = 1.5f;
    public float slideOffset       = 0.1f;
    public float sporeHealthCost   = 5f;
    public float healthReturnOnDetach = 5f;

    [Header("Health")]
    public MMProgressBar healthBar;
    private Health _healthComponent;

    [Header("Aiding Sprouts")]
    public LayerMask sproutLayer;
    public float aidRadius = 1f;

    [Header("Wild Spore Pickup")]
    public LayerMask sporeLayer;
    public float pickupRadius = 1f;

    private GameObject activeSpore;
    private Coroutine slideCoroutine;
    private bool isSliding;

    void Start()
    {
        _healthComponent = GetComponent<Health>();
        if (_healthComponent == null) Debug.LogError("Health component not found!");
        if (healthBar == null)      Debug.LogError("Health bar not assigned!");
        UpdateHealthBar();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !isSliding)
        {
            if (activeSpore != null && IsNearSprout())
                AidSprout();
            else if (activeSpore == null && IsNearWildSpore())
                PickupWildSpore();
            else if (activeSpore == null)
                CreateSpore();
            else
                DetachSpore();
        }
    }

    bool IsNearSprout()
    {
        var cols = Physics2D.OverlapCircleAll(transform.position, aidRadius, sproutLayer);
        foreach (var col in cols)
        {
            var sprout = col.GetComponentInParent<SproutAndLightManager>();
            if (sprout != null && sprout.isPlayerNearby)
                return true;
        }
        return false;
    }

    bool IsNearWildSpore()
    {
        return Physics2D.OverlapCircle(transform.position, pickupRadius, sporeLayer) != null;
    }

    void CreateSpore()
    {
        if (_healthComponent.CurrentHealth < sporeHealthCost)
        {
            Debug.Log("Not enough health to create a spore!");
            return;
        }
        Vector3 spawnPos = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPos, Quaternion.identity);
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
        _healthComponent.Damage(sporeHealthCost, gameObject, 0.5f, 0f, Vector3.zero);
        UpdateHealthBar();
    }

    void DetachSpore()
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
            _healthComponent.CurrentHealth = Mathf.Min(
                _healthComponent.CurrentHealth + healthReturnOnDetach,
                _healthComponent.MaximumHealth
            );
            UpdateHealthBar();
        }
    }

    void AidSprout()
    {
        Debug.Log("Planting spore on a nearby sprout...");
        var cols = Physics2D.OverlapCircleAll(transform.position, aidRadius, sproutLayer);
        foreach (var col in cols)
        {
            var sprout = col.GetComponentInParent<SproutAndLightManager>();
            if (sprout != null && sprout.isPlayerNearby)
            {
                sprout.PlantSpore();
                Destroy(activeSpore);
                activeSpore = null;
                return;
            }
        }
        Debug.Log("No valid sprout found to aid.");
    }

    void PickupWildSpore()
    {
        Debug.Log("Attempting to pick up a wild spore...");
        var col = Physics2D.OverlapCircle(transform.position, pickupRadius, sporeLayer);
        if (col != null)
        {
            Destroy(col.gameObject);
            Vector3 spawnPos = attachPoint.position + Vector3.up * slideOffset;
            activeSpore = Instantiate(sporePrefab, spawnPos, Quaternion.identity);
            slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
        }
    }

    void UpdateHealthBar()
    {
        if (healthBar != null && _healthComponent != null)
            healthBar.UpdateBar(
                _healthComponent.CurrentHealth,
                0f,
                _healthComponent.MaximumHealth
            );
    }

    IEnumerator SlideSporeIntoPlace()
    {
        isSliding = true;
        while (activeSpore != null &&
               Vector3.Distance(activeSpore.transform.position, attachPoint.position) > 0.01f)
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
            if (activeSpore.TryGetComponent<Collider2D>(out var c)) c.enabled = false;
            if (activeSpore.TryGetComponent<Rigidbody2D>(out var rb)) rb.simulated = false;
        }
        isSliding = false;
    }

    IEnumerator SlideSporeOutAndDestroy()
    {
        isSliding = true;
        Vector3 target = attachPoint.position + Vector3.up * slideOffset;
        while (activeSpore != null &&
               Vector3.Distance(activeSpore.transform.position, target) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                target,
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
