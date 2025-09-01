using System.Collections;
using UnityEngine;

public class TeacupReceiver : MonoBehaviour
{
    public Transform teacupHoldPoint; // Assign this in the inspector

    public bool CanReceiveTeacup()
    {
        // Add your own logic for whether the NPC can take a teacup
        return true;
    }

    public void ReceiveTeacup(GameObject teacup)
    {
        if (teacupHoldPoint != null)
        {
            teacup.transform.SetParent(teacupHoldPoint);
            teacup.transform.localPosition = Vector3.zero;
        }
        else
        {
            Debug.LogWarning("No teacupHoldPoint assigned on NPC!");
            teacup.transform.SetParent(transform);
        }

        Debug.Log("🫖 NPC received the teacup.");
        StartCoroutine(HoldTeacupThenDestroy(teacup));
    }

    private IEnumerator HoldTeacupThenDestroy(GameObject teacup)
    {
        yield return new WaitForSeconds(2f);

        if (teacup != null)
        {
            Destroy(teacup);
            Debug.Log("🫖 NPC finished with the teacup.");
        }
    }
}
