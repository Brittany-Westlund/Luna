using UnityEngine;
using System.Linq;

public class LanternLightGiver : MonoBehaviour
{
    public KeyCode giveKey = KeyCode.E;
    public float lightRadius = 1.2f;

    public LanternSmartToggle lanternToggle;

    void Awake()
    {
        if (lanternToggle == null)
            lanternToggle = GetComponent<LanternSmartToggle>();
    }

    void Update()
    {
        if (lanternToggle == null) return;
        if (!lanternToggle.IsLit) return;

        if (Input.GetKeyDown(giveKey))
        {
            TryGiveLight();
        }
    }

    void TryGiveLight()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, lightRadius);

        var teapot = hits
            .Select(h =>
                h.GetComponent<TeapotLightReceiver>()
                ?? h.GetComponentInChildren<TeapotLightReceiver>()
                ?? h.GetComponentInParent<TeapotLightReceiver>())
            .FirstOrDefault(t => t != null && !t.HasLight);

        if (teapot == null)
        {
            Debug.Log(" No unlit teapot in range.");
            return;
        }

        Debug.Log("Lantern transferring light to teapot.");

        lanternToggle.ExtinguishLantern();   // turn lantern OFF
        teapot.ActivateBrewReadyState();     // turn teapot ON
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, lightRadius);
    }
}
