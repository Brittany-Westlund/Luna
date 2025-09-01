using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTimerLoader : MonoBehaviour
{
    public float delay = 30f;
    public string sceneToLoad = "NextSceneName";

    private float timer;

    void Start()
    {
        timer = delay;
    }

    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }
}
