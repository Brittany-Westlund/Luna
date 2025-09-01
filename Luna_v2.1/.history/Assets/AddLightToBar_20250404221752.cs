using UnityEngine;
using MoreMountains.Tools;

public class AddLightToBar : MonoBehaviour
{
    [Range(0f, 1f)]
    public float lightAmount = 0.1f;
    private MMProgressBar lightBar;

    private void Start()
    {
        // Look specifically for the object named "LightBar"
        GameObject lightBarObject = GameObject.Find("LightBar");

        if (lightBarObject != null)
        {
            lightBar = lightBarObject.GetComponent<MMProgressBar>();
            Debug.Log("AddLightToBar: Found LightBar named " + lightBar.name);
        }
        else
        {
            Debug.LogWarning("AddLightToBar: No object named 'LightBar' found in scene.");
        }
    }

    public void AddLight()
    {
        if (lightBar != null)
        {
            float newValue = Mathf.Clamp01(lightBar.BarProgress + lightAmount);
            lightBar.UpdateBar(newValue, 0f, 1f); // For LocalScale mode
            Debug.Log("AddLightToBar: LightBar increased to " + newValue);
        }
    }
}
