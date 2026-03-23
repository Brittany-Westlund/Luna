using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    [Header("Holding")]
    public Transform teacupHoldPoint;
    public GameObject currentTeacup;
    public bool resetLocalScaleOnReceive = false;
    public Vector3 receivedLocalScale = Vector3.one;

    [Header("Feedback")]
    public GameObject happyIcon;
    public AudioSource audioSource;
    public AudioClip drinkSFX;

    public LilystoolCandleController sourceCandleController;

    private void Start()
    {
        if (happyIcon != null)
            happyIcon.SetActive(false);
    }

    public bool HasTeacup()
    {
        return currentTeacup != null;
    }

    public void SetSourceCandleController(LilystoolCandleController sourceCandle)
    {
        sourceCandleController = sourceCandle;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (teacup == null)
        {
            Debug.LogWarning("[TeacupInventory] ReceiveTeacup called with null.");
            return;
        }

        if (teacupHoldPoint == null)
        {
            Debug.LogError("[TeacupInventory] No teacupHoldPoint assigned.");
            return;
        }

        currentTeacup = teacup;
        currentTeacup.SetActive(true);

        currentTeacup.transform.SetParent(teacupHoldPoint, false);
        currentTeacup.transform.localPosition = Vector3.zero;
        currentTeacup.transform.localRotation = Quaternion.identity;

        if (resetLocalScaleOnReceive)
            currentTeacup.transform.localScale = receivedLocalScale;

        Debug.Log($"[TeacupInventory] Received teacup '{currentTeacup.name}' at hold point '{teacupHoldPoint.name}'.");
    }

    public void DrinkTeacup()
    {
        if (currentTeacup == null)
            return;

        if (audioSource != null && drinkSFX != null)
            audioSource.PlayOneShot(drinkSFX);

        TeaEffectManager effect = currentTeacup.GetComponent<TeaEffectManager>();
        if (effect != null)
            effect.ApplyEffects(gameObject);

        if (happyIcon != null)
        {
            happyIcon.SetActive(true);
            Invoke(nameof(HideHappyIcon), 5f);
        }

        Destroy(currentTeacup);
        currentTeacup = null;

        if (sourceCandleController != null)
        {
            sourceCandleController.NotifyTeaFinished();
            sourceCandleController = null;
        }
    }

    public void TryGiveTeacupToNPC()
    {
        if (currentTeacup == null)
            return;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            TeacupReceiver receiver = hit.GetComponent<TeacupReceiver>();
            if (receiver == null)
                continue;

            receiver.ReceiveTeacup(currentTeacup);
            currentTeacup = null;

            if (sourceCandleController != null)
            {
                sourceCandleController.NotifyTeaFinished();
                sourceCandleController = null;
            }

            return;
        }
    }

    private void HideHappyIcon()
    {
        if (happyIcon != null)
            happyIcon.SetActive(false);
    }
}