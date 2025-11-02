using UnityEngine;
using MoreMountains.CorgiEngine;

[DisallowMultipleComponent]
public class LunaNoDeathAndRest : MonoBehaviour
{
    [Header("Health Floor & Rest")]
    [Tooltip("Luna's health will never go below this value.")]
    public float minHealthFloor = 1f;

    [Range(0f, 1f)]
    [Tooltip("When current health is below this fraction of max, auto-rest begins.")]
    public float restThresholdPercent = 0.10f; // 10%

    [Tooltip("If true, stop resting automatically at full health.")]
    public bool stopRestAtFull = true;

    private Health _health;
    private LunaRest _rest;

    void Awake()
    {
        _health = GetComponent<Health>();
        _rest   = GetComponent<LunaRest>();

        if (_health == null)
        {
            Debug.LogError("[LunaNoDeathAndRest] No Corgi 'Health' found on this GameObject.");
            enabled = false;
            return;
        }

        // Defensive: avoid engine death side effects even if death is triggered elsewhere.
        _health.DestroyOnDeath = false;
        _health.RespawnAtInitialLocation = false;
        _health.CollisionsOffOnDeath = false;
        _health.GravityOffOnDeath = false;
    }

    void OnEnable()
    {
        if (_health != null)
        {
            // Intercept death if engine still fires it; we immediately revive and enforce floor.
            _health.OnDeath += HandleDeathIntercept;
        }
    }

    void OnDisable()
    {
        if (_health != null)
        {
            _health.OnDeath -= HandleDeathIntercept;
        }
    }

    void Update()
    {
        if (_health == null) return;

        // 1) Enforce hard floor every frame (handles damage/decay paths that bypass our events)
        if (_health.CurrentHealth < minHealthFloor)
        {
            // Use SetHealth so all listeners update properly
            _health.SetHealth(minHealthFloor, gameObject);
        }

        // 2) Auto-rest below threshold
        float threshold = Mathf.Max(minHealthFloor, _health.MaximumHealth * restThresholdPercent);
        if (_rest != null)
        {
            bool belowThreshold = _health.CurrentHealth <= threshold;

            if (belowThreshold && !_rest.isResting)
            {
                _rest.BeginRestExternal();
            }
            else if (stopRestAtFull && _rest.isResting && _health.CurrentHealth >= _health.MaximumHealth)
            {
                _rest.EndRestExternal();
            }
        }
    }

    private void HandleDeathIntercept()
    {
        // If something still managed to push to 0 and fire OnDeath:
        // Immediately revive and clamp to a safe sliver above floor.
        _health.Revive(); // re-enables internals; sets to initial health in some versions

        // After revive, enforce a safe value (>= floor, >= 10% if you prefer—here we use floor+epsilon)
        float safe = Mathf.Max(minHealthFloor, 1f);
        _health.SetHealth(safe, gameObject);

        // Begin rest if available
        if (_rest != null && !_rest.isResting)
        {
            _rest.BeginRestExternal();
        }

        Debug.Log("🌙 Death intercepted: revived, clamped to floor, entering rest.");
    }
}
