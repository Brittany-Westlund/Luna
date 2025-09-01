using UnityEngine;

public class TeacupInventory : MonoBehaviour
{
    private GameObject heldTeacup;
    public Transform holdPoint; // Where the teacup appears near Luna
    public KeyCode drinkKey = KeyCode.R;

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
            Destroy(heldTeacup); // Replace any existing one
        }

        heldTeacup = teacup;
        heldTeacup.transform.SetParent(holdPoint);
        heldTeacup.transform.localPosition = Vector3.zero;
        heldTeacup.transform.localRotation = Quaternion.identity;
        heldTeacup.GetComponent<Collider2D>().enabled = false;
        Rigidbody2D rb = heldTeacup.GetComponent<Rigidbody2D>();
        if (rb) rb.simulated = false;
    }

    private void DrinkTeacup()
    {
        if (heldTeacup.TryGetComponent<TeaEffectManager>(out var tea))
        {
            tea.ApplyEffects(gameObject); // Player is the target
        }

        Destroy(heldTeacup);
        heldTeacup = null;
    }
}
