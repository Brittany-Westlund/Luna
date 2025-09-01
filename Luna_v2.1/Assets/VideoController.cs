using UnityEngine;
using UnityEngine.Video;

public class VideoController : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public Canvas videoCanvas;

    void Start() {
        videoPlayer.playOnAwake = false;
        videoCanvas.enabled = false;
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            videoCanvas.enabled = true;
            videoPlayer.Play();
            Debug.Log("Video should be playing now.");
        }
    }
}
