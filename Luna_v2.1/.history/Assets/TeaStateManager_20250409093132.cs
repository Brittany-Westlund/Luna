using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    public KeyCode teaKey = KeyCode.T;
    public GameObject teapotPrefab;
    public Transform teapotSpawnPoint;

    private GameObject currentTeapot;
    private TeacupInventory teacupInventory;
    private bool teaBrewed = false;

    void Start()
    {
        teacupInventory = GetComponent<TeacupInventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teaKey))
        {
            HandleTeaState();
        }
    }

    void HandleTeaState()
    {
        // 1. Already holding a teacup
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

        // 2. No teacup, teapot not in scene — summon teapot
        if (currentTeapot == null)
        {
            currentTeapot = Instantiate(teapotPrefab, teapotSpawnPoint.position, Quaternion.identity);
            teaBrewed = false;
            Debug.Log("🫖 Teapot summoned.");
            return;
        }

        // 3. Teapot exists, not brewed yet — brew tea
        if (!teaBrewed)
        {
            BrewTea();
            return;
        }

        // 4. Tea is brewed — pick up teacup
        PickupTeacup();
    }

    void BrewTea()
    {
        teaBrewed = true;
        Debug.Log("🍵 Tea brewed!");
        // Add brewing visuals here later
    }

    void PickupTeacup()
    {
        GameObject teacup = currentTeapot.GetComponent<Teapot>()?.CreateTeacup();
        if (teacup != null)
        {
            teacupInventory.ReceiveTeacup(teacup);
            Debug.Log("🫖 Teacup picked up.");
        }

        Destroy(currentTeapot); // Remove teapot once the cup is collected
        currentTeapot = null;
        teaBrewed = false;
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
