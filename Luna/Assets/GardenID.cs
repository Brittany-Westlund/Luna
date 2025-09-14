using UnityEngine;
using System;

[DisallowMultipleComponent]
public class GardenID : MonoBehaviour
{
    [Tooltip("Unique identifier for this garden. Auto-assigned if empty.")]
    public string gardenID;

    void Awake()
    {
        if (string.IsNullOrEmpty(gardenID))
            gardenID = Guid.NewGuid().ToString();
    }
}
