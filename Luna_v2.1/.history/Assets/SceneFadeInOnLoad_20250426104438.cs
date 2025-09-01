using UnityEngine;
using MoreMountains.Tools;

public class SceneFadeInOnLoad : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    void Start()
    {
        // Fade from black (alpha 1) to transparent (alpha 0)
        MMFadeEvent.Trigger(fadeDuration, 0f, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic));
    }
}
