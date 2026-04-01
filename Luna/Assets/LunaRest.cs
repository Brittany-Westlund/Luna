using UnityEngine;
using MoreMountains.CorgiEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
public class LunaRest : MonoBehaviour
{
    [Header("Healing")]
    public float restRate = 0.1f;
    public float gardenBonus = 0.15f;

    [Header("Rest Visual")]
    public SpriteRenderer lunaRestVisualRenderer;

    [Header("Optional UI Sync")]
    public LunaStatusBarConnector statusBarConnector;

    [Header("Input")]
    public KeyCode restKey = KeyCode.Z;
    public float cancelGraceTime = 0.15f;
    public bool cancelOnMovement = true;
    public bool cancelOnOtherKeys = true;

    [Header("State")]
    public bool isResting = false;
    public bool isInGarden = false;

    [Header("Debug")]
    public bool debugLogs = false;

    private Character _character;
    private Health _health;
    private SpriteRenderer _lunaSpriteRenderer;
    private Animator _lunaAnimator;
    private float _restStartTime;

    private void Awake()
    {
        _character = GetComponent<Character>();

        _health = GetComponent<Health>();
        if (_health == null)
            _health = GetComponentInParent<Health>();
        if (_health == null)
            _health = GetComponentInChildren<Health>(true);

        _lunaSpriteRenderer = GetComponent<SpriteRenderer>();
        _lunaAnimator = GetComponent<Animator>();

        if (statusBarConnector == null)
            statusBarConnector = GetComponent<LunaStatusBarConnector>();

        if (statusBarConnector == null)
            statusBarConnector = FindObjectOfType<LunaStatusBarConnector>();

        if (_health == null)
            Debug.LogError("LunaRest: No Health component found.");
        if (_lunaSpriteRenderer == null)
            Debug.LogError("LunaRest: No SpriteRenderer found on Luna.");
        if (_lunaAnimator == null)
            Debug.LogError("LunaRest: No Animator found on Luna.");
        if (lunaRestVisualRenderer == null)
            Debug.LogError("LunaRest: lunaRestVisualRenderer is not assigned.");
        if (statusBarConnector == null)
            Debug.LogWarning("LunaRest: No LunaStatusBarConnector found. Health bar will not update from resting.");
    }

    private void Start()
    {
        if (lunaRestVisualRenderer != null)
        {
            lunaRestVisualRenderer.enabled = false;

            Color c = lunaRestVisualRenderer.color;
            c.a = 1f;
            lunaRestVisualRenderer.color = c;
        }

        UpdateHealthBarUI();
    }

    private void Update()
    {
        if (!isResting && Input.GetKeyDown(restKey))
        {
            StartResting();
            return;
        }

        if (isResting && Time.time - _restStartTime > cancelGraceTime && Input.GetKeyDown(restKey))
        {
            StopResting();
            return;
        }

        if (isResting && Time.time - _restStartTime > cancelGraceTime)
        {
            bool horizontalInput = cancelOnMovement && Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f;
            bool verticalInput = cancelOnMovement && Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f;
            bool otherKeyPressed = cancelOnOtherKeys && Input.anyKeyDown && !Input.GetKeyDown(restKey);

            if (horizontalInput || verticalInput || otherKeyPressed)
            {
                StopResting();
                return;
            }
        }

        if (isResting && _health != null)
        {
            float rate = restRate;
            if (isInGarden)
                rate += gardenBonus;

            float oldHealth = _health.CurrentHealth;
            float healAmount = rate * _health.MaximumHealth * Time.deltaTime;
            float newHealth = Mathf.Clamp(oldHealth + healAmount, 0f, _health.MaximumHealth);

            if (newHealth > oldHealth)
            {
                _health.SetHealth(newHealth, gameObject);
                UpdateHealthBarUI();

                if (debugLogs)
                    Debug.Log("LunaRest healing -> " + newHealth + " / " + _health.MaximumHealth);
            }
        }
    }

    private void LateUpdate()
    {
        if (!isResting)
            return;

        if (_lunaSpriteRenderer != null)
            _lunaSpriteRenderer.enabled = false;

        if (_lunaAnimator != null)
            _lunaAnimator.enabled = false;

        if (lunaRestVisualRenderer != null)
            lunaRestVisualRenderer.enabled = true;
    }

    public void BeginRestExternal()
    {
        StartResting();
    }

    public void EndRestExternal()
    {
        StopResting();
    }

    private void StartResting()
    {
        if (isResting)
            return;

        if (lunaRestVisualRenderer == null)
        {
            Debug.LogError("LunaRest: lunaRestVisualRenderer is not assigned.");
            return;
        }

        isResting = true;
        _restStartTime = Time.time;

        if (_character != null)
            _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);

        if (_lunaSpriteRenderer != null)
            _lunaSpriteRenderer.enabled = false;

        if (_lunaAnimator != null)
            _lunaAnimator.enabled = false;

        lunaRestVisualRenderer.enabled = true;

        if (debugLogs)
        {
            Debug.Log(
                "LunaRest StartResting | " +
                "health=" + (_health != null) +
                " | statusBarConnector=" + (statusBarConnector != null) +
                " | current=" + (_health != null ? _health.CurrentHealth.ToString() : "NULL") +
                " | max=" + (_health != null ? _health.MaximumHealth.ToString() : "NULL")
            );
        }

        Debug.Log("Luna started resting");
    }

    private void StopResting()
    {
        if (!isResting)
            return;

        isResting = false;

        if (_character != null)
            _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);

        if (_lunaSpriteRenderer != null)
            _lunaSpriteRenderer.enabled = true;

        if (_lunaAnimator != null)
            _lunaAnimator.enabled = true;

        if (lunaRestVisualRenderer != null)
            lunaRestVisualRenderer.enabled = false;

        UpdateHealthBarUI();

        Debug.Log("Luna stopped resting");
    }

    private void UpdateHealthBarUI()
    {
        if (statusBarConnector != null)
        {
            statusBarConnector.UpdateHealthBar();

            if (debugLogs)
                Debug.Log("LunaRest UpdateHealthBarUI called.");
        }
        else if (debugLogs)
        {
            Debug.LogWarning("LunaRest: statusBarConnector is null; could not update health bar.");
        }
    }

    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
            isInGarden = true;
    }

    private void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
            isInGarden = false;
    }
}