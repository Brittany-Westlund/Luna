using UnityEngine;

public class TeacupReceiver : MonoBehaviour
{
    public Transform teacupHoldPoint;
    private GameObject receivedTeacup;

    public void ReceiveTeacup(GameObject teacup)
    {
        if (receivedTeacup != null)
        {
            Destroy(receivedTeacup); // Optional: remove previous
        }

        receivedTeacup = teacup;
        receivedTeacup.transform.SetParent(teacupHoldPoint);
        receivedTeacup.transform.localPosition = Vector3.zero;

        Debug.Log($"{name} received a teacup!");
    }

    public bool CanReceiveTeacup()
    {
        return receivedTeacup == null;
    }
}
