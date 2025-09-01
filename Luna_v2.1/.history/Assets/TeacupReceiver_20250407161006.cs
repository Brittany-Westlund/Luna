using UnityEngine;
using System.Collections;

public class TeacupReceiver : MonoBehaviour
{
    private GameObject heldTeacup;

    private GameObject happyIcon;
    private GameObject cozyIcon;

    private Transform teacupHoldPoint;

    private void Start()
    {
        // Try to find the hold point and icons by name in children
        teacupHoldPoint = transform.Find("TeacupHoldPoint");
        happyIcon = transform.Find("HappyIcon")?.gameObject;
        cozyIcon = transform.Find("CozyIcon")?.gameObject;

        if (happyIcon != null) happyIcon.SetActive(false);
        if (cozyIcon != null) cozyIcon.SetActive(false);
    }

    public bool CanReceiveTeacup()
    {
        return heldTeacup == null;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        heldTeacup = teacup;

        if (teacupHoldPoint != null)
        {
            heldTeacup.transform.SetParent(teacupHoldPoint);
            heldTeacup.transform.localPosition = Vector3.zero;
        }
        else
        {
            heldTeacup.transform.SetParent(transform);
            heldTeacup.transform.localPosition = Vector3.zero;
        }

        StartCoroutine(HandleTeacupRoutine());
    }

    private IEnumerator HandleTeacupRoutine()
    {
        yield return new WaitForSeconds(2f); // Hold it briefly

        if (heldTeacup != null)
        {
            Destroy(heldTeacup);
            heldTeacup = null;
        }

        if (happyIcon != null) happyIcon.SetActive(true);
        if (cozyIcon != null) cozyIcon.SetActive(true);

        yield return new WaitForSeconds(60f);

        if (happyIcon != null) happyIcon.SetActive(false);
        if (cozyIcon != null) cozyIcon.SetActive(false);
    }
}
