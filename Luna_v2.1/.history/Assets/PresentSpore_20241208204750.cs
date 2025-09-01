using UnityEngine;
using System.Collections;
using MoreMountains.Tools; // For MMProgressBar
using MoreMountains.CorgiEngine; // For Corgi's Health component

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // Prefab to instantiate
    public Transform attachPoint; // Attachment point for the spore
    public float slideSpeed = 1.5f; // Speed of sliding animation
    public float slideOffset = 0.1f; // Offset above the attach point
    public float sporeHealthCost = 5f; // Health cost for creating a spore
    public float healthReturnOnDetach = 5f; // Health returned when a spore is detached

    private Health _healthComponent; // Reference to the health component
    public MMProgressBar healthBar; // Progress bar for health display

    private GameObject activeSpore; // Reference to the active spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine
    private bool isSliding = false; // Sliding state
    public bool IsSliding => isSliding; // Public getter for sliding state
    public bool HasSporeAttached => activeSpore != null; // True if a spore is attached

    public WildSporePickup wildSporePickup; // Reference to the WildSporePickup script

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
        // Prevent instantiation if holding a wild spore
        if (wildSporePickup != null && wildSporePickup.IsHoldingSpore) return;

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
    }

    public void CreateSpore()
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

    public void DetachSpore()
    {
        if (activeSpore != null)
        {
            if (slideCoroutine != null) StopCoroutine(slideCoroutine);

            slideCoroutine = StartCoroutine(SlideSporeOutAndDestroy());
            RestoreHealth(healthReturnOnDetach);
        }
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

    private void RestoreHealth(float amount)
    {
        _healthComponent.CurrentHealth = Mathf.Min(
            _healthComponent.CurrentHealth + amount,
            _healthComponent.MaximumHealth
        );
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        if (healthBar != null && _healthComponent != null)
        {
            healthBar.UpdateBar(_healthComponent.CurrentHealth, 0f, _healthComponent.MaximumHealth);
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
