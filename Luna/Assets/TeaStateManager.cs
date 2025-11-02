using System.Collections;
using UnityEngine;
using UnityEngine.UI;


public class TeaStateManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject teapotPrefab;
    public KeyCode    teaKey = KeyCode.T;
    public float lilyStoolSearchRadius = 1.5f; // How close you need to be to a lilystool to spawn a teapot
   
    private TeacupInventory     _teacupInventory;
    private GameObject          _currentTeapot;
    private TeapotLightReceiver _currentReceiver;

    private bool justDrankTea = false;
private float lastDrinkTime = -1f;

   
    [Header("SFX")]
    public AudioSource spawnAudioSource;
    public AudioSource storeAudioSource;

    [Header("Hint Icon (shown when not near Lilystool)")]
    public GameObject lilystoolHintIcon; // Drag Luna's child icon here
    public float hintShowTime = 2f;
    public TeacupHighlight lunaHighlight;
    public float highlightTime = 1f; // Public: control how long the highlight lasts
    private Coroutine highlightCoroutine;
    void Start()
    {
        _teacupInventory = GetComponent<TeacupInventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teaKey))
            HandleTeaLogic();

        // Only show highlights while holding a teacup
        if (_teacupInventory.HasTeacup())
        {
            UpdateDrinkTargetHighlight();
        }
        else
        {
            RemoveDrinkTargetHighlight();
        }
    }

    void HandleTeaLogic()
{
    Debug.Log("🍵 HandleTeaLogic: Attempting to brew (should see this when T is pressed and teapot is lit)");

    var hydrationGate = FindObjectOfType<TeaHydrationInputBlocker>();
    bool hydrationTooLow = hydrationGate != null && hydrationGate.IsHydrationTooLow();

    // 1️⃣ If Luna already has a teacup (drinking/giving): always allowed
    if (_teacupInventory.HasTeacup())
    {
            if (IsNearNPC())
            {
                _teacupInventory.TryGiveTeacupToNPC();
                RemoveDrinkTargetHighlight();
            }
            else
            {
                _teacupInventory.DrinkTeacup();
                RemoveDrinkTargetHighlight();
            }
        // 🧭 Do NOT show hint here — drinking is allowed, no hydration gate yet
       
        justDrankTea = true;
        lastDrinkTime = Time.time;
        return;
    }

        // 2️⃣ If hydration too low, but make sure we *didn't just drink*
if (hydrationTooLow)
{
    // ✅ Require at least a short pause after drinking before showing hint
    if (!justDrankTea || Time.time - lastDrinkTime > 1.0f)
    {
        Debug.Log("🛑 Too dehydrated — cannot brew or spawn new teapot.");
        if (hydrationGate != null)
            StartCoroutine(hydrationGate.PulseNearestLilystoolHint());
    }
    return;
}

// 3️⃣ No teapot in scene → try to spawn one near LilyStool
if (_currentTeapot == null)
{
    // ✅ Reset "just drank" only now that she’s actually trying to brew again
    justDrankTea = false;

    LilyStool[] stools = FindObjectsOfType<LilyStool>();
    LilyStool nearest = null;
    float minDist = float.MaxValue;

        foreach (var stool in stools)
        {
            float dist = Vector2.Distance(transform.position, stool.transform.position);
            if (dist < lilyStoolSearchRadius && dist < minDist)
            {
                nearest = stool;
                minDist = dist;
            }
        }

        if (nearest == null)
        {
            Debug.Log("❌ No LilyStool nearby! Find one to place your teapot.");
            ShowLilystoolHint();
            return;
        }

        // ✅ Spawn teapot only if hydrated enough
        _currentTeapot = Instantiate(
            teapotPrefab,
            nearest.teapotSpawnPoint.position,
            Quaternion.identity
        );

        if (spawnAudioSource != null)
        {
            Debug.Log("PlaySpawnSFX called on manager!");
            spawnAudioSource.Play();
        }

        _currentReceiver = _currentTeapot.GetComponent<TeapotLightReceiver>();
        if (_currentReceiver == null)
            Debug.LogError("❌ Spawned teapot has no TeapotLightReceiver!");
        return;
    }

    // 5) Teapot exists but unlit → store or warn
    if (!_currentReceiver.HasLight)
    {
        if (_currentReceiver.GetIngredientCount() > 0)
        {
            Debug.Log("❗ You added flowers but haven't lit it yet—hit Q to light or remove them first.");
            return;
        }

        Debug.Log("🫖 Teapot was stored (empty).");
        float destroyDelay = 0f;
        if (storeAudioSource != null && storeAudioSource.clip != null)
        {
            Debug.Log("PlayStoreSFX called on manager!");
            storeAudioSource.Play();
            destroyDelay = storeAudioSource.clip.length;
        }

        Destroy(_currentTeapot, destroyDelay);
        _currentTeapot = null;
        _currentReceiver = null;
        return;
    }

    // 6) Teapot is lit → brew
    Debug.Log("🍵 Brewing tea (light detected)...");
    var cup = _currentReceiver.BrewTea();
    if (cup != null)
    {
        Debug.Log("🍵 BrewTea() succeeded, receiving cup");
        _teacupInventory.ReceiveTeacup(cup);
        Destroy(_currentTeapot);
        _currentTeapot = null;
        _currentReceiver = null;
    }
    else
    {
        Debug.LogWarning("❗ BrewTea() returned null—check prefab or spawn‐point");
    }
}


    private TeacupReceiver _lastNpcHighlighted = null;

    void UpdateDrinkTargetHighlight()
    {
        TeacupReceiver npc = GetNearbyNPC();

        // NPC in range
        if (npc != null)
        {
            // Remove highlight from Luna if needed
            if (lunaHighlight != null) lunaHighlight.RemoveHighlight();
            // Only add highlight to NPC if not already
            if (_lastNpcHighlighted != npc)
            {
                if (_lastNpcHighlighted != null)
                    _lastNpcHighlighted.GetComponent<TeacupHighlight>()?.RemoveHighlight();

                npc.GetComponent<TeacupHighlight>()?.Highlight();
                _lastNpcHighlighted = npc;
            }
        }
        else
        {
            // No NPC: Highlight Luna, remove from any previously highlighted NPC
            if (_lastNpcHighlighted != null)
            {
                _lastNpcHighlighted.GetComponent<TeacupHighlight>()?.RemoveHighlight();
                _lastNpcHighlighted = null;
            }
            if (lunaHighlight != null) lunaHighlight.Highlight();
        }
    }

    public void RemoveDrinkTargetHighlight()
    {
        if (_lastNpcHighlighted != null)
            _lastNpcHighlighted.GetComponent<TeacupHighlight>()?.RemoveHighlight();

        if (lunaHighlight != null)
            lunaHighlight.RemoveHighlight();

        _lastNpcHighlighted = null;
    }

    void ShowLilystoolHint()
    {
        if (lilystoolHintIcon != null)
            StartCoroutine(ShowHintForSeconds());
    }

    IEnumerator ShowHintForSeconds()
    {
        ToggleHintRenderers(true);
        yield return new WaitForSeconds(hintShowTime);
        ToggleHintRenderers(false);
    }

    bool IsNearNPC()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.35f);
        foreach (var h in hits)
            if (h.GetComponent<TeacupReceiver>() != null)
                return true;
        return false;
    }

    private TeacupReceiver GetNearbyNPC()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 0.35f);
        foreach (var h in hits)
        {
            var receiver = h.GetComponent<TeacupReceiver>();
            if (receiver != null)
                return receiver;
        }
        return null;
    }

private void ToggleHintRenderers(bool visible)
{
    if (lilystoolHintIcon == null) return;

    // SpriteRenderer (world-space icon)
    foreach (var sr in lilystoolHintIcon.GetComponentsInChildren<SpriteRenderer>(true))
        sr.enabled = visible;

    // UI Image (screen-space icon)
    foreach (var img in lilystoolHintIcon.GetComponentsInChildren<Image>(true))
        img.enabled = visible;

    // CanvasGroup (if used for fading)
    foreach (var cg in lilystoolHintIcon.GetComponentsInChildren<CanvasGroup>(true))
        cg.alpha = visible ? 1f : 0f;
}


}
