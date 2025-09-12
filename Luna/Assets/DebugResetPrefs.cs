using UnityEngine;

public class DebugResetPrefs : MonoBehaviour
{
    void Awake()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("🧹 PlayerPrefs cleared!");
    }
}
