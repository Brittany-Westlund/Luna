using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

[DisallowMultipleComponent]
public class LunaRest : MonoBehaviour
{
    [Header("Healing")]
    public float restRate = 0.1f;
    public float gardenBonus = 0.15f;

    [Header("Sprites")]
    public GameObject restingSitSprite;     // For Sparkling Myst / Moonbow rest
    public GameObject restingLaySprite;     // For normal/garden/mid-forest rest
    public SpriteRenderer normalSprite;

    [Header("Detection")]
    public bool isInGarden = false;
    public bool isInSparklingMyst = false;

    [Header("State")]
    public bool isResting = false;

    private Character _character;
    private Health _health;
    private float _restStartTime;

    void Awake()
    {
        _character = GetComponent<Character>();
        _health = GetComponent<Health>();
    }

    void Update()
    {
        if (_health == null) return;

        // -------------------------
        // TOGGLE REST (manual Z)
        // -------------------------
        if (Input.GetKeyDown(KeyCode.Z))
        {
            if (!isResting) StartResting(manual: true);
            return; // never let Z cancel rest
        }

        // -----------------------------------------
        // CANCEL REST — any input except Z cancels
        // -----------------------------------------
        if (isResting && Time.time - _restStartTime > 0.15f)
        {
            if (Input.anyKeyDown ||
                Mathf.Abs(Input.GetAxisRaw("Horizontal")) > 0.01f ||
                Mathf.Abs(Input.GetAxisRaw("Vertical")) > 0.01f)
            {
                StopResting();
                return;
            }
        }

        // -------------------------
        // HEALING WHILE RESTING
        // -------------------------
        if (isResting)
        {
            float rate = restRate;
            if (isInGarden) rate += gardenBonus;

            float newHealth = Mathf.Clamp(
                _health.CurrentHealth + (rate * _health.MaximumHealth * Time.deltaTime),
                0f, _health.MaximumHealth
            );

            _health.SetHealth(newHealth, gameObject);
        }
    }

    // --------------------------------------------------
    // PUBLIC REST START — FOR AUTO REST (health system)
    // --------------------------------------------------
    public void BeginRestExternal() => StartResting(manual: false);
    public void EndRestExternal() => StopResting();

    // --------------------------------------------------
    // INTERNAL START / STOP REST
    // --------------------------------------------------
    private void StartResting(bool manual)
    {
        if (isResting) return;

        isResting = true;
        _restStartTime = Time.time;

        // freeze movement without disabling controller
        _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);

        // choose correct sprite
        normalSprite.enabled = false;

        if (isInSparklingMyst)
        {
            restingSitSprite.SetActive(true);
            restingLaySprite.SetActive(false);
        }
        else
        {
            restingLaySprite.SetActive(true);
            restingSitSprite.SetActive(false);
        }

        Debug.Log("🌙 Luna started resting");
    }

    private void StopResting()
    {
        if (!isResting) return;
        isResting = false;

        // restore control
        _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);

        // restore sprites
        restingSitSprite.SetActive(false);
        restingLaySprite.SetActive(false);
        normalSprite.enabled = true;

        Debug.Log("💤 Luna stopped resting");
    }

    // --------------------------------------------------
    // TRIGGER AREAS
    // --------------------------------------------------
    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
            isInGarden = true;

        if (col.CompareTag("Mist"))
            isInSparklingMyst = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
            isInGarden = false;

        if (col.CompareTag("Mist"))
            isInSparklingMyst = false;
    }
}




