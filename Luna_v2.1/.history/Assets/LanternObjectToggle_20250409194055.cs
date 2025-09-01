using UnityEngine;
using MoreMountains.Tools;

public class LanternObjectToggle : MonoBehaviour
{
    [Header("Lantern Settings")]
    public SpriteRenderer LitLanternSprite;
    public MMProgressBar lightBar;
    public KeyCode activationKey = KeyCode.E;
    public float lightCost = 0.1f;

    [Header("Linked Object Settings")]
    public GameObject targetObject;         // Object to toggle on/off (e.g. nearby platform)
    public bool deactivateOnLight = true;   // If true, target turns off when lantern is lit
    public GameObject linkedObject;         // Additional object to activate when lantern is lit

    private bool isLit = false;
    private bool playerInRange = false;

    private void Update()
    {
        if (!playerInRange || lightBar == null) return;

        if (Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;

            if (!isLit && currentProgress >= lightCost)
            {
                lightBar.SetBar01(currentProgress - lightCost);
                SetLanternState(true);
            }
            else if (isLit)
            {
                lightBar.SetBar01(currentProgress + lightCost);
                SetLanternState(false);
            }
        }
    }

    private void SetLanternState(bool lit)
    {
        isLit = lit;

        if (LitLanternSprite != null)
            LitLanternSprite.enabled = lit;

        ToggleTargetObject(deactivateOnLight ? !lit : lit);
        HandleLinkedObject(lit);
    }

    private void ToggleTargetObject(bool state)
    {
        if (targetObject != null)
            targetObject.SetActive(state);
    }

    private void HandleLinkedObject(bool enable)
    {
        if (linkedObject == null) return;

        // Fully enable/disable the object
        linkedObject.SetActive(enable);

        // Just in case it’s a visual-only object
        SpriteRenderer sr = linkedObject.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.enabled = enable;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
            playerInRange = false;
    }
}
