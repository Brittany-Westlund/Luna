using UnityEngine;
using System.Collections;
using MoreMountains.Tools;
using MoreMountains.CorgiEngine;

public class PresentSpore : MonoBehaviour
{
    public GameObject sporePrefab;
    public Transform attachPoint;
    public float slideSpeed = 1.5f;
    public float slideOffset = 0.1f;
    public float sporeHealthCost = 5f;
    public float healthReturnOnDetach = 5f;

    private Health _healthComponent;
    public MMProgressBar healthBar;

    private GameObject activeSpore;
    private Coroutine slideCoroutine;

    private bool _isSliding;

    public bool IsSliding
    {
        get => _isSliding;
        private set => _isSliding = value;
    }

    public bool HasSporeAttached => activeSpore != null; // Public property to check if a spore is attached

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

        // Deduct health
        _healthComponent.Damage(sporeHealthCost, gameObject, 0.5f, 0f, Vector3.zero);
        UpdateHealthBar();

        Vector3 spawnPosition = attachPoint.position + Vector3.up * slideOffset;
        activeSpore = Instantiate(sporePrefab, spawnPosition, Quaternion.identity);
        slideCoroutine = StartCoroutine(SlideSporeIntoPlace());
    }

    void UpdateHealthBar()
    {
        if (healthBar != null && _healthComponent != null)
        {
            healthBar.UpdateBar(_healthComponent.CurrentHealth, 0f, _healthComponent.MaximumHealth);
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
}
