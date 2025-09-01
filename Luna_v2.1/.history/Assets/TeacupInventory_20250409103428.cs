using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    public Transform teacupHoldPoint;
    private GameObject currentTeacup;

    public GameObject happyIcon;

    void Start()
    {
        if (happyIcon != null)
            happyIcon.SetActive(false);
    }

    public bool HasTeacup()
    {
        return currentTeacup != null;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        currentTeacup = teacup;
        currentTeacup.transform.SetParent(teacupHoldPoint);
        currentTeacup.transform.localPosition = Vector3.zero;
    }

    public void DrinkTeacup()
    {
        if (currentTeacup == null) return;

        TeaEffectManager effect = currentTeacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.ApplyEffects(gameObject);
        }

        if (happyIcon != null)
        {
            happyIcon.SetActive(true);
            Invoke(nameof(HideHappyIcon), 5f);
        }

        Destroy(currentTeacup);
        currentTeacup = null;
    }

    public void TryGiveTeacupToNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (var hit in hits)
        {
            var receiver = hit.GetComponent<TeacupReceiver>();
            if (receiver != null)
            {
                receiver.ReceiveTeacup(currentTeacup);
                currentTeacup = null;
                break;
            }
        }
    }

    void HideHappyIcon()
    {
        if (happyIcon != null) happyIcon.SetActive(false);
    }
}
