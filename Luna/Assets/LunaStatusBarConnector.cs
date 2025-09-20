using UnityEngine;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;

public class LunaStatusBarConnector : MonoBehaviour
{
    [Header("Light System Reference (Optional)")]
    public float currentLight = 100f;
    public float maxLight = 100f;

    private Health _health;
    private MMProgressBar _healthBar;
    private MMProgressBar _lightBar;

    void Awake()
    {
        _health = GetComponent<Health>();
        if (_health == null)
        {
            Debug.LogError("❌ No Health component found on Luna.");
        }
    }

    void Start()
    {
        // Find bars by name
        MMProgressBar[] bars = FindObjectsOfType<MMProgressBar>();

        foreach (var bar in bars)
        {
            if (bar.name.ToLower().Contains("health"))
                _healthBar = bar;
            else if (bar.name.ToLower().Contains("light"))
                _lightBar = bar;
        }

        if (_healthBar == null)
            Debug.LogWarning("⚠️ Health bar (MMProgressBar) not found.");
        if (_lightBar == null)
            Debug.LogWarning("⚠️ Light bar (MMProgressBar) not found.");

        if (_health != null)
        {
            _health.OnHit += OnHealthChanged;
            UpdateHealthBar(); // Set initial value
        }

        UpdateLightBar(); // Set initial light value
    }

    void OnDestroy()
    {
        if (_health != null)
        {
            _health.OnHit -= OnHealthChanged;
        }
    }

    void OnHealthChanged()
    {
        UpdateHealthBar();
    }

    public void SetLight(float current, float max)
    {
        currentLight = current;
        maxLight = max;
        UpdateLightBar();
    }

    public void UpdateHealthBar()
    {
        if (_healthBar != null && _health != null)
        {
            _healthBar.UpdateBar(_health.CurrentHealth, 0f, _health.MaximumHealth);
        }
    }

    public void UpdateLightBar()
    {
        if (_lightBar != null)
        {
            _lightBar.UpdateBar(currentLight, 0f, maxLight);
        }
    }
}
