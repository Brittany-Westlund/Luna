using UnityEngine;
using System.Collections.Generic;

public class TeapotReceiver : MonoBehaviour
{
    [Header("References")]
    public GameObject teacupPrefab;
    public Transform teacupSpawnPoint;
    public GameObject sparkleEffect;
    public Animator teapotAnimator;

    public bool hasLight { get; private set; } = false;

    private bool playerIsNearby = false;
    private FlowerHolder nearbyHolder;

    private List<string> teaIngredients = new List<string>();
    private List<GameObject> storedFlowerObjects = new List<GameObject>();
    private Dictionary<string, GameObject> ingredientIcons = new Dictionary<string, GameObject>();
    private GameObject brewIcon;

    private bool canInteract = true;
    private float interactionCooldown = 0.1f;
    private float cooldownTimer = 0f;
    public bool PlayerIsNearby => playerIsNearby;


    void Start()
    {
        InitializeIcons();
    }

    public void InitializeIcons()
    {
        teaIngredients.Clear();
        storedFlowerObjects.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        ingredientIcons.Clear();
        Transform[] allChildren = GetComponentsInChildren<Transform>(true);

        foreach (Transform child in allChildren)
        {
            if (child.name == "TeacupBrewIcon")
            {
                brewIcon = child.gameObject;
                brewIcon.SetActive(false);
            }
            else if (child.name.EndsWith("IconTeapot"))
            {
                string flowerType = child.name.Replace("IconTeapot", "");
                ingredientIcons[flowerType] = child.gameObject;
                child.gameObject.SetActive(false);
            }
        }

        if (brewIcon == null)
        {
            Debug.LogWarning("⚠️ TeacupBrewIcon not found in teapot prefab!");
        }
    }

    private void Update()
    {
        if (!playerIsNearby || nearbyHolder == null) return;

        if (canInteract && Input.GetKeyDown(KeyCode.X))
        {
            if (nearbyHolder.HasFlower)
            {
                GameObject flower = nearbyHolder.GetHeldFlower();
                string type = !string.IsNullOrEmpty(nearbyHolder.CurrentFlowerType) ? nearbyHolder.CurrentFlowerType : "Unknown";

                if (flower != null && type != "Unknown")
                {
                    flower.SetActive(false);
                    storedFlowerObjects.Add(flower);
                    teaIngredients.Add(type);
                    ShowIngredientIcon(type);

                    nearbyHolder.DropFlower();
                    Debug.Log($"🫖 Added {type} to teapot.");
                }
                else
                {
                    Debug.LogWarning("⚠️ Cannot add flower: missing flower or flowerType.");
                }

                canInteract = false;
            }
            else if (storedFlowerObjects.Count > 0)
            {
                GameObject flower = storedFlowerObjects[^1];
                string type = teaIngredients[^1];

                storedFlowerObjects.RemoveAt(storedFlowerObjects.Count - 1);
                teaIngredients.RemoveAt(teaIngredients.Count - 1);
                HideIngredientIcon(type);

                if (flower != null)
                {
                    flower.SetActive(true);

                    Transform holdPoint = nearbyHolder.holdPoint;
                    if (holdPoint == null)
                    {
                        holdPoint = FindDeepChild(nearbyHolder.transform, "HoldPoint");
                        if (holdPoint != null)
                        {
                            nearbyHolder.holdPoint = holdPoint;
                            Debug.Log("✅ Found HoldPoint dynamically.");
                        }
                        else
                        {
                            Debug.LogWarning("⚠️ Could not find HoldPoint on holder.");
                        }
                    }

                    if (holdPoint != null)
                    {
                        flower.transform.position = holdPoint.position;
                    }

                    nearbyHolder.PickUpFlower(flower);
                    Debug.Log($"🌼 Retrieved {type} from teapot.");
                }
                else
                {
                    Debug.LogWarning("⚠️ Tried to retrieve a flower, but it was null.");
                }

                canInteract = false;
            }
        }

        if (!canInteract)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= interactionCooldown)
            {
                canInteract = true;
                cooldownTimer = 0f;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("LightMote"))
        {
            ReceiveLight();
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (nearbyHolder == null)
        {
            FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
            if (holder != null)
            {
                nearbyHolder = holder;
                playerIsNearby = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
        if (holder == nearbyHolder)
        {
            playerIsNearby = false;
            nearbyHolder = null;
        }
    }

    public void ReceiveLight()
    {
        hasLight = true;

        if (sparkleEffect != null)
            sparkleEffect.SetActive(true);

        if (teapotAnimator != null)
            teapotAnimator.SetTrigger("Pulse");

        if (brewIcon != null)
            brewIcon.SetActive(true);

        Debug.Log("✨ Teapot charged with light!");
    }

    public void ShowIngredientIcon(string flowerType)
    {
        if (ingredientIcons.TryGetValue(flowerType, out GameObject icon))
        {
            icon.SetActive(true);
        }
        else
        {
            Debug.LogWarning($"⚠️ No icon registered for flower: {flowerType}");
        }
    }

    public void HideIngredientIcon(string flowerType)
    {
        if (ingredientIcons.TryGetValue(flowerType, out GameObject icon))
        {
            icon.SetActive(false);
        }
    }

    public bool HasAnyIngredients()
    {
        return teaIngredients.Count > 0;
    }

    public GameObject BrewTea()
    {
        if (!hasLight)
        {
            Debug.LogWarning("❌ Cannot brew without light.");
            return null;
        }

        GameObject teacup = Instantiate(teacupPrefab, teacupSpawnPoint.position, Quaternion.identity);
        TeaEffectManager effect = teacup.GetComponent<TeaEffectManager>();
        if (effect != null)
        {
            effect.SetIngredients(teaIngredients, hasLight);
        }

        ResetTeapot();
        return teacup;
    }

    private void ResetTeapot()
    {
        hasLight = false;
        teaIngredients.Clear();
        storedFlowerObjects.Clear();

        if (sparkleEffect != null)
            sparkleEffect.SetActive(false);

        if (brewIcon != null)
            brewIcon.SetActive(false);

        foreach (var icon in ingredientIcons.Values)
        {
            icon.SetActive(false);
        }
    }

    private Transform FindDeepChild(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
                return child;

            Transform found = FindDeepChild(child, name);
            if (found != null)
                return found;
        }
        return null;
    }

}
