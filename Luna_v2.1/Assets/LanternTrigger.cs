using UnityEngine;

public class LanternTrigger : MonoBehaviour
{
    [Header("Lantern Visual")]
    public SpriteRenderer LitLantern;  // Assign the lit version in the inspector

    [Header("Optional Settings")]
    public GameObject objectToActivate; // (Optional) Any object to reveal or trigger
    
    private bool isLit = false;

    public void Illuminate()
    {
        if (isLit) return;

        if (LitLantern != null)
        {
            LitLantern.enabled = true;
        }

        if (objectToActivate != null)
        {
            objectToActivate.SetActive(true);
        }

        isLit = true;
        Debug.Log("🕯️ Lantern lit by wand!");
    }
}
