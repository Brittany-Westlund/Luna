using UnityEngine;

public class Teapot : MonoBehaviour
{
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;

    public GameObject CreateTeacup()
    {
        if (teacupPrefab == null || teacupSpawnPoint == null)
        {
            Debug.LogWarning("Missing teacup prefab or spawn point!");
            return null;
        }

        return Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
    }
}
