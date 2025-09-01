using UnityEngine;
using MoreMountains.Tools;

public class FaderTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            MMFadeEvent.Trigger(0.5f, 1f, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic)); // Fade out (black)
        if (Input.GetKeyDown(KeyCode.I))
            MMFadeEvent.Trigger(0.5f, 0f, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic)); // Fade in (visible)
    }
}
