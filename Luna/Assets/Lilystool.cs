using UnityEngine;

public class LilyStool : MonoBehaviour
{
    public Transform teapotSpawnPoint;

    [Header("Gate State")]
    [SerializeField] private bool playerOnLilypad = false;

    public bool PlayerOnLilypad => playerOnLilypad;

    public void SetPlayerOnLilypad(bool value)
    {
        playerOnLilypad = value;
        Debug.Log($"[LilyStool] {name} PlayerOnLilypad = {playerOnLilypad}");
    }
}