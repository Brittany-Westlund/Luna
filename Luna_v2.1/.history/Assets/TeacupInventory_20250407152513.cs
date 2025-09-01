using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    private GameObject currentTeacup;

    public void ReceiveTeacup(GameObject teacup)
    {
        if (currentTeacup != null)
        {
            Destroy(currentTeacup); // Remove any old teacup
        }

        currentTeacup = teacup;
        currentTeacup.transform.SetParent(transform);
        currentTeacup.transform.localPosition = Vector3.zero;

        Debug.Log("🫖 Luna picked up a teacup.");

        // ✅ Apply tea effects immediately
        ApplyTeacupEffects();
    }

    private void ApplyTeacupEffects()
    {
        if (currentTeacup == null)
        {
            Debug.LogWarning("⚠️ No teacup to apply effects from.");
            return;
        }

        TeaEffectManager effect = currentTeacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            Debug.Log("🧪 Applying effects from teacup...");
            effect.ApplyEffects(gameObject); // Luna = this.gameObject
        }
        else
        {
            Debug.LogWarning("⚠️ Teacup is missing TeaEffectManager!");
        }

        Destroy(currentTeacup); // Teacup consumed
        currentTeacup = null;
    }
}
