using UnityEngine;

public class ToggleControlsPanel : MonoBehaviour
{
    public GameObject controlsPanel; // Drag your controls panel GameObject here in the Inspector
    public Vector3 hiddenPosition; // Set the position to move the panel to when hiding
    public Vector3 shownPosition; // Default position to move the panel to when showing
    public Vector3 newShownPosition; // New position to apply during specific dialogue
    public string targetConversationName; // Name of the conversation to trigger the new position

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
    /// Call this method when a conversation starts.
    /// Checks if the conversation name matches and adjusts the position.
    /// </summary>
    /// <param name="conversationName">The name of the conversation that started.</param>
    public void OnConversationStart(string conversationName)
    {
        if (conversationName == targetConversationName)
        {
            // Update shown position to the new one
            shownPosition = newShownPosition;

            // If the panel is currently shown, move it to the new position
            if (isPanelShown)
            {
                controlsPanel.transform.localPosition = shownPosition;
            }
        }
    }
}
