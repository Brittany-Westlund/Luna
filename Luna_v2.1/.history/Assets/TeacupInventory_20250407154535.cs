using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    public Transform teacupHoldPoint; // assign this in the inspector!

    private GameObject currentTeacup;

    void Update()
    {
        if (currentTeacup != null)
        {
            if (Input.GetKeyDown(KeyCode.Q))
            {
                DrinkTeacup();
            }

            if (Input.GetKeyDown(KeyCode.W))
            {
                TryGiveTeacupToNPC();
            }
        }
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (currentTeacup != null)
        {
            Destroy(currentTeacup); // Drop/replace the old one
        }

        currentTeacup = teacup;

        if (teacupHoldPoint != null)
        {
            currentTeacup.transform.SetParent(teacupHoldPoint);
            currentTeacup.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("No teacupHoldPoint assigned! Defaulting to player position.");
            currentTeacup.transform.SetParent(transform);
            currentTeacup.transform.localPosition = Vector3.zero;
        }

        Debug.Log("🫖 Luna picked up a teacup.");
    }

    private void DrinkTeacup()
    {
        if (currentTeacup == null)
        {
            Debug.Log("⚠️ No teacup to drink!");
            return;
        }

        TeaEffectManager effect = currentTeacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            Debug.Log("🍵 Luna drinks the tea...");
            effect.ApplyEffects(gameObject);
        }
        else
        {
            Debug.LogWarning("⚠️ Teacup is missing TeaEffectManager!");
        }

        Destroy(currentTeacup);
        currentTeacup = null;
    }

    private void TryGiveTeacupToNPC()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1.5f);
        foreach (Collider2D hit in hits)
        {
            TeacupReceiver receiver = hit.GetComponent<TeacupReceiver>();
            if (receiver != null && receiver.CanReceiveTeacup())
            {
                receiver.ReceiveTeacup(currentTeacup);
                currentTeacup = null;
                Debug.Log("🫖 Gave teacup to " + receiver.name);
                return;
            }
        }

        Debug.Log("No valid NPC nearby to give the teacup to.");
    }
}
