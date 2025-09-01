using UnityEngine;
using PixelCrushers.DialogueSystem; // Required for Dialogue System integration

public class ToggleControlsPanel : MonoBehaviour
{
    public GameObject controlsPanel; // Drag your controls panel GameObject here
    public Vector3 hiddenPosition; // Position when hiding
    public Vector3 shownPosition; // Default position when showing
    public Vector3 newShownPosition; // Special position when Lua variable triggers

    private bool isPanelShown = true;

    void Update()
    {
        // Check Lua variable and update the position dynamically before toggling
        CheckLuaForPanelUpdate();

        // Toggle panel visibility with input
        if (Input.GetButtonDown("HideShowControls"))
        {
            isPanelShown = !isPanelShown;

            // Ensure the panel toggles between the updated shown position and hidden position
            controlsPanel.transform.localPosition = isPanelShown ? shownPosition : hiddenPosition;
        }
    }

    private void CheckLuaForPanelUpdate()
    {
        // Get the value of the Lua variable "movePanel"
        bool movePanel = DialogueLua.GetVariable("movePanel").AsBool;

        // If movePanel is true, update the shown position and ensure the panel is visible
        if (movePanel)
        {
            // Move the panel to the new shown position
            controlsPanel.transform.localPosition = newShownPosition;

            // Ensure the panel is shown (active)
            if (!isPanelShown)
            {
                isPanelShown = true; // Update the state to shown
            }

            // Optionally reset the variable to prevent repeated execution
            DialogueLua.SetVariable("movePanel", false);

            Debug.Log("Controls panel moved to the new shown position and displayed.");
        }
    }

    /// <summary>
    /// Updates the shown position and moves the panel if it's currently shown.
    /// </summary>
    /// <param name="newPosition">The new position to set for the panel.</param>
    public void UpdateShownPosition(Vector3 newPosition)
    {
        shownPosition = newPosition;

        // If the panel is currently shown, move it to the new position
        if (isPanelShown)
        {
            controlsPanel.transform.localPosition = shownPosition;
        }
    }
}
