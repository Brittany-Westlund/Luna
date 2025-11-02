using UnityEngine;
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