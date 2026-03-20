using UnityEngine;

public class CandleController : MonoBehaviour
{
    private Animator candleAnimator;
    private GameObject teapotRoot;

    private bool hasExtinguished = false;

    private const string PLAYER_TAG = "Player";
    private const string TEACUP_NAME = "Teacup";
    private const string LIT_BOOL = "IsLit";

    private const float PICKUP_RANGE = 1.2f;
    private const KeyCode PICKUP_KEY = KeyCode.T;

    void Awake()
    {
        // Auto-find animator
        candleAnimator = GetComponent<Animator>();

        // Remember the teapot before detaching
        teapotRoot = transform.root.gameObject;

        // Detach so candle survives when teapot is destroyed
        transform.SetParent(null, true);

        // Start candle lit
        if (candleAnimator != null)
            candleAnimator.SetBool(LIT_BOOL, true);
    }

    void Update()
    {
        // When teapot is destroyed → extinguish candle
        if (!hasExtinguished && teapotRoot == null)
        {
            Extinguish();
            hasExtinguished = true;
        }

        // After brew → wait for Luna to pick up tea
        if (hasExtinguished && DidPlayerPickupTeacup())
        {
            Destroy(gameObject);
        }
    }

    void Extinguish()
    {
        if (candleAnimator != null)
            candleAnimator.SetBool(LIT_BOOL, false);
    }

    bool DidPlayerPickupTeacup()
    {
        GameObject luna = GameObject.FindGameObjectWithTag(PLAYER_TAG);
        if (luna == null) return false;

        GameObject teacup = FindTeacup();
        if (teacup == null) return false;

        float distance = Vector3.Distance(teacup.transform.position, luna.transform.position);
        if (distance > PICKUP_RANGE) return false;

        return Input.GetKeyDown(PICKUP_KEY);
    }

    GameObject FindTeacup()
    {
        GameObject[] allObjects = FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains(TEACUP_NAME))
                return obj;
        }

        return null;
    }
}
