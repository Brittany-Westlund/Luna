using UnityEngine;

public class LunariaWandSummoner : MonoBehaviour
{
    [Header("Unlock State")]
    [Tooltip("Has Luna already acquired the wand?")]
    public bool hasUnlockedWand = false;

    [Header("Scene Wand Reference")]
    [Tooltip("Drag the hidden wand GameObject from the scene here.")]
    public GameObject sceneWand;      // assign in Inspector (the real wand with inspector references set)
    public Transform summonPoint;     // empty GameObject placed just in front of Luna
    public KeyCode summonKey = KeyCode.Q;

    private bool wandIsSummoned = false;

    void Start()
    {
        if (sceneWand != null)
            sceneWand.SetActive(false); // start hidden
    }

    void Update()
    {
        if (hasUnlockedWand && Input.GetKeyDown(summonKey))
        {
            if (!wandIsSummoned)
            {
                // place and show the wand
                sceneWand.transform.position = summonPoint.position;
                sceneWand.SetActive(true);
                wandIsSummoned = true;
                Debug.Log("✨ Luna conjured her wand!");
            }
            else
            {
                // hide the wand again
                sceneWand.SetActive(false);
                wandIsSummoned = false;
                Debug.Log("💨 Luna dismissed her wand.");
            }
        }
    }

    /// <summary>
    /// Call this when Luna first picks up the wand in the world.
    /// </summary>
    public void UnlockWand()
    {
        hasUnlockedWand = true;
        Debug.Log("🔑 Wand unlocked! Luna can now conjure it with Q.");
    }
}
