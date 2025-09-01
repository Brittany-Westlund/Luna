using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MoreMountains.Tools;
using UnityEngine.UI;

public class LightDeactivation : MonoBehaviour
{
    public MMProgressBar LightBar;
    public SpriteRenderer LightDoorRenderer;
    public Transform PlayerTransform; // Reference to the player's transform
    public float ActivationRange = 3f; // The range within which the door can be deactivated
    public float lightCost = 0.1f; // Cost of light to deactivate the door
    public LightDoorActivated lightDoorActivated; // Reference to the LightDoorActivated script

    void Update()
    {
        // Check if the X key is pressed
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Check if the player is in range and has enough light
            if (IsPlayerInRange() && LightBar.BarProgress >= lightCost)
            {
                // Update the light bar progress
                LightBar.UpdateBar01(LightBar.BarProgress - lightCost);

                // Turn off the SpriteRenderer of the light door
                LightDoorRenderer.enabled = false; 

                // Call ToggleLightDoor method from LightDoorActivated script
                if (lightDoorActivated != null)
                {
                    lightDoorActivated.ToggleLightDoor();
                }
            }
        }
    }

    private bool IsPlayerInRange()
    {
        // Calculate the distance to the player and return true if in range
        float distanceToPlayer = Vector3.Distance(PlayerTransform.position, transform.position);
        return distanceToPlayer <= ActivationRange;
    }
}
