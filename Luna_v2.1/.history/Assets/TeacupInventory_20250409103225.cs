using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    public Transform teacupHoldPoint;
    private GameObject currentTeacup;
    private GameObject happyIcon;

    void Start()
    {
        Transform iconTransform = transform.Find("HappyIcon");
        if (iconTransform != null)
        {
            happyIcon = iconTransform.gameObject;
            happyIcon.SetActive(false);
        }
    }

    public bool HasTeacup()
    {
        return currentTeacup != null;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (currentTeacup != null) Destroy(currentTeacup);

        currentTeacup = teacup;
        currentTeacup.transform.SetParent(teacupHoldPoint);
        currentTeacup.transform.localPosition = Vector3.zero;
    }

    public void DrinkTeacup()
    {
        if (currentTeacup == null) return;

        TeaEffectManager effect = currentTeacup.GetComponent<TeaEffectManager>();
        if (effect != null) effect.ApplyEffects(gameObject);

        ShowHappyIcon();
        Destroy(currentTeacup);
        currentTeacup = null;
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
                return;
            }
        }
    }

    private void ShowHappyIcon()
    {
        if (happyIcon != null)
        {
            happyIcon.SetActive(true);
            CancelInvoke(nameof(HideHappyIcon));
            Invoke(nameof(HideHappyIcon), 5f);
        }
    }

    private void HideHappyIcon()
    {
        if (happyIcon != null) happyIcon.SetActive(false);
    }
}