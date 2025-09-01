using UnityEngine;

public class TeacupReceiver : MonoBehaviour
{
    public Transform teacupHoldPoint;
    private GameObject heldTeacup;
    private GameObject happyIcon;
    private GameObject cozyIcon;

    private void Awake()
    {
        // Automatically find the HappyIcon in children
        Transform iconTransform = transform.Find("HappyIcon");
        if (iconTransform != null)
        {
            happyIcon = iconTransform.gameObject;
            happyIcon.SetActive(false); // Make sure it starts hidden
        }
        else
        {
            Debug.LogWarning("HappyIcon not found on " + gameObject.name);
        }
    }

    public bool CanReceiveTeacup()
    {
        return heldTeacup == null;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        heldTeacup = teacup;
        teacup.transform.SetParent(teacupHoldPoint);
        teacup.transform.localPosition = Vector3.zero;

        Debug.Log("🫖 NPC received teacup!");

        StartCoroutine(HandleTeacupRoutine());
    }

    private System.Collections.IEnumerator HandleTeacupRoutine()
{
    yield return new WaitForSeconds(2f); // Hold teacup for 2 seconds

    if (heldTeacup != null)
    {
        Destroy(heldTeacup);
        heldTeacup = null;
    }

    // Show Happy Icon
    if (happyIcon != null)
    {
        happyIcon.SetActive(true);
        Debug.Log("😊 Happy icon displayed!");
    }

    // Show Cozy Icon
    if (cozyIcon != null)
    {
        cozyIcon.SetActive(true);
        Debug.Log("🧣 Cozy icon displayed!");
    }

    yield return new WaitForSeconds(60f); // Duration both icons stay visible

    if (happyIcon != null) happyIcon.SetActive(false);
    if (cozyIcon != null) cozyIcon.SetActive(false);

    Debug.Log("Icons hidden after 1 minute.");
}


}
