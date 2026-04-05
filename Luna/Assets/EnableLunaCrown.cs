using UnityEngine;

public class CrownDialogueRelay : MonoBehaviour
{
    public void EnableLunaCrown()
    {
        MoonflowerCrownController crown = FindObjectOfType<MoonflowerCrownController>(true);

        if (crown == null)
        {
            Debug.LogWarning("CrownDialogueRelay: MoonflowerCrownController not found.");
            return;
        }

        crown.EnableCrown();
        Debug.Log("CrownDialogueRelay: Enabled Luna crown.");
    }

    public void DisableLunaCrown()
    {
        MoonflowerCrownController crown = FindObjectOfType<MoonflowerCrownController>(true);

        if (crown == null)
        {
            Debug.LogWarning("CrownDialogueRelay: MoonflowerCrownController not found.");
            return;
        }

        crown.SetCrownEquipped(false);
        Debug.Log("CrownDialogueRelay: Disabled Luna crown.");
    }
}