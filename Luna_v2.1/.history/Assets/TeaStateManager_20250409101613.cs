using UnityEngine;

public class TeaStateManager : MonoBehaviour
{
    public KeyCode teaKey = KeyCode.T;
    public GameObject teapotPrefab;
    public Transform teapotSpawnPoint;

    private GameObject currentTeapot;
    private GameObject spawnedTeacup;
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
        // ✅ Step 1: If Luna has a teacup, try to drink or give it
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

        // ✅ Step 2: Pick up a brewed teacup if one is ready
        if (spawnedTeacup != null)
        {
            teacupInventory.ReceiveTeacup(spawnedTeacup);
            spawnedTeacup = null;

            // Optionally disable the teapot after pickup
            if (currentTeapot != null)
            {
                currentTeapot.SetActive(false);
                currentTeapot = null;
                teapotReceiver = null;
            }

            Debug.Log("☕ Luna picked up the teacup.");
            return;
        }

        // ✅ Step 3: If no teapot exists, summon one
        if (currentTeapot == null)
        {
            currentTeapot = Instantiate(teapotPrefab, teapotSpawnPoint.position, Quaternion.identity);
            teapotReceiver = currentTeapot.GetComponent<TeapotReceiver>();
            Debug.Log("🫖 Teapot summoned.");
            return;
        }

        // ✅ Step 4: If teapot exists and has light, brew tea
        if (teapotReceiver != null && teapotReceiver.hasLight)
        {
            spawnedTeacup = teapotReceiver.BrewTea(); // <- return the GameObject here
            Debug.Log("✨ Tea brewed. Teacup ready to be picked up.");
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
