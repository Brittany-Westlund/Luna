using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;

public class VideoSceneLoader : MonoBehaviour
{
    public VideoPlayer videoPlayer; // Assign the VideoPlayer component in the Inspector
    public string sceneToLoad;      // Assign the name of the scene to load after the video

    void Start()
    {
        if (videoPlayer == null)
        {
            videoPlayer = GetComponent<VideoPlayer>();
        }
        
        // Subscribe to the loopPointReached event
        videoPlayer.loopPointReached += EndReached;
    }

    void EndReached(VideoPlayer vp)
    {
        // Load the next scene when the video finishes
        SceneManager.LoadScene(sceneToLoad);
    }
}
