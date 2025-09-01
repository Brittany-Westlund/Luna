using UnityEngine;

public class TeapotLightReceiver : MonoBehaviour
{
    [Header("Teapot Settings")]
    public GameObject sparkleEffect;         // Sparkle effect or lit visual
    public GameObject brewingIndicatorIcon;  // Icon that appears when ready to brew

    private bool isLit = false;
    private bool readyToBrew = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the object colliding has the wand script
        LunariaWandAttractor wand = other.GetComponent<LunariaWandAttractor>();

        if (wand != null && wand.HasLight() && !isLit)
        {
            // Accept light from the wand
            bool lightGiven = wand.GiveLightToObject();
            if (lightGiven)
            {
                ActivateBrewReadyState();
            }
        }
    }

    private void ActivateBrewReadyState()
    {
        isLit = true;
        readyToBrew = true;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(true);

        if (brewingIndicatorIcon != null)
            brewingIndicatorIcon.SetActive(true);

        Debug.Log("Teapot is ready to brew!");
    }

    public bool IsReadyToBrew()
    {
        return readyToBrew;
    }

    public void ResetTeapot()
    {
        isLit = false;
        readyToBrew = false;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        if (brewingIndicatorIcon != null)
            brewingIndicatorIcon.SetActive(false);
    }

    /// <summary>
    /// Called when the wand delivers a light mote.
    /// </summary>
    public void ReceiveLight()
    {
        // whatever you need to do to light the teapot:
        // e.g. enable a sprite, play a VFX, increment an "isLit" flag, etc.
        Debug.Log("TeapotLightReceiver – light received!");
    }
}
