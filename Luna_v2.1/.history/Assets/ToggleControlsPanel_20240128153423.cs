using UnityEngine;

public class ToggleControlsPanel : MonoBehaviour
{
    public GameObject controlsPanel; // Drag your controls panel GameObject here in the Inspector
    public Vector3 hiddenPosition; // Set the position to move the panel to when hiding
    public Vector3 shownPosition; // Set the position to move the panel to when showing

    private bool isPanelShown = true;

    void Update()
    {
        if (Input.GetButtonDown("HideShowControls"))
        {
            isPanelShown = !isPanelShown;
            controlsPanel.transform.localPosition = isPanelShown ? shownPosition : hiddenPosition;
        }
    }
}