/* using UnityEngine;
using MoreMountains.CorgiEngine;

public class LunaRest : MonoBehaviour
{
    [Header("Health")]
    public float restRate = 0.1f;
    public float gardenBonus = 0.15f;

    [Header("Visuals")]
    public GameObject lunaRestingSprite;
    public SpriteRenderer normalSprite;

    private Health _health;
    private Character _character;
    private CorgiController _controller;

    public bool isInGarden;
    public bool isResting;

    private Vector2 lastPos;

    void Start()
    {
        _health = GetComponent<Health>();
        _character = GetComponent<Character>();
        _controller = GetComponent<CorgiController>();
        lastPos = transform.position;
    }

    void Update()
    {
        if (_health == null) return;

        // Toggle rest with Z inside garden
        if (isInGarden && Input.GetKeyDown(KeyCode.Z))
        {
            if (!isResting) StartResting();
            else StopResting();
        }

        if (!isResting) return;

        // heal
        float healRate = restRate + (isInGarden ? gardenBonus : 0f);
        float newHealth = Mathf.Clamp(
            _health.CurrentHealth + healRate * _health.MaximumHealth * Time.deltaTime,
            0f, _health.MaximumHealth);
        _health.SetHealth(newHealth, gameObject);

        // cancel on movement
        if (Vector2.Distance(transform.position, lastPos) > 0.03f)
        {
            StopResting();
            return;
        }

        lastPos = transform.position;
    }

    public void StartResting()
    {
        isResting = true;
        lastPos = transform.position;

        // Freeze input/state, not controller
        if (_character)
            _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Frozen);

        if (lunaRestingSprite) lunaRestingSprite.SetActive(true);
        if (normalSprite) normalSprite.enabled = false;

        Debug.Log("🌙 Luna is resting");
    }

    public void StopResting()
    {
        isResting = false;

        if (_character)
            _character.ConditionState.ChangeState(CharacterStates.CharacterConditions.Normal);

        if (lunaRestingSprite) lunaRestingSprite.SetActive(false);
        if (normalSprite) normalSprite.enabled = true;

        Debug.Log("💤 Luna stopped resting");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden")) isInGarden = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = false;
            if (isResting) StopResting();
        }
    }

    // External helper hooks (fixes all your calling scripts)
    public void BeginRestExternal()
    {
        if (!isResting)
            StartResting();
    }

    public void EndRestExternal()
    {
        if (isResting)
            StopResting();
    }
}
*/



/* using UnityEngine;
using MoreMountains.CorgiEngine;

public class LunaRest : MonoBehaviour
{
    [Header("Health Settings")]
    public float restRate = 0.1f;
    public float gardenBonus = 0.15f;
    public bool isInGarden;
    public bool isResting;

    [Header("Visuals")]
    public GameObject lunaRestingSprite;
    public SpriteRenderer normalSprite;

    [Header("Movement Cancel Settings")]
    [Tooltip("How much movement cancels rest.")]
    public float moveCancelThreshold = 0.03f;
    [Tooltip("Allow key input to instantly break rest.")]
    public bool cancelOnInput = true;

    private Vector2 lastPosition;
    private Health _health;
    private CorgiController _controller;
    private CharacterHorizontalMovement _movement;

    void Start()
    {
        _health = GetComponent<Health>();
        _controller = GetComponent<CorgiController>();
        _movement = GetComponent<CharacterHorizontalMovement>();
        lastPosition = transform.position;

        if (isResting)
            StartResting();
    }

    void Update()
    {
        if (_health == null) return;

        // ✅ Detect rest key
        if (isInGarden && Input.GetKeyDown(KeyCode.Z))
        {
            if (isResting) StopResting();
            else StartResting();
        }

        // ✅ Cancel if movement input or motion occurs
        if (isResting)
        {
            if (cancelOnInput && (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.RightArrow) ||
                                  Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D)))
            {
                StopResting();
                return;
            }

            float healRate = restRate + (isInGarden ? gardenBonus : 0f);
            float newHealth = Mathf.Clamp(
                _health.CurrentHealth + healRate * _health.MaximumHealth * Time.deltaTime,
                0f, _health.MaximumHealth
            );
            _health.SetHealth(newHealth, gameObject);

            Vector2 currentPosition = transform.position;
            if (Vector2.Distance(currentPosition, lastPosition) > moveCancelThreshold)
            {
                StopResting();
                return;
            }

            lastPosition = currentPosition;
        }
        else
        {
            lastPosition = transform.position;
        }
    }

    public void StartResting()
    {
        // 💤 Disable player control while resting
        if (_controller != null) _controller.enabled = false;
        if (_movement != null) _movement.enabled = false;

        isResting = true;
        if (lunaRestingSprite) lunaRestingSprite.SetActive(true);
        if (normalSprite) normalSprite.enabled = false;

        lastPosition = transform.position;
        Debug.Log("🌙 Luna is resting");
    }

    public void StopResting()
    {
        // 🌕 Re-enable controls when done resting
        if (_controller != null) _controller.enabled = true;
        if (_movement != null) _movement.enabled = true;

        isResting = false;
        if (lunaRestingSprite) lunaRestingSprite.SetActive(false);
        if (normalSprite) normalSprite.enabled = true;

        Debug.Log("💤 Luna stopped resting");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden")) isInGarden = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = false;
            if (isResting) StopResting();
        }
    }

    // External hooks
    public void BeginRestExternal() => StartResting();
    public void EndRestExternal() => StopResting();
}
*/



