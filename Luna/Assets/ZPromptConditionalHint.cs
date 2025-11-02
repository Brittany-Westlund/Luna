using UnityEngine;

public class ZPromptConditionalHint : MonoBehaviour
{
    [Header("Key Settings")]
    public KeyCode keyToPress = KeyCode.Z;

    [Header("References")]
    public GameObject mystParent;          // Assign the parent Myst object
    public GameObject lightHintIcon;       // Drag the LightHintIcon here
    public bool disableWholeObject = true; // optional, matches your other behavior

    void Update()
    {
        if (Input.GetKeyDown(keyToPress))
        {
            // find Sparkles inside the Myst parent
            Transform sparkles = mystParent != null ? mystParent.transform.Find("Sparkles") : null;

            bool sparklesActive = sparkles != null && sparkles.gameObject.activeSelf;

            if (!sparklesActive)
            {
                // Sparkles off → show hint instead
                if (lightHintIcon != null)
                {
                    lightHintIcon.SetActive(true);
                    CancelInvoke(nameof(HideHint)); // restart timer if pressed again
                    Invoke(nameof(HideHint), 2f);
                }
            }
            else
            {
                // Sparkles on → proceed with normal disabling
                if (disableWholeObject)
                    gameObject.SetActive(false);
                else
                    enabled = false;
            }
        }
    }

    private void HideHint()
    {
        if (lightHintIcon != null)
            lightHintIcon.SetActive(false);
    }
}
