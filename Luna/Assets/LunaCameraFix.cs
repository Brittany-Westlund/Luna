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
                Character character = playerObj.GetComponent<Character>();
                if (character != null)
                {
                    cameraController.SetTarget(character);
                    cameraController.StartFollowing();
                    Debug.Log("📷 Camera now following character: " + playerObj.name);
                    yield break;
                }
                else
                {
                    Debug.LogWarning("⚠️ Player object found, but no Character component attached.");
                }
            }
        }
    }
}
