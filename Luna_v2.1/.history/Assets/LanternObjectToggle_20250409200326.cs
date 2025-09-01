using UnityEngine;
using MoreMountains.Tools;

public class LanternObjectToggle : MonoBehaviour
{
    [Header("Lantern Visual")]
    public SpriteRenderer LitLanternSprite;

    [Header("Light System")]
    public MMProgressBar lightBar;
    public float lightCost = 0.1f;
    private bool isLit = false;

    [Header("Activation Settings")]
    public GameObject targetObject;
    public GameObject linkedObject;
    public SpriteRenderer additionalSpriteToToggle; // 🔄 NEW!

    public bool deactivateOnLight = true;

    private KeyCode activationKey = KeyCode.E;
    private bool playerInRange = false;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            float currentProgress = lightBar.BarProgress;

            if (!isLit && currentProgress >= lightCost)
            {
                lightBar.SetBar01(currentProgress - lightCost);
                SetLitState(true);
            }
            else if (isLit)
            {
                lightBar.SetBar01(currentProgress + lightCost);
                SetLitState(false);
            }
        }
    }

    private void SetLitState(bool state)
    {
        isLit = state;

        if (LitLanternSprite != null)
            LitLanternSprite.enabled = state;

        if (targetObject != null)
            targetObject.SetActive(deactivateOnLight ? !state : state);

        if (linkedObject != null)
            linkedObject.SetActive(state);

        if (additionalSpriteToToggle != null)
            additionalSpriteToToggle.enabled = state;
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
