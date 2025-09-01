using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    [Header("Teapot Spawning")]
    public GameObject teapotPrefab;
    public Transform teapotSpawnPoint;
    public KeyCode    teaKey = KeyCode.T;

    private GameObject           _currentTeapot;
    private TeapotLightReceiver  _teapotReceiver;
    private TeacupInventory      _teacupInventory;

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
        // 1) If Luna already has a teacup, drink or give it
        if (_teacupInventory.HasTeacup())
        {
            if (IsNearNPC()) _teacupInventory.TryGiveTeacupToNPC();
            else             _teacupInventory.DrinkTeacup();
            return;
        }

        // 2) If no teapot in scene, spawn one
        if (_currentTeapot == null)
        {
            _currentTeapot = Instantiate(teapotPrefab,
                                         teapotSpawnPoint.position,
                                         Quaternion.identity);
            _teapotReceiver = _currentTeapot.GetComponent<TeapotLightReceiver>();
            if (_teapotReceiver == null)
                Debug.LogError("❗ Spawned teapot has no TeapotLightReceiver");
            return;
        }

        // 3) If teapot exists but has neither ingredients nor light, store it
        bool hasLight      = _teapotReceiver != null && _teapotReceiver.IsReadyToBrew();
        bool hasAnyIngr    = _teapotReceiver != null && _teapotReceiver.HasAnyIngredients();
        if (!hasLight && !hasAnyIngr)
        {
            Destroy(_currentTeapot);
            _currentTeapot     = null;
            _teapotReceiver    = null;
            Debug.Log("🫖 Teapot stored: no ingredients or light.");
            return;
        }

        // 4) If teapot has light (i.e. ready), brew tea
        if (hasLight)
        {
            GameObject cup = _teapotReceiver.BrewTea();
            if (cup != null)
            {
                _teacupInventory.ReceiveTeacup(cup);
                Destroy(_currentTeapot);
                _currentTeapot  = null;
                _teapotReceiver = null;
            }
            else
            {
                Debug.LogWarning("❗ BrewTea() returned null.");
            }
        }
        else
        {
            Debug.Log("💡 Teapot not ready—add light first.");
        }
    }

    bool IsNearNPC()
    {
        const float radius = 1.5f;
        foreach (var hit in Physics2D.OverlapCircleAll(transform.position, radius))
            if (hit.GetComponent<TeacupReceiver>() != null)
                return true;
        return false;
    }
}


/* using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    public GameObject teapotPrefab;
    public Transform teapotSpawnPoint;
    public KeyCode teaKey = KeyCode.T;

    private GameObject currentTeapot;
    private TeapotReceiver currentTeapotReceiver;
    private TeacupInventory teacupInventory;

    void Start()
    {
        teacupInventory = GetComponent<TeacupInventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teaKey))
        {
            HandleTeaLogic();
        }
    }

    void HandleTeaLogic()
    {
        // 1. If Luna has tea, try to drink or give it
        if (teacupInventory.HasTeacup())
        {
            if (IsNearNPC())
            {
                teacupInventory.TryGiveTeacupToNPC();
            }
            else
            {
                teacupInventory.DrinkTeacup();
            }
            return;
        }

        // 2. If no teapot exists, instantiate one
        if (currentTeapot == null)
        {
            currentTeapot = Instantiate(teapotPrefab, teapotSpawnPoint.position, Quaternion.identity);
            currentTeapotReceiver = currentTeapot.GetComponent<TeapotReceiver>();

            if (currentTeapotReceiver == null)
            {
                Debug.LogError("❗ Spawned teapot has no TeapotReceiver component.");
            }

            return;
        }

        // 3. If teapot exists but has no light or ingredients, store it
        if (currentTeapotReceiver != null && !currentTeapotReceiver.hasLight && !currentTeapotReceiver.HasAnyIngredients())
        {
            Destroy(currentTeapot);
            currentTeapot = null;
            currentTeapotReceiver = null;
            Debug.Log("🫖 Teapot was stored because it had no ingredients or light.");
            return;
        }

        // 4. If the current teapot is ready (has light), brew tea
        if (currentTeapotReceiver != null && currentTeapotReceiver.hasLight)
        {
            GameObject teacup = currentTeapotReceiver.BrewTea();

            if (teacup != null)
            {
                teacupInventory.ReceiveTeacup(teacup);
                Destroy(currentTeapot);
                currentTeapot = null;
                currentTeapotReceiver = null;
            }
            else
            {
                Debug.Log("❗ BrewTea() was called but no teacup was returned.");
            }
        }
        else
        {
            Debug.Log("💡 Teapot is not ready yet — make sure it's charged with light.");
        }
    }

    bool IsNearNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            if (hit.GetComponent<TeacupReceiver>() != null)
            {
                return true;
            }
        }
        return false;
    }
}
*/