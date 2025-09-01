using UnityEngine;

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

        // 3. Check if the current teapot is ready (has light)
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
