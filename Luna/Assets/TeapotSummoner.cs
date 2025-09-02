using UnityEngine;

public class TeapotSummoner : MonoBehaviour
{
    public GameObject teapotPrefab;
    public Transform spawnPoint;
    public KeyCode summonKey = KeyCode.T;

    private GameObject currentTeapot;

    void Update()
    {
        if (Input.GetKeyDown(summonKey))
        {
            TrySummonTeapot();
        }
    }

    void TrySummonTeapot()
    {
        if (currentTeapot == null)
        {
            currentTeapot = Instantiate(teapotPrefab, spawnPoint.position, Quaternion.identity);
            currentTeapot.SetActive(true);
        }
        else if (!currentTeapot.activeInHierarchy)
        {
            currentTeapot.transform.position = spawnPoint.position;
            currentTeapot.SetActive(true);
        }
    }
}
