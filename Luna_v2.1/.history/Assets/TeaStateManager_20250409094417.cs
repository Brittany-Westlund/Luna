using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    public KeyCode teaKey = KeyCode.T;
    public GameObject teapotPrefab;
    public Transform teapotSpawnPoint;

    private GameObject currentTeapot;
    private TeacupInventory teacupInventory;
    private TeapotReceiver teapotReceiver;

    void Start()
    {
        teacupInventory = GetComponent<TeacupInventory>();
    }

    void Update()
    {
        if (Input.GetKeyDown(teaKey))
        {
            HandleTeaKeyPress();
        }
    }

    void HandleTeaKeyPress()
    {
        // ✅ Step 1: Already holding a teacup
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

        // ✅ Step 2: No teacup, teapot doesn't exist — summon one
        if (currentTeapot == null)
        {
            currentTeapot = Instantiate(teapotPrefab, teapotSpawnPoint.position, Quaternion.identity);
            teapotReceiver = currentTeapot.GetComponent<TeapotReceiver>();
            Debug.Log("🫖 Teapot summoned.");
            return;
        }

        // ✅ Step 3: Teapot exists — check if brewed and has spawned a teacup
        if (teapotReceiver != null && teapotReceiver.spawnedTeacup != null)
        {
            teacupInventory.ReceiveTeacup(teapotReceiver.spawnedTeacup);
            teapotReceiver.spawnedTeacup = null;
            currentTeapot = null;
            teapotReceiver = null;
            Debug.Log("☕ Picked up teacup from teapot.");
            return;
        }

        // ✅ Step 4: Teapot exists but no tea yet — check for light
        if (teapotReceiver != null && teapotReceiver.hasLight)
        {
            teapotReceiver.BrewTea(); // Brew happens here, creates teacup
            Debug.Log("✨ Tea brewed.");
            return;
        }

        // 🚫 Not ready to brew
        Debug.Log("⚠️ Teapot needs light before brewing!");
    }

    bool IsNearNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            if (hit.GetComponent<TeacupReceiver>() != null)
            {
                return true;
            }
        }
        return false;
    }
}
