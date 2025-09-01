using UnityEngine;
using System.Collections;
using MoreMountains.CorgiEngine;

public class LunaCameraFix : MonoBehaviour
{
    private CinemachineCameraController cameraController;

    private void Start()
    {
        cameraController = FindObjectOfType<CinemachineCameraController>();
        StartCoroutine(WatchForRespawnedPlayer());
    }

    private IEnumerator WatchForRespawnedPlayer()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);

            GameObject playerObj = GameObject.FindWithTag("Player");

            if (playerObj != null && cameraController != null)
            {
                cameraController.StartFollowing();
                Debug.Log("📷 Camera reconnected to: " + playerObj.name);
                yield break; // Stop once it's fixed
            }
        }
    }
}
