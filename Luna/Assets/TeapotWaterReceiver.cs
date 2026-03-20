using UnityEngine;

public class TeapotWaterReceiver : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private bool startsWithWater = false;

    [Header("Optional Direct Assignments")]
    [SerializeField] private GameObject waterIconObject;
    [SerializeField] private SpriteRenderer waterIconRenderer;
    [SerializeField] private GameObject brewingIndicator;

    [Header("Search")]
    [SerializeField] private string waterIconChildName = "WaterDropIcon";
    [SerializeField] private string brewingIndicatorChildName = "BrewingIndicator";

    [Header("Debug")]
    [SerializeField] private bool debugLogging = false;

    private bool hasWater;

    private void Awake()
    {
        FindWaterIconReferences();
        FindBrewingIndicator();
        hasWater = startsWithWater;
        RefreshVisual();
    }

    private void FindWaterIconReferences()
    {
        if (waterIconObject == null)
        {
            Transform[] allChildren = GetComponentsInChildren<Transform>(true);

            for (int i = 0; i < allChildren.Length; i++)
            {
                if (allChildren[i].name == waterIconChildName)
                {
                    waterIconObject = allChildren[i].gameObject;
                    break;
                }
            }
        }

        if (waterIconRenderer == null && waterIconObject != null)
        {
            waterIconRenderer = waterIconObject.GetComponent<SpriteRenderer>();
        }

        if (debugLogging)
        {
            string objectName = waterIconObject != null ? waterIconObject.name : "null";
            string rendererName = waterIconRenderer != null ? waterIconRenderer.name : "null";
            Debug.Log($"[TeapotWaterReceiver] '{name}' icon object: {objectName}, renderer: {rendererName}");
        }
    }

    private void FindBrewingIndicator()
    {
        if (brewingIndicator != null)
            return;

        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < allChildren.Length; i++)
        {
            if (allChildren[i].name == brewingIndicatorChildName)
            {
                brewingIndicator = allChildren[i].gameObject;

                if (debugLogging)
                {
                    Debug.Log($"[TeapotWaterReceiver] Found BrewingIndicator on '{name}'.");
                }

                return;
            }
        }

        if (debugLogging)
        {
            Debug.LogWarning($"[TeapotWaterReceiver] Could not find child named '{brewingIndicatorChildName}' on '{name}'.");
        }
    }

    public void ReceiveWater()
    {
        hasWater = true;
        RefreshVisual();

        if (debugLogging)
        {
            Debug.Log($"[TeapotWaterReceiver] '{name}' received water.");
        }
    }

    public bool HasWater()
    {
        return hasWater;
    }

    public bool TryConsumeWaterForBrewing()
    {
        if (!hasWater)
        {
            if (debugLogging)
            {
                Debug.Log($"[TeapotWaterReceiver] '{name}' has no water; brewing blocked.");
            }

            return false;
        }

        hasWater = false;
        RefreshVisual();

        if (debugLogging)
        {
            Debug.Log($"[TeapotWaterReceiver] '{name}' consumed water for brewing.");
        }

        return true;
    }

    public void ClearWater()
    {
        hasWater = false;
        RefreshVisual();

        if (debugLogging)
        {
            Debug.Log($"[TeapotWaterReceiver] '{name}' water cleared.");
        }
    }

    public void RefreshVisual()
    {
        if (waterIconObject == null || (waterIconRenderer == null && waterIconObject != null))
        {
            FindWaterIconReferences();
        }

        if (brewingIndicator == null)
        {
            FindBrewingIndicator();
        }

        if (waterIconObject != null)
        {
            waterIconObject.SetActive(hasWater);
        }

        if (waterIconRenderer != null)
        {
            waterIconRenderer.enabled = hasWater;
        }

        if (brewingIndicator != null)
        {
            brewingIndicator.SetActive(hasWater);
        }

        if (debugLogging)
        {
            Debug.Log($"[TeapotWaterReceiver] '{name}' RefreshVisual -> hasWater={hasWater}");
        }
    }
}