using UnityEngine;
using MoreMountains.Tools;

public class LanternObjectToggle : MonoBehaviour
{
    public SpriteRenderer LitLanternSprite;
    private KeyCode activationKey = KeyCode.E;
    public MMProgressBar lightBar;
    public GameObject targetObject;
    public bool deactivateOnLight = true;
    private float lightCost = 0.1f;
    private bool isLit = false;
    private bool playerInRange = false;

    [Header("Linked Object to Activate When Lit")]
    public GameObject linkedObject;  // 👈 new linked object reference

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;

            if (!isLit && currentProgress >= lightCost)
            {
                lightBar.SetBar01(currentProgress - lightCost);
                if (LitLanternSprite != null) LitLanternSprite.enabled = true;
                ToggleTargetObject(!deactivateOnLight);
                HandleLinkedObject(true); // 👈 enable linkedObject
                isLit = true;
            }
            else if (isLit)
            {
                lightBar.SetBar01(currentProgress + lightCost);
                if (LitLanternSprite != null) LitLanternSprite.enabled = false;
                ToggleTargetObject(deactivateOnLight);
                HandleLinkedObject(false); // 👈 disable linkedObject
                isLit = false;
            }
        }
    }

    private void ToggleTargetObject(bool state)
    {
        if (targetObject != null)
        {
            targetObject.SetActive(state);
        }
    }

    private void HandleLinkedObject(bool enable)
    {
        if (linkedObject != null)
        {
            linkedObject.SetActive(enable);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
