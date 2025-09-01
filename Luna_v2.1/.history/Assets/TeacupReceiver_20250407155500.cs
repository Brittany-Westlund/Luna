using UnityEngine;

public class TeacupReceiver : MonoBehaviour
{
    public Transform teacupHoldPoint;
    public GameObject happyIcon; // Assign in Inspector
    private GameObject heldTeacup;

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
        yield return new WaitForSeconds(2f); // hold teacup for 2 seconds

        if (heldTeacup != null)
        {
            Destroy(heldTeacup);
            heldTeacup = null;
        }

        if (happyIcon != null)
        {
            happyIcon.SetActive(true);
            Debug.Log("😊 Happy icon displayed!");
        }
    }
}
