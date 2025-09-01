using UnityEngine;
using Cinemachine;
using System.Collections;

public class ButterflyFlyHandler : MonoBehaviour
{
    [Header("References")]
    public GameObject luna;
    public GameObject lunaInFlight;
    public Transform butterfly;
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public SpriteRenderer butterflyRenderer;
    public FollowAndFlip followAndFlip;
    public GameObject butterflySpeechBubble;
    public GameObject sparklePrefab;

    [Header("Animation Speeds")]
    public float animationSpeedFlying = 2f;
    public float animationSpeedNormal = 1f;

    [Header("Movement & Jump")]
    public float baseSpeed       = 2.2f;
    public float bonusSpeed      = 0f;
    public float jumpForce       = 5f;
    public Vector3 dismountOffset= new Vector3(0f, -0.5f, 0f);

    [Header("Flight Timing")]
    public float flightDuration   = 5f;
    public float warningTime      = 1f;
    public float cooldownDuration = 3f;

    [Header("Colors")]
    public Color normalColor;
    public Color warningColor;
    public Color cooldownColor;

    [Header("Tea Rose Pollen")]
    public GameObject teaRosePollenFX;
    public int   teaRosePollenCount   = 0;
    public float speedBoostPerPollen  = 1f;
    public int   pollenThreshold      = 3;

    [Header("Spore Attach Points")]
    public Transform groundSporeAttachPoint;
    public Transform flightSporeAttachPoint;

    // **Added for LunaSporeSystem’s justDismounted check**
    [HideInInspector] public bool justDismounted = false;

    // internals
    private Rigidbody2D     butterflyRb;
    private Collider2D[]    butterflyColliders;
    private Rigidbody2D     lunaRb;
    private float           lunaInitialGravity;
    private SpriteRenderer  lunaGroundSprite;
    private Collider2D[]    lunaColliders;
    private Collider2D[]    flightColliders;
    private LunaSporeSystem sporeSystem;
    private FlowerHolder    flowerHolder;

    private float flightTimer      = 0f;
    private bool  isFlying         = false;
    private bool  warningTriggered = false;
    private bool  isInCooldown     = false;
    private bool  canBeExtended    = false;
    private bool  hasTempBoost     = false;
    private float nextTempBoost    = 0f;

    void Start()
    {
        butterflyRb        = butterfly.GetComponent<Rigidbody2D>();
        butterflyColliders = butterfly.GetComponentsInChildren<Collider2D>();

        lunaRb             = luna.GetComponent<Rigidbody2D>();
        if (lunaRb != null) lunaInitialGravity = lunaRb.gravityScale;

        lunaGroundSprite   = luna.GetComponent<SpriteRenderer>();
        lunaColliders      = luna.GetComponentsInChildren<Collider2D>();
        flightColliders    = lunaInFlight.GetComponentsInChildren<Collider2D>();

        sporeSystem        = luna.GetComponent<LunaSporeSystem>();
        flowerHolder       = luna.GetComponent<FlowerHolder>();

        // match what LunaSporeSystem already has
        groundSporeAttachPoint = sporeSystem.attachPoint;

        lunaGroundSprite.enabled = true;
        lunaInFlight.SetActive(false);
        butterflyRenderer.color  = normalColor;
        butterflyRb.bodyType     = RigidbodyType2D.Kinematic;
        butterflyRb.gravityScale = 0f;
        sparklePrefab?.SetActive(false);
        teaRosePollenFX?.SetActive(false);
    }

