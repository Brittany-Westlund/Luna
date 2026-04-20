using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using PixelCrushers.DialogueSystem;

public class TeaStateManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject teapotPrefab;
    public KeyCode teaKey = KeyCode.T;
    public float lilyStoolSearchRadius = 1.5f;

    [Header("Teapot Control")]
    [SerializeField] private bool allowTeaKeyToSpawnTeapot = false;
    [SerializeField] private string teapotTag = "Teapot";
    [SerializeField] private float existingTeapotSearchRadius = 2f;

    [Header("Held Cup Input Buffer")]
    [Tooltip("Prevents the same keypress that brewed the tea from immediately consuming it.")]
    public float postReceiveUseBlockTime = 0.2f;

    [Header("SFX")]
    public AudioSource spawnAudioSource;
    public AudioSource storeAudioSource;

    [Header("Hint Icon (shown when not near Lilystool)")]
    public GameObject lilystoolHintIcon;
    public float hintShowTime = 2f;

    [Header("Highlights")]
    public TeacupHighlight lunaHighlight;
    public float highlightTime = 1f;

    private TeacupInventory _teacupInventory;
    private GameObject _currentTeapot;
    private TeapotLightReceiver _currentReceiver;

    private bool justDrankTea = false;
    private float lastDrinkTime = -1f;
    private float lastCupReceivedTime = -999f;

    private TeacupReceiver _lastNpcHighlighted = null;
    private LilyStool _currentLilystool;
    private LilystoolCandleController _currentLilystoolCandle;

    private void Start()
    {
        _teacupInventory = GetComponent<TeacupInventory>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(teaKey))
            HandleTeaLogic();

        if (_teacupInventory != null && _teacupInventory.HasTeacup())
            UpdateDrinkTargetHighlight();
        else
            RemoveDrinkTargetHighlight();

        // Clean up stale references if a teapot was destroyed elsewhere.
        if (_currentTeapot == null)
        {
            _currentTeapot = null;
            _currentReceiver = null;
        }
    }

    private void HandleTeaLogic()
    {
        Debug.Log("🍵 HandleTeaLogic: Attempting tea action.");

        if (_teacupInventory == null)
        {
            Debug.LogError("[TeaStateManager] No TeacupInventory found on player.");
            return;
        }

        TeaHydrationInputBlocker hydrationGate = FindObjectOfType<TeaHydrationInputBlocker>();
        bool hydrationTooLow = hydrationGate != null && hydrationGate.IsHydrationTooLow();

        // 1) If Luna already has a teacup, use it.
        if (_teacupInventory.HasTeacup())
        {
            if (Time.time - lastCupReceivedTime < postReceiveUseBlockTime)
            {
                Debug.Log("[TeaStateManager] Ignoring immediate follow-up tea input after receiving cup.");
                return;
            }

            if (IsNearNPC())
            {
                _teacupInventory.TryGiveTeacupToNPC();
                RemoveDrinkTargetHighlight();
            }
            else
            {
                _teacupInventory.DrinkTeacup();
                RemoveDrinkTargetHighlight();

                DialogueLua.SetVariable("LunaConsumeTeaDone", true);
                Debug.Log("[TeaStateManager] LunaConsumeTeaDone set to TRUE");
            }

            justDrankTea = true;
            lastDrinkTime = Time.time;
            return;
        }

        // 2) Hydration gate for spawning/brewing new tea
        if (hydrationTooLow)
        {
            if (!justDrankTea || Time.time - lastDrinkTime > 1.0f)
            {
                Debug.Log("🛑 Too dehydrated — cannot brew or spawn new teapot.");
                if (hydrationGate != null)
                    StartCoroutine(hydrationGate.PulseNearestLilystoolHint());
            }
            return;
        }

        // 3) If we are not currently tracking a teapot, try to find an existing nearby one first.
        if (_currentTeapot == null)
        {
            TryFindNearbyExistingTeapot();
        }

        // 4) Still no teapot tracked/found.
        if (_currentTeapot == null)
        {
            if (!allowTeaKeyToSpawnTeapot)
            {
                Debug.Log("[TeaStateManager] No teapot present. Tea key is set to brew-only.");
                return;
            }

            justDrankTea = false;

            LilyStool[] stools = FindObjectsOfType<LilyStool>();
            LilyStool nearest = null;
            float minDist = float.MaxValue;

            foreach (LilyStool stool in stools)
            {
                if (stool == null)
                    continue;

                if (!stool.PlayerOnLilypad)
                    continue;

                float dist = Vector2.Distance(transform.position, stool.transform.position);
                if (dist < lilyStoolSearchRadius && dist < minDist)
                {
                    nearest = stool;
                    minDist = dist;
                }
            }

            if (nearest == null)
            {
                Debug.Log("❌ No usable LilyStool nearby! Stand on the lilypad to place your teapot.");
                ShowLilystoolHint();
                return;
            }

            if (nearest.teapotSpawnPoint == null)
            {
                Debug.LogError($"❌ LilyStool '{nearest.name}' has no teapotSpawnPoint assigned.");
                return;
            }

            _currentLilystool = nearest;
            _currentLilystoolCandle = nearest.GetComponent<LilystoolCandleController>();

            _currentTeapot = Instantiate(
                teapotPrefab,
                nearest.teapotSpawnPoint.position,
                Quaternion.identity
            );

            if (_currentLilystoolCandle != null)
                _currentLilystoolCandle.NotifyTeapotSpawned();

            if (spawnAudioSource != null)
            {
                Debug.Log("PlaySpawnSFX called on manager!");
                spawnAudioSource.Play();
            }

            _currentReceiver = _currentTeapot.GetComponent<TeapotLightReceiver>();
            if (_currentReceiver == null)
                _currentReceiver = _currentTeapot.GetComponentInChildren<TeapotLightReceiver>(true);

            if (_currentReceiver == null)
                Debug.LogError("❌ Spawned teapot has no TeapotLightReceiver!");

            return;
        }

        // 5) Teapot exists but receiver is missing
        if (_currentReceiver == null)
        {
            Debug.LogError("[TeaStateManager] Current teapot exists but current receiver is null.");
            return;
        }

        // 6) Teapot exists but isn't brew-ready -> store or warn
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

            if (_currentTeapot != null)
                Destroy(_currentTeapot, destroyDelay);

            if (_currentLilystool != null)
                _currentLilystool.NotifyTeapotDestroyed();

            _currentTeapot = null;
            _currentReceiver = null;
            _currentLilystool = null;
            _currentLilystoolCandle = null;
            return;
        }

        // 7) Brew
        Debug.Log("🍵 Brewing tea (light detected)...");
        GameObject cup = _currentReceiver.BrewTea();

        if (cup != null)
        {
            Debug.Log("🍵 BrewTea() succeeded, receiving cup");
            _teacupInventory.ReceiveTeacup(cup);
            _teacupInventory.SetSourceCandleController(_currentLilystoolCandle);
            lastCupReceivedTime = Time.time;

            if (_currentLilystoolCandle != null)
                _currentLilystoolCandle.NotifyTeapotGoneAfterBrewing();

            if (_currentTeapot != null)
                Destroy(_currentTeapot);

            if (_currentLilystool != null)
                _currentLilystool.NotifyTeapotDestroyed();

            Debug.Log("🔥 Teapot destroyed by TeaStateManager");

            _currentTeapot = null;
            _currentReceiver = null;
            _currentLilystool = null;
            _currentLilystoolCandle = null;
        }
        else
        {
            Debug.LogWarning("❗ BrewTea() returned null. Check TeapotReceiver logs above for exact reason (usually no water or no light).");
        }
    }

    private void TryFindNearbyExistingTeapot()
    {
        GameObject[] teapots = GameObject.FindGameObjectsWithTag(teapotTag);

        GameObject nearestTeapot = null;
        float nearestDist = float.MaxValue;
        float maxDistSq = existingTeapotSearchRadius * existingTeapotSearchRadius;

        for (int i = 0; i < teapots.Length; i++)
        {
            GameObject teapot = teapots[i];
            if (teapot == null)
                continue;

            float distSq = ((Vector2)(teapot.transform.position - transform.position)).sqrMagnitude;
            if (distSq > maxDistSq)
                continue;

            TeapotLightReceiver receiver = teapot.GetComponent<TeapotLightReceiver>();
            if (receiver == null)
                receiver = teapot.GetComponentInChildren<TeapotLightReceiver>(true);

            if (receiver == null)
                continue;

            if (distSq < nearestDist)
            {
                nearestDist = distSq;
                nearestTeapot = teapot;
            }
        }

        if (nearestTeapot == null)
        {
            Debug.Log("[TeaStateManager] No nearby existing teapot found.");
            return;
        }

        _currentTeapot = nearestTeapot;
        _currentReceiver = _currentTeapot.GetComponent<TeapotLightReceiver>();
        if (_currentReceiver == null)
            _currentReceiver = _currentTeapot.GetComponentInChildren<TeapotLightReceiver>(true);

        _currentLilystool = FindNearestActiveLilystool();
        _currentLilystoolCandle = _currentLilystool != null ? _currentLilystool.GetComponent<LilystoolCandleController>() : null;

        Debug.Log($"[TeaStateManager] Found nearby teapot: {_currentTeapot.name}");
    }

    private LilyStool FindNearestActiveLilystool()
    {
        LilyStool[] stools = FindObjectsOfType<LilyStool>();
        LilyStool nearest = null;
        float minDist = float.MaxValue;

        for (int i = 0; i < stools.Length; i++)
        {
            LilyStool stool = stools[i];
            if (stool == null)
                continue;

            float dist = Vector2.Distance(transform.position, stool.transform.position);
            if (dist < lilyStoolSearchRadius && dist < minDist)
            {
                nearest = stool;
                minDist = dist;
            }
        }

        return nearest;
    }

    private void UpdateDrinkTargetHighlight()
    {
        TeacupReceiver npc = GetNearbyNPC();

        if (npc != null)
        {
            if (lunaHighlight != null)
                lunaHighlight.RemoveHighlight();

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
            if (_lastNpcHighlighted != null)
            {
                _lastNpcHighlighted.GetComponent<TeacupHighlight>()?.RemoveHighlight();
                _lastNpcHighlighted = null;
            }

            if (lunaHighlight != null)
                lunaHighlight.Highlight();
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

    private void ShowLilystoolHint()
    {
        if (lilystoolHintIcon != null)
            StartCoroutine(ShowHintForSeconds());
    }

    private IEnumerator ShowHintForSeconds()
    {
        ToggleHintRenderers(true);
        yield return new WaitForSeconds(hintShowTime);
        ToggleHintRenderers(false);
    }

    private bool IsNearNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.35f);
        foreach (Collider2D h in hits)
        {
            if (h.GetComponent<TeacupReceiver>() != null)
                return true;
        }
        return false;
    }

    private TeacupReceiver GetNearbyNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.35f);
        foreach (Collider2D h in hits)
        {
            TeacupReceiver receiver = h.GetComponent<TeacupReceiver>();
            if (receiver != null)
                return receiver;
        }
        return null;
    }

    private void ToggleHintRenderers(bool visible)
    {
        if (lilystoolHintIcon == null)
            return;

        foreach (SpriteRenderer sr in lilystoolHintIcon.GetComponentsInChildren<SpriteRenderer>(true))
            sr.enabled = visible;

        foreach (Image img in lilystoolHintIcon.GetComponentsInChildren<Image>(true))
            img.enabled = visible;

        foreach (CanvasGroup cg in lilystoolHintIcon.GetComponentsInChildren<CanvasGroup>(true))
            cg.alpha = visible ? 1f : 0f;
    }
}