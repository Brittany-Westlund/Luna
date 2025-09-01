using System.Collections;
using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject teapotPrefab;
    public KeyCode    teaKey = KeyCode.T;
    public float lilyStoolSearchRadius = 1.5f; // How close you need to be to a lilystool to spawn a teapot
   
    private TeacupInventory     _teacupInventory;
    private GameObject          _currentTeapot;
    private TeapotLightReceiver _currentReceiver;
   
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
    }

    void HandleTeaLogic()
    { 
        Debug.Log("🍵 HandleTeaLogic: Attempting to brew (should see this when T is pressed and teapot is lit)");
        
        // 1) If you already have a teacup, use it
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
            return;
        }

        // 2) No teapot in scene? Spawn one.
        if (_currentTeapot == null)
        {
            // Find all LilyStools in the scene
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
            // Spawn teapot at the stool's spawn point
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
        
        // 3) Teapot exists but is unlit → either forbid storing (if flowers inside)
        //                                           or store it (if empty)
        if (!_currentReceiver.HasLight)
        {
            if (_currentReceiver.GetIngredientCount() > 0)
            {
                Debug.Log("❗ You added flowers but haven't lit it yet—hit Q to light or remove them first.");
                return;
            }
            else
            {
                Debug.Log("🫖 Teapot was stored (empty).");

                float destroyDelay = 0f;
                
                if (storeAudioSource != null && storeAudioSource.clip != null)
                {
                    Debug.Log("PlayStoreSFX called on manager!");
                    storeAudioSource.Play();
                    destroyDelay = storeAudioSource.clip.length;
                }
                else
                {
                    Debug.LogWarning("No Store SFX found or clip missing—destroying teapot immediately.");
                }

                Destroy(_currentTeapot, destroyDelay);
                _currentTeapot = null;
                _currentReceiver = null;
                return;
            }
        }

        // 4) Teapot is lit → brew
        Debug.Log("🍵 Brewing tea (light detected)...");
        var cup = _currentReceiver.BrewTea();
        if (cup != null)
        {
            Debug.Log("🍵 BrewTea() succeeded, receiving cup");
            _teacupInventory.ReceiveTeacup(cup);
            ShowDrinkTargetHighlight();
            Destroy(_currentTeapot);
            _currentTeapot = null;
            _currentReceiver = null;
        }
        else
        {
            Debug.LogWarning("❗ BrewTea() returned null—check prefab or spawn‐point");
        }
    }

    public void ShowDrinkTargetHighlight()
    {
        TeacupReceiver npc = GetNearbyNPC();
        if (npc != null)
        {
            var npcHighlight = npc.GetComponent<TeacupHighlight>();
            if (npcHighlight != null) npcHighlight.Highlight();
        }
        else if (lunaHighlight != null)
        {
            lunaHighlight.Highlight();
        }
    }

    public void RemoveDrinkTargetHighlight()
    {
        // Remove from both NPC and Luna just in case
        TeacupReceiver npc = GetNearbyNPC();
        if (npc != null)
        {
            var npcHighlight = npc.GetComponent<TeacupHighlight>();
            if (npcHighlight != null) npcHighlight.RemoveHighlight();
        }
        if (lunaHighlight != null)
        {
            lunaHighlight.RemoveHighlight();
        }
    }

    void ShowLilystoolHint()
    {
        if (lilystoolHintIcon != null)
            StartCoroutine(ShowHintForSeconds());
    }

    IEnumerator ShowHintForSeconds()
    {
        lilystoolHintIcon.SetActive(true);
        yield return new WaitForSeconds(hintShowTime);
        lilystoolHintIcon.SetActive(false);
    }

    bool IsNearNPC()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var h in hits)
            if (h.GetComponent<TeacupReceiver>() != null)
                return true;
        return false;
    }

    private TeacupReceiver GetNearbyNPC()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var h in hits)
        {
            var receiver = h.GetComponent<TeacupReceiver>();
            if (receiver != null)
                return receiver;
        }
        return null;
    }

}
