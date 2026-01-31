using UnityEngine;
using UnityEngine.SceneManagement;
using PixelCrushers.DialogueSystem;

public class LuaSceneBridge : MonoBehaviour
{
    private void Awake()
    {
        // Registers a global Lua function called LoadUnityScene
        Lua.RegisterFunction("LoadUnityScene", this,
            SymbolExtensions.GetMethodInfo(() => LoadUnityScene(string.Empty)));
    }

    public void LoadUnityScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}
