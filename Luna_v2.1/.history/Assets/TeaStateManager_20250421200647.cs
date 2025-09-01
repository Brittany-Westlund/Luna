// TeaStateManager.cs
using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject teapotPrefab;
    public Transform  teapotSpawnPoint;
    public KeyCode    teaKey = KeyCode.T;

    TeacupInventory        _teacupInventory;
    GameObject             _currentTeapot;
    TeapotLightReceiver    _currentTeapotReceiver;

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
        // 1) If you already have a teacup, drink or give it away…
        if (_teacupInventory.HasTeacup())
        {
            if (IsNearNPC())
                _teacupInventory.TryGiveTeacupToNPC();
            else
                _teacupInventory.DrinkTeacup();
            return;
        }

        // 2) No teapot in scene?  Spawn one.
        if (_currentTeapot == null)
        {
            _currentTeapot = Instantiate(
                teapotPrefab,
                teapotSpawnPoint.position,
                Quaternion.identity
            );
            _currentTeapotReceiver = _currentTeapot.GetComponent<TeapotLightReceiver>();
            if (_currentTeapotReceiver == null)
                Debug.LogError("Spawned teapot has no TeapotLightReceiver!");
            return;
        }

        // 3a) If it has flowers but no light, forbid storing
        if (!_currentTeapotReceiver.HasLight
            && _currentTeapotReceiver.HasAnyIngredients())
        {
            Debug.Log("❗ You added flowers but haven't lit it yet—hit Q to light or remove them first.");
            return;
        }

        // 3b) If it’s empty and unlit, store it away
        if (!_currentTeapotReceiver.HasLight
            && !_currentTeapotReceiver.HasAnyIngredients())
        {
            Destroy(_currentTeapot);
            _currentTeapot = null;
            _currentTeapotReceiver = null;
            Debug.Log("🫖 Teapot was stored (no ingredients or light).");
            return;
        }

        // 4) If it has light, brew tea
        if (_currentTeapotReceiver.HasLight)
        {
            GameObject cup = _currentTeapotReceiver.BrewTea();
            if (cup != null)
            {
                _teacupInventory.ReceiveTeacup(cup);
                Destroy(_currentTeapot);
                _currentTeapot = null;
                _currentTeapotReceiver = null;
            }
            else
            {
                Debug.LogWarning("❗ BrewTea() returned null—no cup created.");
            }
            return;
        }

        // fallback
        Debug.Log("💡 Teapot not ready—make sure it's charged with light.");
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
