using UnityEngine;

public class ToggleControlsPanel : MonoBehaviour
{
    public GameObject controlsPanel; // Drag your controls panel GameObject here
    public Vector3 hiddenPosition;
    public Vector3 shownPosition; // Default shown position
    public Vector3 newShownPosition; // Special position during specific dialogues

    private bool isPanelShown = true;

    void Update()
    {
        if (Input.GetButtonDown("HideShowControls"))
        {
            isPanelShown = !isPanelShown;
            controlsPanel.transform.localPosition = isPanelShown ? shownPosition : hiddenPosition;
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
