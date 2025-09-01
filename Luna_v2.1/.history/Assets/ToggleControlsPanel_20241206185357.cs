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
        // Toggle panel visibility with input
        if (Input.GetButtonDown("HideShowControls"))
        {
            isPanelShown = !isPanelShown;
            controlsPanel.transform.localPosition = isPanelShown ? shownPosition : hiddenPosition;
        }

        // Check Lua variable and update the position dynamically
        CheckLuaForPanelUpdate();
    }

    private void CheckLuaForPanelUpdate()
    {
        // Get the value of the Lua variable "movePanel"
        bool movePanel = DialogueLua.GetVariable("movePanel").AsBool;

        // If movePanel is true, update the shown position
        if (movePanel && isPanelShown)
        {
            controlsPanel.transform.localPosition = newShownPosition;

            // Optionally reset the variable to prevent repeated execution
            DialogueLua.SetVariable("movePanel", false);
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