    void Update()
    {
        if (isInCooldown) return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isFlying) Dismount(false);
            else          Mount();
            return;
        }

        if (!isFlying) return;

        float moveSpeed = baseSpeed + bonusSpeed + (hasTempBoost ? nextTempBoost : 0f);
        if (hasTempBoost)
        {
            hasTempBoost = false;
            teaRosePollenFX?.SetActive(false);
        }

        HandleMovement(moveSpeed);
        HandleFlip();
        HandleTimer();

        if (Input.GetKeyDown(KeyCode.R) 
         && sporeSystem.HasSporeAttached 
         && canBeExtended)
        {
            ExtendFlightWithSpore();
        }

        if (Input.GetButtonDown("Jump"))
            Dismount(true);

        if (Input.GetKeyDown(KeyCode.X))
            HandleAirFlowerInteract();
    }

    private void Mount()
    {
        justDismounted = false;

        lunaGroundSprite.enabled = false;
        lunaInFlight.SetActive(true);

        sporeSystem.attachPoint = flightSporeAttachPoint;

        foreach (var c in butterflyColliders) c.enabled = false;
        foreach (var c in lunaColliders)      c.enabled = false;
        foreach (var c in flightColliders)    c.enabled = false;

        vCam.Follow = butterfly;
        butterflySpeechBubble?.SetActive(false);
        followAndFlip.enabled = false;
        sparklePrefab?.SetActive(true);
        sparklePrefab.transform.SetParent(butterfly, false);

        bool facingLeft = luna.transform.localScale.x < 0f;
        butterflyRenderer.flipX = facingLeft;
        var ls = lunaInFlight.transform.localScale;
        ls.x = facingLeft ? -Mathf.Abs(ls.x) : Mathf.Abs(ls.x);
        lunaInFlight.transform.localScale = ls;

        butterflyAnimator.speed   = animationSpeedFlying;
        butterflyRb.bodyType      = RigidbodyType2D.Dynamic;
        butterflyRb.gravityScale  = 0f;

        flightTimer      = 0f;
        warningTriggered = false;
        canBeExtended    = false;
        isFlying         = true;
    }

    private void Dismount(bool jumpOff)
    {
        lunaInFlight.SetActive(false);
        lunaGroundSprite.enabled = true;

        sporeSystem.attachPoint = groundSporeAttachPoint;

        foreach (var c in butterflyColliders) c.enabled = true;
        foreach (var c in lunaColliders)      c.enabled = true;
        foreach (var c in flightColliders)    c.enabled = true;

        if (lunaRb != null)
        {
            lunaRb.velocity     = Vector2.zero;
            lunaRb.gravityScale = lunaInitialGravity;
        }

        luna.transform.position = butterfly.position + dismountOffset;
        vCam.Follow = luna.transform;

        followAndFlip.enabled = true;
        butterflySpeechBubble?.SetActive(true);
        sparklePrefab?.SetActive(false);
        butterflyAnimator.speed = animationSpeedNormal;

        if (jumpOff && lunaRb != null)
            lunaRb.velocity = new Vector2(lunaRb.velocity.x, jumpForce);

        // set the flag so LunaSporeSystem skips its detach lock
        justDismounted = true;

        isInCooldown = true;
        Invoke(nameof(EndCooldown), cooldownDuration);
        isFlying = false;
    }

    private void HandleMovement(float speed)
    {
        float hx = Input.GetAxis("Horizontal");
        float hy = Input.GetAxis("Vertical");
        if (Mathf.Abs(hx) < 0.01f)
        {
            if (Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
            if (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)) hx =  1f;
        }
        butterfly.position += new Vector3(hx, hy, 0f) * speed * Time.deltaTime;
    }

    private void HandleFlip()
    {
        float hx = Input.GetAxis("Horizontal");
        if (Mathf.Abs(hx) < 0.01f)
        {
            if      (Input.GetKey(KeyCode.A)||Input.GetKey(KeyCode.LeftArrow))  hx = -1f;
            else if (Input.GetKey(KeyCode.D)||Input.GetKey(KeyCode.RightArrow)) hx =  1f;
        }
        if (hx != 0f)
        {
            bool left = hx < 0f;
            butterflyRenderer.flipX = left;
            var ls = lunaInFlight.transform.localScale;
            ls.x = left ? -Mathf.Abs(ls.x) : Mathf.Abs(ls.x);
            lunaInFlight.transform.localScale = ls;
        }
    }

    private void HandleTimer()
    {
        flightTimer += Time.deltaTime;
        if (!warningTriggered && flightTimer >= flightDuration - warningTime)
        {
            warningTriggered = true;
            canBeExtended    = true;
            StartCoroutine(FlashWarningColor());
        }
        if (flightTimer >= flightDuration)
        {
            Dismount(false);
        }
    }

    private void ExtendFlightWithSpore()
    {
        flightTimer = Mathf.Max(0f, flightTimer - warningTime);
        sporeSystem.DestroyAttachedSpore();
        canBeExtended = false;
        butterflyRenderer.color = normalColor;
    }

    private void HandleAirFlowerInteract()
    {
        var hits = Physics2D.OverlapCircleAll(butterfly.position, 1f);
        foreach (var col in hits)
        {
            if (col.CompareTag("Flower") && !flowerHolder.HasFlower)
            {
                flowerHolder.PickUpFlower(col.gameObject);
                return;
            }
            var spot = col.GetComponent<GardenSpot>();
            if (spot != null && flowerHolder.HasFlower)
            {
                var f = flowerHolder.GetHeldFlower();
                f.transform.SetParent(spot.transform, false);
                f.transform.localPosition = Vector3.zero;
                flowerHolder.DropFlower();
                var spr = f.GetComponent<SproutAndLightManager>();
                if (spr != null) spr.isPlanted = true;
                return;
            }
        }
    }

    private IEnumerator FlashWarningColor()
    {
        for (int i = 0; i < 3; i++)
        {
            butterflyRenderer.color = warningColor;
            yield return new WaitForSeconds(0.2f);
            butterflyRenderer.color = cooldownColor;
            yield return new WaitForSeconds(0.2f);
        }
        butterflyRenderer.color = cooldownColor;
    }

    private void EndCooldown()
    {
        isInCooldown            = false;
        justDismounted          = false;
        butterflyRenderer.color = normalColor;
    }

    public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;
        hasTempBoost   = true;
        nextTempBoost  = speedBoost;
        teaRosePollenFX?.SetActive(true);
        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed += speedBoost;
            teaRosePollenCount = 0;
        }
    }
}





