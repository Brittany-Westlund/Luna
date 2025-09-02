using UnityEngine;
using PixelCrushers;

public class SaveGameProxy : MonoBehaviour
{
    // Public method to be called by your UI Button.
    // You can optionally allow setting the slot from the inspector.
    public int saveSlot = 1;

    public void SaveGame()
    {
        SaveSystem.SaveToSlot(saveSlot);
        Debug.Log($"Game saved to slot {saveSlot}!");
    }
}
