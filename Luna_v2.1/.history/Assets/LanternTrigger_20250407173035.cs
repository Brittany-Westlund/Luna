using UnityEngine;

public class LanternTrigger : MonoBehaviour
{
    public SpriteRenderer litLanternSprite;  // The lit version
    public GameObject optionalObjectToActivate; // Optional object (like a platform or hint)
    public bool isLit = false;

    public void Illuminate()
    {
        if (isLit) return;

        if (litLanternSprite != null)
        {
            litLanternSprite.enabled = true;
        }

        if (optionalObjectToActivate != null)
        {
            optionalObjectToActivate.SetActive(true);
        }

        isLit = true;
        Debug.Log("🕯️ Lantern has been lit!");
    }
}
