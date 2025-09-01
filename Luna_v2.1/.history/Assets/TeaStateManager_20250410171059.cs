using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    public GameObject teapotPrefab;
    public Transform teapotSpawnPoint;
    public KeyCode teaKey = KeyCode.T;

    private GameObject currentTeapot;
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

        // Spawn a new teapot if one doesn't exist
        if (currentTeapot == null)
        {
            currentTeapot = Instantiate(teapotPrefab, teapotSpawnPoint.position, Quaternion.identity);
            return;
        }

        // Always re-fetch the receiver from the current teapot in case it was replaced
        TeapotReceiver teapotReceiver = currentTeapot.GetComponent<TeapotReceiver>();

        if (teapotReceiver != null && teapotReceiver.hasLight)
        {
            GameObject teacup = teapotReceiver.BrewTea();
            if (teacup != null)
            {
                teacupInventory.ReceiveTeacup(teacup);
                Destroy(currentTeapot);
                currentTeapot = null;
            }
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
