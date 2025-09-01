using UnityEngine;
using System.Collections;
using MoreMountains.Tools; // For MMProgressBar
using MoreMountains.CorgiEngine; // For Corgi's Health component

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab; // The prefab to instantiate as the spore
    public Transform attachPoint; // The point where the spore will attach to Luna
    public float slideSpeed = 1.5f; // Speed of the slide animation
    public float slideOffset = 0.1f; // Offset above the attach point to start the slide
    public float sporeHealthCost = 5f; // Health cost for creating a spore
    public float healthReturnOnDetach = 5f; // Health returned when the spore detaches

    private Health _healthComponent; // Reference to Corgi's Health component
    public MMProgressBar healthBar; // Reference to MMProgressBar for the health display

    private GameObject activeSpore; // Reference to the currently instantiated spore
    private Coroutine slideCoroutine; // Reference to the slide coroutine

    private bool _isSliding; // Internal field to track sliding state

    public bool IsSliding
    {
        get => _isSliding;
        private set => _isSliding = value; // Allows external scripts to read but not write
    }

    public bool HasSporeAttached => activeSpore != null;

    void Start()
    {
        _healthComponent = GetComponent<Health>();
        if (_healthComponent == null)
        {
            Debug.LogError("Health component not found on Luna!");
        }

        if (healthBar == null)
        {
            Debug.LogError("HealthBar (MMProgressBar) is not assigned!");
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
    }

    public void CreateSpore()
    {
        if (sporePrefab == null || attachPoint == null)
        {
            Debug.LogError("Spore prefab or attach point is not assigned!");
            return;
        }

        if (_healthComponent.CurrentHealth < sporeHealthCost)
        {
            Debug.Log("Not enough health to create a spore!");
            return;
        }

        // Temporarily disable flicker effects
        bool originalFlickerState = _healthComponent.FlickerSpriteOnHit;
        Color originalFlickerColor = _healthComponent.FlickerColor;

        _healthComponent.FlickerSpriteOnHit = false; // Disable flicker
        _healthComponent.FlickerColor = Color.clear; // Clear flicker color

        // Deduct health
        _healthComponent.Damage(sporeHealthCost, gameObject, 0.5f, 0f, Vector3.zero);

        // Restore original flicker settings
        _healthComponent.FlickerSpriteOnHit = originalFlickerState;
        _healthComponent.FlickerColor = originalFlickerColor;

        UpdateHealthBar();

        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
    }

    void DetachSpore()
    {
        if (activeSpore != null)
        {
            if (slideCoroutine != null)
            {
                StopCoroutine(slideCoroutine);
            }

            slideCoroutine = StartCoroutine(SlideSporeOutAndDestroy());

            // Restore health
            _healthComponent.CurrentHealth = Mathf.Min(
                _healthComponent.CurrentHealth + healthReturnOnDetach,
                _healthComponent.MaximumHealth
            );
            UpdateHealthBar();
        }
    }

    IEnumerator SlideSporeIntoPlace()
    {
        IsSliding = true;

        while (Vector3.Distance(activeSpore.transform.position, attachPoint.position) > 0.01f)
        {
            activeSpore.transform.position = Vector3.MoveTowards(
                activeSpore.transform.position,
                attachPoint.position,
                slideSpeed * Time.deltaTime
            );
            yield return null;
        }

        activeSpore.transform.position = attachPoint.position;
        activeSpore.transform.SetParent(attachPoint);
        IsSliding = false;
    }

    IEnumerator SlideSporeOutAndDestroy()
    {
        IsSliding = true;

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
        IsSliding = false;
    }

    void UpdateHealthBar()
    {
        if (healthBar != null && _healthComponent != null)
        {
            healthBar.UpdateBar(_healthComponent.CurrentHealth, 0f, _healthComponent.MaximumHealth);
        }
    }

    public void ResetSporeState()
    {
        if (activeSpore != null)
        {
            Destroy(activeSpore);
            activeSpore = null;
        }
        IsSliding = false;
    }

    // Method to get the currently attached spore
    public GameObject GetAttachedSpore()
    {
        return activeSpore;
    }
}

// Perfect