/* using UnityEngine;
using Cinemachine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class ButterflyFlyHandler : MonoBehaviour
{
    public GameObject luna;
    public GameObject lunaInFlight;
    public GameObject butterflySpeechBubble; // Reference to the speech bubble
    public FollowAndFlip followAndFlip; // Reference to the FollowAndFlip script
    public Transform butterfly;
    public CinemachineVirtualCamera vCam;
    public Animator butterflyAnimator;
    public GameObject sparklePrefab;
    public Vector3 sparkleOffset = Vector3.zero; // Public offset for sparkle prefab
    public float moveSpeed = 5f;
    public float animationSpeedFlying = 2f;
    public float animationSpeedNormal = 1f;
    public Vector3 dismountOffset = new Vector3(0f, -0.5f, 0f);
    public float flightDuration = 5f;
    public float warningTime = 1f;
    public float cooldownDuration = 3f;
    public Color cooldownColor = Color.gray;
    public Color normalColor = Color.white;
    public Color warningColor = Color.red;
    public Color aidableColor = Color.green;

    public GameObject teaRosePollenFX; // Assign this in Inspector (set it inactive in prefab)
    public float baseSpeed = 2.2f; // Original speed
    public float bonusSpeed = 0f; // Stackable bonus
    public int teaRosePollenCount = 0; // Tracks how many collected
    public int permanentBoosts = 0; // For tracking cycles
    private bool hasTemporaryBoost = false;
    private float nextFlightSpeedBoost = 0f;

    public bool isFlying = false;
    public bool justDismounted = false;
    public float storedHealth = -1f; // Stores Luna's health before dismounting
    private bool isInCooldown = false;
    private bool canBeAided = false; // Aidable state flag
    private bool warningTriggered = false;
    private float flightTimer = 0f;
    private SpriteRenderer butterflyRenderer;
    private bool initialFlipX; // Tracks the initial flip state of the butterfly
    private bool isFlippedRelativeToInitial; // Tracks if the butterfly is flipped relative to its initial state

    private Rigidbody2D butterflyRb; // Reference to butterfly's Rigidbody2D
    private PresentSpore presentSpore; // Reference to check Luna's spore state

    void Start()
    {
        if (luna != null) luna.SetActive(true);
        if (lunaInFlight != null) lunaInFlight.SetActive(false);

        butterflyRenderer = butterfly.GetComponent<SpriteRenderer>();
        butterflyRb = butterfly.GetComponent<Rigidbody2D>();
        if (butterflyRenderer != null)
        {
            butterflyRenderer.color = normalColor;
            initialFlipX = butterflyRenderer.flipX; // Store the initial flip state
            isFlippedRelativeToInitial = false; // Initially, it is not flipped relative to itself
        }

        if (butterflyRb != null)
        {
            butterflyRb.bodyType = RigidbodyType2D.Kinematic; // Start as Kinematic
            butterflyRb.gravityScale = 0f; // Ensure no gravity
        }

        presentSpore = luna.GetComponent<PresentSpore>(); // Reference to Luna's PresentSpore script
        if (sparklePrefab != null) sparklePrefab.SetActive(false);
    }

    void Update()
    {
        if (isInCooldown)
        {
            // Check for Aid action and update Aidable Color
            UpdateAidableColor();

            if (canBeAided && Input.GetKeyDown(KeyCode.A))
            {
                ResetCooldownExplicitly();
            }
            return; // Skip updates if in cooldown
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isFlying)
            {
                Dismount();
            }
            else
            {
                Mount();
            }
        }

        if (isFlying)
        {
            HandleButterflyMovement();
            HandleButterflyFlip();
            HandleFlightTimer();
        }
    }

    private void Mount()
    {
        if (luna == null || lunaInFlight == null || butterfly == null || vCam == null) return;

        luna.SetActive(false); // Hide grounded Luna
        lunaInFlight.SetActive(true); // Show flying Luna
        vCam.Follow = butterfly;

        if (butterflySpeechBubble != null)
        {
            butterflySpeechBubble.SetActive(false); // Disable the entire GameObject
            Debug.Log("Speech bubble GameObject disabled during flight.");
        }

        if (followAndFlip != null)
        {
            followAndFlip.enabled = false; // Disable FollowAndFlip script
            Debug.Log("FollowAndFlip script disabled during flight.");
        }

        if (sparklePrefab != null)
        {
            sparklePrefab.SetActive(true);
            sparklePrefab.transform.parent = butterfly; // Parent to butterfly
            sparklePrefab.transform.localPosition = sparkleOffset; // Apply offset
        }

        if (butterflyAnimator != null) butterflyAnimator.speed = animationSpeedFlying;

        flightTimer = 0f;
        warningTriggered = false; // Reset warning for new flight
        isFlying = true;

        // Adjust butterfly facing direction based on Luna's approach
        bool approachFromLeft = luna.transform.position.x < butterfly.position.x;
        isFlippedRelativeToInitial = approachFromLeft != initialFlipX;
        butterflyRenderer.flipX = isFlippedRelativeToInitial;

        // Make the butterfly dynamic for flight
       if (butterflyRb != null)
    {
        butterflyRb.bodyType = RigidbodyType2D.Dynamic;
        butterflyRb.gravityScale = 0f;
    }

    // Apply speed boost logic
    moveSpeed = baseSpeed + bonusSpeed;

    if (hasTemporaryBoost)
    {
        moveSpeed += nextFlightSpeedBoost;
        hasTemporaryBoost = false; // Use up the temp boost
        nextFlightSpeedBoost = 0f;
        Debug.Log("🚀 Temporary Tea Rose speed boost applied for this flight!");
    }

    if (hasTemporaryBoost)
    {
        moveSpeed += nextFlightSpeedBoost;
        hasTemporaryBoost = false;
        nextFlightSpeedBoost = 0f;
        Debug.Log("🚀 Temporary Tea Rose speed boost applied for this flight!");
    }

    // Turn off pollen FX when boost is used
        if (teaRosePollenFX != null)
        {
            teaRosePollenFX.SetActive(false);
        }

    }

   private void Dismount()
{
    if (luna == null || lunaInFlight == null || butterfly == null || vCam == null) return;

    // ✅ Store Luna's current health before she gets disabled
    Health lunaHealth = luna.GetComponent<Health>();
    if (lunaHealth != null)
    {
        storedHealth = lunaHealth.CurrentHealth;
        Debug.Log("💾 Stored Health Before Dismount: " + storedHealth);

        // ✅ Stop the health bar from updating temporarily
        lunaHealth.UpdateHealthBar(false);
    }

    luna.SetActive(true); // Reactivate grounded Luna
    lunaInFlight.SetActive(false); // Deactivate flying Luna
    luna.transform.position = butterfly.position + dismountOffset;
    vCam.Follow = luna.transform;

    if (butterflySpeechBubble != null)
    {
        butterflySpeechBubble.SetActive(true);
    }

    if (followAndFlip != null)
    {
        followAndFlip.enabled = true;
    }

    if (sparklePrefab != null)
    {
        sparklePrefab.SetActive(false);
        sparklePrefab.transform.parent = null;
    }

    if (butterflyAnimator != null) butterflyAnimator.speed = animationSpeedNormal;

    StartCoroutine(RestoreHealthAfterDismount()); // ✅ Restore health after OnEnable() runs

    StartCooldown();
    isFlying = false;

    butterflyRenderer.flipX = initialFlipX;

    if (butterflyRb != null)
    {
        butterflyRb.bodyType = RigidbodyType2D.Kinematic;
    }
}





private IEnumerator DisableSporeActionsTemporarily()
{
    Debug.Log("Temporarily disabling spore interactions after dismount...");

    LunaSporeSystem sporeSystem = luna.GetComponent<LunaSporeSystem>();
    if (sporeSystem != null)
    {
        sporeSystem.SetHealthLock(true); // Lock health gain
    }

    yield return new WaitForSeconds(3f); // Adjust time as needed

    if (sporeSystem != null)
    {
        sporeSystem.SetHealthLock(false); // Unlock health gain
        sporeSystem.ResetSporeState();  // Fully reset spore-related state
    }

    Debug.Log("Spore interactions re-enabled.");
}

 private IEnumerator RestoreHealthAfterDismount()
{
    yield return new WaitForSeconds(0.1f); // Small delay to let OnEnable() run first

    if (luna != null)
    {
        Health lunaHealth = luna.GetComponent<Health>();
        if (lunaHealth != null && storedHealth > 0)
        {
            Debug.Log("🔄 Restoring Health After Dismount: " + storedHealth);
            lunaHealth.SetHealth(storedHealth, luna.gameObject);

            // ✅ NOW update the health bar after restoring the correct value
            lunaHealth.UpdateHealthBar(true);
        }
    }
}
   

private IEnumerator DisableHealthGainTemporarily()
{
    Debug.Log("Health gain locked after dismount.");
    LunaSporeSystem sporeSystem = luna.GetComponent<LunaSporeSystem>();

    if (sporeSystem != null)
    {
        sporeSystem.SetHealthLock(true);
    }

    yield return new WaitForSeconds(10f); // Adjust the duration if needed

    if (sporeSystem != null)
    {
        sporeSystem.SetHealthLock(false);
    }

    Debug.Log("Health gain unlocked.");
}


    private void HandleButterflyMovement()
    {
        float moveX = Input.GetAxis("Horizontal") * moveSpeed * Time.deltaTime;
        float moveY = Input.GetAxis("Vertical") * moveSpeed * Time.deltaTime;
        butterfly.position += new Vector3(moveX, moveY, 0f);
    }

    private void HandleButterflyFlip()
    {
        float moveX = Input.GetAxis("Horizontal");

        if (moveX != 0)
        {
            bool shouldFlip = (moveX < 0) != isFlippedRelativeToInitial;

            if (butterflyRenderer != null)
            {
                butterflyRenderer.flipX = shouldFlip;

                // Ensure LunaInFlight aligns with the butterfly
                if (lunaInFlight != null)
                {
                    Vector3 localScale = lunaInFlight.transform.localScale;
                    localScale.x = shouldFlip ? -Mathf.Abs(localScale.x) : Mathf.Abs(localScale.x);
                    lunaInFlight.transform.localScale = localScale;
                }
            }
        }
    }

    private void StartCooldown()
    {
        isInCooldown = true;
        canBeAided = true; // Butterfly enters Aidable state
        UpdateAidableColor(); // Set initial aidable color based on Luna's spore state

        Invoke(nameof(EndCooldown), cooldownDuration);
    }

    private void EndCooldown()
    {
        isInCooldown = false;
        canBeAided = false;
        if (butterflyRenderer != null)
        {
            butterflyRenderer.color = normalColor; // Reset to normal color
        }
    }

    public void ResetCooldownExplicitly()
    {
        isInCooldown = false;
        canBeAided = false;
        if (butterflyRenderer != null)
        {
            butterflyRenderer.color = normalColor; // Reset to normal color
        }

        CancelInvoke(nameof(EndCooldown)); // Stop cooldown timer
        Debug.Log("Cooldown reset explicitly via Aid.");
    }

    private void HandleFlightTimer()
    {
        flightTimer += Time.deltaTime;

        // Trigger warning if nearing the flight duration limit
        if (!warningTriggered && flightTimer >= flightDuration - warningTime)
        {
            if (butterflyRenderer != null)
            {
                StartCoroutine(FlashWarningColor());
            }
            warningTriggered = true;
        }

        if (flightTimer >= flightDuration)
        {
            Dismount();
        }
    }

    private System.Collections.IEnumerator FlashWarningColor()
    {
        int flashCount = 3;
        for (int i = 0; i < flashCount; i++)
        {
            if (butterflyRenderer != null)
            {
                butterflyRenderer.color = warningColor;
            }
            yield return new WaitForSeconds(0.2f);

            if (butterflyRenderer != null)
            {
                butterflyRenderer.color = cooldownColor;
            }
            yield return new WaitForSeconds(0.2f);
        }

        // Hold cooldown color after flashing
        if (butterflyRenderer != null)
        {
            butterflyRenderer.color = cooldownColor;
        }
    }

    private void UpdateAidableColor()
    {
        if (canBeAided && presentSpore != null && presentSpore.HasSporeAttached)
        {
            if (butterflyRenderer != null)
            {
                butterflyRenderer.color = aidableColor; // Set aidable color
            }
        }
        else if (butterflyRenderer != null)
        {
            butterflyRenderer.color = cooldownColor; // Default to cooldown color
        }
    }

   public void ApplyTeaRosePollen(float speedBoost, int threshold)
    {
        teaRosePollenCount++;

        hasTemporaryBoost = true;
        nextFlightSpeedBoost = speedBoost;

        // Show pollen FX
        if (teaRosePollenFX != null)
        {
            teaRosePollenFX.SetActive(true);
        }

        Debug.Log("Tea Rose Pollen collected! Count: " + teaRosePollenCount);

        if (teaRosePollenCount >= threshold)
        {
            bonusSpeed += speedBoost;
            permanentBoosts++;
            teaRosePollenCount = 0;
            Debug.Log("Permanent Tea Rose Speed Boost applied! Total bonus: " + bonusSpeed);
        }
    }


}

// Works - using the cooldown color as the player's indication that the butterfly is ready to be aided.

*/