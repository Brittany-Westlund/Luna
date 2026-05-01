using UnityEngine;

public class WandDialogueRelay : MonoBehaviour
{
    public void EnableLunaWand()
    {
        WandForever wand = FindObjectOfType<WandForever>(true);

        if (wand == null)
        {
            Debug.LogWarning("WandDialogueRelay: WandForever not found.");
            return;
        }

        if (wand.wandChild != null)
        {
            wand.wandChild.SetActive(true);
            Debug.Log("WandDialogueRelay: Enabled Luna wand.");
        }
        else
        {
            Debug.LogWarning("WandDialogueRelay: wandChild is null.");
        }
    }

    public void DisableLunaWand()
    {
        WandForever wand = FindObjectOfType<WandForever>(true);

        if (wand == null)
        {
            Debug.LogWarning("WandDialogueRelay: WandForever not found.");
            return;
        }

        if (wand.wandChild != null)
        {
            wand.wandChild.SetActive(false);
            Debug.Log("WandDialogueRelay: Disabled Luna wand.");
        }
        else
        {
            Debug.LogWarning("WandDialogueRelay: wandChild is null.");
        }
    }
}