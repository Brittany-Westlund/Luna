// LuaFunctionsUtility.cs
using UnityEngine;
using PixelCrushers.DialogueSystem;
using System.Reflection;

public class LuaFunctionsUtility : MonoBehaviour
{
    void Start()
    {
        Lua.RegisterFunction("IsGameObjectActive", null, typeof(LuaFunctionsUtility).GetMethod("IsGameObjectActive"));
        Lua.RegisterFunction("EnableGameObject", null, typeof(LuaFunctionsUtility).GetMethod("EnableGameObject"));
        Lua.RegisterFunction("FindAndSendMessage", null, typeof(LuaFunctionsUtility).GetMethod("FindAndSendMessage"));
        Lua.RegisterFunction("SetSpriteRendererEnabled", null, typeof(LuaFunctionsUtility).GetMethod("SetSpriteRendererEnabled"));
        Lua.RegisterFunction("MoveTo", null, typeof(LuaFunctionsUtility).GetMethod("MoveTo"));
        Lua.RegisterFunction("SetTeapotSpawned", null, typeof(LuaFunctionsUtility).GetMethod("SetTeapotSpawned"));
    }

    public static bool IsGameObjectActive(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj == null)
        {
            Debug.LogError($"GameObject '{objectName}' not found.");
            return false;
        }
        return obj.activeSelf;
    }

    public static void EnableGameObject(string objectName)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            obj.SetActive(true);
            Debug.Log($"GameObject '{objectName}' has been enabled.");
        }
        else
        {
            Debug.LogError($"GameObject '{objectName}' not found.");
        }
    }

    public static void FindAndSendMessage(string objectName, string message)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            obj.SendMessage(message, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"Message '{message}' sent to GameObject '{objectName}'.");
        }
        else
        {
            Debug.LogError($"GameObject '{objectName}' not found.");
        }
    }

    public static void SetSpriteRendererEnabled(string objectName, bool enabled)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            SpriteRenderer renderer = obj.GetComponent<SpriteRenderer>();
            if (renderer != null)
            {
                renderer.enabled = enabled;
                Debug.Log($"{objectName}'s SpriteRenderer has been set to {enabled}.");
            }
            else
            {
                Debug.LogError($"SpriteRenderer not found on {objectName}.");
            }
        }
        else
        {
            Debug.LogError($"GameObject '{objectName}' not found.");
        }
    }

    // ✅ New function: Move a GameObject to a specified world position
    public static void MoveTo(string objectName, double x, double y, double z)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            obj.transform.position = new Vector3((float)x, (float)y, (float)z);
            Debug.Log($"Moved '{objectName}' to new position: ({x}, {y}, {z})");
        }
        else
        {
            Debug.LogError($"MoveTo Error: GameObject '{objectName}' not found.");
        }
    }

    public static void SetTeapotSpawned(bool value)
    {
        Lua.Run($"TeapotSpawned = {value.ToString().ToLower()}");
        Debug.Log($"TeapotSpawned set to {value}");
    }

}
