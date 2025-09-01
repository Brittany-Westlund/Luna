using UnityEngine;
using MoreMountains.Tools;

public class SceneFadeInOnLoad : MonoBehaviour
{
    [Header("Fade Settings")]
    public float fadeDuration = 0.5f;

    void Awake()
    {
        var faders = FindObjectsOfType<MMFader>();
        if (faders.Length > 1)
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject); // only if I want one persistent fader
    }
    void Start()
    {
        var fader = FindObjectOfType<MMFader>();
        if (fader != null)
        {
            // Fade from black (alpha 1) to transparent (alpha 0)
            MMFadeEvent.Trigger(fadeDuration, 0f, new MMTweenType(MMTween.MMTweenCurve.EaseInCubic));
        }
    }
}
