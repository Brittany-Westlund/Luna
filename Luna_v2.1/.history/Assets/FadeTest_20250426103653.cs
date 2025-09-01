using MoreMountains.Tools;
using UnityEngine;

public class FaderTest : MonoBehaviour
{
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
            MMFader.Instance.FadeOut();

        if (Input.GetKeyDown(KeyCode.I))
            MMFader.Instance.FadeIn();
    }
}
