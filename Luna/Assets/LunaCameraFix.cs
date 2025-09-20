using UnityEngine;
using UnityEngine.SceneManagement;
using MoreMountains.CorgiEngine;

public class ReassignCorgiCamera : MonoBehaviour
{
    private CinemachineCameraController cameraController;

    void Start()
    {
        cameraController = FindObjectOfType<CinemachineCameraController>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(ReassignCameraAfterDelay());
    }

    private System.Collections.IEnumerator ReassignCameraAfterDelay()
    {
        yield return new WaitForSeconds(0.25f); // Short delay to let Luna settle

        GameObject playerObj = GameObject.FindWithTag("Player");

        if (playerObj != null && cameraController != null)
        {
            Character character = playerObj.GetComponent<Character>();
            if (character != null)
            {
                cameraController.SetTarget(character);
                cameraController.StartFollowing();
                Debug.Log("📷 Camera now following Luna after scene load.");
            }
            else
            {
                Debug.LogWarning("⚠️ Player found but no Character component.");
            }
        }
        else
        {
            Debug.LogWarning("⚠️ Could not find Player or CameraController.");
        }
    }
}
