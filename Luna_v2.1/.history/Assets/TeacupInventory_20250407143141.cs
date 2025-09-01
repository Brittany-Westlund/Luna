using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    private GameObject heldTeacup;
    public Transform holdPoint; // Optional: auto-find if null
    public KeyCode drinkKey = KeyCode.R;

    void Start()
    {
        if (holdPoint == null)
        {
            holdPoint = transform.Find("TeacupHoldPoint"); // Just name the empty point in prefab
        }
    }

    void Update()
    {
        if (heldTeacup != null && Input.GetKeyDown(drinkKey))
        {
            DrinkTeacup();
        }
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (heldTeacup != null)
        {
            Destroy(heldTeacup);
        }

        heldTeacup = teacup;
        teacup.transform.SetParent(holdPoint);
        teacup.transform.localPosition = Vector3.zero;
        teacup.transform.localRotation = Quaternion.identity;

        var collider = teacup.GetComponent<Collider2D>();
        if (collider != null) collider.enabled = false;

        var rb = teacup.GetComponent<Rigidbody2D>();
        if (rb != null) rb.simulated = false;
    }

    private void DrinkTeacup()
    {
        TeaEffectManager effect = heldTeacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.ApplyEffects(gameObject); // Pass Luna as the target
        }

        Destroy(heldTeacup);
        heldTeacup = null;
    }
}
