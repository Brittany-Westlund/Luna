using UnityEngine;

public class SwitchOnMax : MonoBehaviour
{
    public GameObject sprout; // Reference to the Sprout GameObject
    public GameObject externalObject; // The GameObject to move to Sprout's position
    public float maxScale = 0.22f; // Maximum scale of the Sprout

    void Update()
    {
        if (sprout.transform.localScale.x >= maxScale)
        {
            // Disable the Sprout GameObject
            sprout.SetActive(false);

            // Move the external object to the Sprout's position
            externalObject.transform.position = sprout.transform.position;

            // Optionally, if you want to stop checking every frame after this happens
            this.enabled = false;
        }
    }
}
