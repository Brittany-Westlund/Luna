using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    public Transform teacupHoldPoint;
    private GameObject currentTeacup;

    private GameObject happyIcon;

    void Start()
    {
        // Try to find the HappyIcon child object
        Transform iconTransform = transform.Find("HappyIcon");
        if (iconTransform != null)
        {
            happyIcon = iconTransform.gameObject;
            happyIcon.SetActive(false); // Start hidden
        }
    }

    void Update()
    {
        // Only responsible for giving tea; "T" is handled in TeaStateManager
        if (currentTeacup != null && Input.GetKeyDown(KeyCode.G))
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

    public void DrinkTeacup()
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
        else
        {
            Debug.LogWarning("No TeaEffectManager found on teacup!");
        }

        ShowHappyIcon();

        Destroy(currentTeacup);
        currentTeacup = null;
    }

    private void ShowHappyIcon()
    {
        if (happyIcon != null)
        {
            happyIcon.SetActive(true);
            CancelInvoke(nameof(HideHappyIcon));
            Invoke(nameof(HideHappyIcon), 5f); // Auto-hide
        }
    }

    private void HideHappyIcon()
    {
        if (happyIcon != null)
        {
            happyIcon.SetActive(false);
        }
    }

    public void TryGiveTeacupToNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            TeacupReceiver receiver = hit.GetComponent<TeacupReceiver>();
            if (receiver != null && receiver.CanReceiveTeacup())
            {
                receiver.ReceiveTeacup(currentTeacup);
                currentTeacup = null;
                Debug.Log("🎁 Gave teacup to " + receiver.name);
                return;
            }
        }

        Debug.Log("🚫 No NPC nearby to give the teacup to.");
    }

    public bool HasTeacup()
    {
        return currentTeacup != null;
    }
}
