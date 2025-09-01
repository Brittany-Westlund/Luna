using UnityEngine;
using MoreMountains.Tools;

public class LanternObjectToggle : MonoBehaviour
{
    [Header("Lantern Visuals")]
    public SpriteRenderer LitLanternSprite;

    [Header("Light Bar")]
    public MMProgressBar lightBar;
    public float lightCost = 0.1f;

    [Header("Objects to Toggle")]
    public GameObject targetObject;
    public GameObject linkedObject;
    public SpriteRenderer additionalSpriteToToggle;

    [Header("Toggle Behavior")]
    public bool deactivateOnLight = true;

    private bool playerInRange = false;
    private bool isLit = false;
    private KeyCode activationKey = KeyCode.E;

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(activationKey))
        {
            Debug.Log("🔸 Player pressed toggle key!");

            float currentProgress = lightBar != null ? lightBar.BarProgress : 0f;
            Debug.Log("🔹 Current LightBar value: " + currentProgress);

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

    private void SetLanternState(bool turnOn)
    {
        isLit = turnOn;
        Debug.Log(turnOn ? "🌕 Lantern turned ON" : "🌑 Lantern turned OFF");

        if (LitLanternSprite != null)
            LitLanternSprite.enabled = turnOn;

        if (targetObject != null)
        {
            targetObject.SetActive(deactivateOnLight ? !turnOn : turnOn);
            Debug.Log("🎯 targetObject " + (targetObject.activeSelf ? "ENABLED" : "DISABLED"));
        }

        if (linkedObject != null)
        {
            linkedObject.SetActive(turnOn);
            Debug.Log("🔗 linkedObject " + (linkedObject.activeSelf ? "ENABLED" : "DISABLED"));
        }

        if (additionalSpriteToToggle != null)
        {
            additionalSpriteToToggle.enabled = turnOn;
            Debug.Log("🎨 additionalSpriteToToggle " + (turnOn ? "ENABLED" : "DISABLED"));
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("✅ Player entered range.");
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("❎ Player left range.");
        }
    }
}
