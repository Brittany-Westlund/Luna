using UnityEngine;

public class GameObjectController : MonoBehaviour
{
    // This will allow you to assign a GameObject in the Unity Inspector
    public GameObject targetGameObject;

    // Function to enable the assigned GameObject
    public void EnableTargetGameObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(true);
            Debug.Log(targetGameObject.name + " has been enabled.");
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }

    // Function to disable the assigned GameObject
    public void DisableTargetGameObject()
    {
        if (targetGameObject != null)
        {
            targetGameObject.SetActive(false);
            Debug.Log(targetGameObject.name + " has been disabled.");
        }
        else
        {
            Debug.LogError("Target GameObject is not assigned.");
        }
    }
}
