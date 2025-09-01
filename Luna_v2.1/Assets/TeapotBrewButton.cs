using UnityEngine;

public class TeapotBrewButton : MonoBehaviour
{
    [Header("Brew Settings")]
    public TeapotLightReceiver teapotReceiver;  // Reference to the teapot's light receiver
    public GameObject teacupPrefab;             // Prefab of the brewed teacup
    public Transform spawnPoint;                // Where the teacup spawns
    public KeyCode brewKey = KeyCode.T;         // Key to trigger brewing

    private void Update()
    {
        if (Input.GetKeyDown(brewKey) && teapotReceiver != null && teapotReceiver.IsReadyToBrew())
        {
            BrewTea();
        }
    }

    private void BrewTea()
    {
        if (teacupPrefab != null && spawnPoint != null)
        {
            Instantiate(teacupPrefab, spawnPoint.position, Quaternion.identity);
        }

        teapotReceiver.ResetTeapot();

        Debug.Log("Tea brewed and teapot reset.");
    }
}
