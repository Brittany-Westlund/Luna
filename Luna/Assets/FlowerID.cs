using UnityEngine;
using System;

[DisallowMultipleComponent]
public class FlowerID : MonoBehaviour
{
    [Tooltip("Unique identifier for this flower (auto-assigned & remembered).")]
    public string flowerID;

    const string PP_PREFIX = "FLOWER_ID_MAP::";

    void Awake()
    {
        if (!string.IsNullOrEmpty(flowerID)) return;

        string scene = gameObject.scene.IsValid() ? gameObject.scene.name : "DontDestroy";
        string path  = GetHierarchyPath(transform);
        string key   = PP_PREFIX + scene + "::" + path;

        if (PlayerPrefs.HasKey(key))
        {
            flowerID = PlayerPrefs.GetString(key);
        }
        else
        {
            flowerID = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(key, flowerID);
            PlayerPrefs.Save();
        }
    }

    static string GetHierarchyPath(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }
}
