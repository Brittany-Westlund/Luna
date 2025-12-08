using UnityEngine;

public class LanternToTeapotLightTransfer : MonoBehaviour
{
    [Header("References")]
    public TeapotLightReceiver teapot;

    [Header("Detection")]
    public string lanternTag = "Lantern";

    [Header("Input (Optional)")]
    public bool requireKeyPress = true;   // ✅ toggle this ON for G-key behavior
    public KeyCode transferKey = KeyCode.G;

    private LanternSmartToggle lanternInRange;

    private void Awake()
    {
        if (teapot == null)
            teapot = GetComponent<TeapotLightReceiver>();
    }

    private void Update()
    {
        if (!requireKeyPress) return;
        if (lanternInRange == null || teapot == null) return;

        // ✅ G-key transfer mode
        if (Input.GetKeyDown(transferKey) && lanternInRange.IsLit && !teapot.HasLight)
        {
            TransferLight();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(lanternTag)) return;

        lanternInRange = other.GetComponent<LanternSmartToggle>();

        // ✅ Auto-transfer mode (if key press not required)
        if (!requireKeyPress && lanternInRange != null && lanternInRange.IsLit && teapot != null && !teapot.HasLight)
        {
            TransferLight();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(lanternTag)) return;

        lanternInRange = null;
    }

    private void TransferLight()
    {
        lanternInRange.ExtinguishLantern();   // Lantern OFF
        teapot.ActivateBrewReadyState();      // Teapot ON
    }
}
