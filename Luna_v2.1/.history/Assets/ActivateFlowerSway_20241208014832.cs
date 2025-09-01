using UnityEngine;

public class ActivateFlowerSwayAndLight : MonoBehaviour
{
    public AidingSprouts wateringScript; // Reference to the Watering script
    public GameObject sproutGameObject; // Reference to the GameObject with the FlowerSway and LightFadeActivation script components
    public float maxScale = 0.2215f; // Maximum scale of the sprout

    private FlowerSway flowerSwayScript; // Reference to the FlowerSway script component
    private LightFadeActivation lightFadeActivationScript; // Reference to the LightFadeActivation script component

    void Start()
    {
        // Get the FlowerSway script component from the sprout GameObject
        flowerSwayScript = sproutGameObject.GetComponent<FlowerSway>();

        // Get the LightFadeActivation script component from the sprout GameObject
        lightFadeActivationScript = sproutGameObject.GetComponent<LightFadeActivation>();

        if (flowerSwayScript == null)
        {
            Debug.LogError("FlowerSway script component not found on the specified GameObject.");
        }

        if (lightFadeActivationScript == null)
        {
            Debug.LogError("LightFadeActivation script component not found on the specified GameObject.");
        }
    }

    void Update()
    {
        // Check if the sprout has reached its maximum scale
        if (transform.localScale.x >= maxScale)
        {
            // If it has, activate the FlowerSway script component
            if (flowerSwayScript != null)
            {
                flowerSwayScript.enabled = true;
            }

            // If it has, activate the LightFadeActivation script component
            if (lightFadeActivationScript != null)
            {
                lightFadeActivationScript.enabled = true;
            }
        }
    }
}
