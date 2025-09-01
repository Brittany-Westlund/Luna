using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    public Transform teacupHoldPoint;
    private GameObject currentTeacup;

    private GameObject happyIcon; // 👈 new field

    void Start()
    {
        // Auto-find HappyIcon if not assigned manually
        Transform iconTransform = transform.Find("HappyIcon");
        if (iconTransform != null)
        {
            happyIcon = iconTransform.gameObject;
            happyIcon.SetActive(false); // Hide by default
        }
    }

    void Update()
    {
        if (currentTeacup != null && Input.GetKeyDown(KeyCode.Q))
        {
            DrinkTeacup();
        }

        if (currentTeacup != null && Input.GetKeyDown(KeyCode.R))
        {
            TryGiveTeacupToNPC();
        }
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (currentTeacup != null)
        {
            Destroy(currentTeacup);
        }

        currentTeacup = teacup;

        if (teacupHoldPoint != null)
        {
            currentTeacup.transform.SetParent(teacupHoldPoint);
            currentTeacup.transform.localPosition = Vector3.zero;
        }
        else
        {
            currentTeacup.transform.SetParent(transform);
            currentTeacup.transform.localPosition = Vector3.zero;
        }

        Debug.Log("🫖 Luna picked up a teacup.");
    }

    private void DrinkTeacup()
    {
        if (currentTeacup == null)
        {
            Debug.Log("⚠️ No teacup to drink!");
            return;
        }

        TeaEffectManager effect = currentTeacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            Debug.Log("🍵 Luna drinks the tea...");
            effect.ApplyEffects(gameObject);
        }

        ShowHappyIcon(); // 👈 show the icon when she drinks

        Destroy(currentTeacup);
        currentTeacup = null;
    }

    private void ShowHappyIcon()
    {
        if (happyIcon != null)
        {
            happyIcon.SetActive(true);
            CancelInvoke(nameof(HideHappyIcon)); // just in case it's already scheduled
            Invoke(nameof(HideHappyIcon), 5f); // hide after 5 seconds
        }
    }

    private void HideHappyIcon()
    {
        if (happyIcon != null)
        {
            happyIcon.SetActive(false);
        }
    }

    private void TryGiveTeacupToNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            TeacupReceiver receiver = hit.GetComponent<TeacupReceiver>();
            if (receiver != null && receiver.CanReceiveTeacup())
            {
                receiver.ReceiveTeacup(currentTeacup);
                currentTeacup = null;
                Debug.Log("🫖 Gave teacup to " + receiver.name);
                return;
            }
        }

        Debug.Log("No valid NPC nearby to give the teacup to.");
    }
}
