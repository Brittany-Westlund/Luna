using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject teapotPrefab;
    public Transform  teapotSpawnPoint;
    public KeyCode    teaKey = KeyCode.T;

    private TeacupInventory     _teacupInventory;
    private GameObject          _currentTeapot;
    private TeapotLightReceiver _currentReceiver;

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
                _teacupInventory.TryGiveTeacupToNPC();
            else
                _teacupInventory.DrinkTeacup();
            return;
        }

        // 2) No teapot in scene? Spawn one.
        if (_currentTeapot == null)
        {
            _currentTeapot = Instantiate(
                teapotPrefab,
                teapotSpawnPoint.position,
                Quaternion.identity
            );

            var sfx = _currentTeapot.GetComponent<TeapotSpawnStoreSFX>();
            if (sfx != null)
                sfx.PlaySpawnSFX();

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

                var sfx = _currentTeapot.GetComponent<TeapotSpawnStoreSFX>();
                float destroyDelay = 0f;

                if (sfx != null && sfx.storeAudioSource != null && sfx.storeAudioSource.clip != null)
                {
                    sfx.PlayStoreSFX();
                    destroyDelay = sfx.storeAudioSource.clip.length;
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
            Destroy(_currentTeapot);
            _currentTeapot = null;
            _currentReceiver = null;
        }
        else
        {
            Debug.LogWarning("❗ BrewTea() returned null—check prefab or spawn‐point");
        }
    }

    bool IsNearNPC()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var h in hits)
            if (h.GetComponent<TeacupReceiver>() != null)
                return true;
        return false;
    }
}
