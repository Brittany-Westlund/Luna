using UnityEngine;

public class FairyPollenStatus : MonoBehaviour
{
    private bool hasFairypetalPollen = false;

    public void GiveFairypetalPollen()
    {
        hasFairypetalPollen = true;
        Debug.Log("🧚‍♀️ Luna now has fairypetal pollen!");
    }

    public void ClearFairypetalPollen()
    {
        hasFairypetalPollen = false;
        Debug.Log("🌬️ Fairypetal pollen removed from Luna.");
    }

    public bool HasFairypetalPollen()
    {
        return hasFairypetalPollen;
    }
}