/* using UnityEngine;
using MoreMountains.CorgiEngine;

public class LunaRest : MonoBehaviour
{
    [Header("Health Settings")]
    public float restRate = 0.1f;
    public float gardenBonus = 0.15f;
    public bool isInGarden;
    public bool isResting;

    [Header("Visuals")]
    public GameObject lunaRestingSprite;
    public SpriteRenderer normalSprite;

    [Header("Movement")]
    public float moveCancelThreshold = 0.03f;

    private Vector2 lastPosition;
    private Health _health;

    void Start()
    {
        _health = GetComponent<Health>();
        lastPosition = transform.position;

        if (isResting)
        {
            StartResting();
        }
    }

    void Update()
    {
        if (_health == null) return;

        if (isInGarden && Input.GetKeyDown(KeyCode.Z))
        {
            if (isResting) StopResting();
            else StartResting();
        }

        if (isResting)
        {
            float healRate = restRate + (isInGarden ? gardenBonus : 0f);
            float newHealth = Mathf.Clamp(
                _health.CurrentHealth + healRate * _health.MaximumHealth * Time.deltaTime,
                0f, _health.MaximumHealth
            );
            _health.SetHealth(newHealth, gameObject);

            // cancel if Luna moves
            Vector2 currentPosition = transform.position;
            if (Vector2.Distance(currentPosition, lastPosition) > moveCancelThreshold)
            {
                StopResting();
                return;
            }
            lastPosition = currentPosition;
        }
        else
        {
            lastPosition = transform.position;
        }
    }

    public void StartResting()
    {
        isResting = true;
        if (lunaRestingSprite) lunaRestingSprite.SetActive(true);
        if (normalSprite) normalSprite.enabled = false;
        lastPosition = transform.position;
        Debug.Log("🌙 Luna is resting");
    }

    public void StopResting()
    {
        isResting = false;
        if (lunaRestingSprite) lunaRestingSprite.SetActive(false);
        if (normalSprite) normalSprite.enabled = true;
        Debug.Log("💤 Luna stopped resting");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden")) isInGarden = true;
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = false;
            if (isResting) StopResting();
        }
    }
    // --- add at the very bottom of LunaRest.cs, right before the final closing brace ---
// Compatibility helpers for other scripts
public void BeginRestExternal()
{
    StartResting();
}

public void EndRestExternal()
{
    StopResting();
}


}

*/



/* using UnityEngine;

public class LunaRest : MonoBehaviour
{
    public float restRate = 0.2f; // Health per second (tweak this!)
    public float maxHealth = 1f;
    public float currentHealth = 0.5f; // Example starting point

    public GameObject lunaRestingSprite; // Assign in Inspector
    public SpriteRenderer normalSprite;

    public bool isInGarden = false;
    public bool isResting = false;
    private Vector2 lastPosition;

    void Update()
    {
        if (isInGarden && Input.GetKeyDown(KeyCode.Z) && !isResting)
        {
            StartResting();
        }

        if (isResting)
        {
            // Refill health
            currentHealth = Mathf.Clamp(currentHealth + restRate * Time.deltaTime, 0f, maxHealth);

            // Detect movement to cancel
            Vector2 currentPosition = new Vector2(transform.position.x, transform.position.y);
            if ((currentPosition - lastPosition).magnitude > 0.01f)
            {
                StopResting();
            }

            lastPosition = currentPosition;
        }
    }

    void StartResting()
    {
        isResting = true;
        lastPosition = new Vector2(transform.position.x, transform.position.y);

        if (lunaRestingSprite != null) lunaRestingSprite.SetActive(true);
        if (normalSprite != null) normalSprite.enabled = false;

        Debug.Log("🌿 Luna is resting...");
    }

    void StopResting()
    {
        isResting = false;

        if (lunaRestingSprite != null) lunaRestingSprite.SetActive(false);
        if (normalSprite != null) normalSprite.enabled = true;

        Debug.Log("🌿 Luna stopped resting.");
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = true;
        }
    }

    void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = true;
        }
    }

    void OnTriggerExit2D(Collider2D col)
    {
        if (col.CompareTag("Garden"))
        {
            isInGarden = false;
            if (isResting)
            {
                StopResting();
            }
        }
    }

    // LunaRest.cs  (add anywhere inside the class)
public void BeginRestExternal()
{
    if (!isResting) StartResting();   // uses your existing private method
}

public void EndRestExternal()
{
    if (isResting) StopResting();     // uses your existing private method
}

}
*/