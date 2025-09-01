// FairyBarrier.cs
using UnityEngine;
using System.Collections.Generic;

public class FairyBarrier : MonoBehaviour
{
    [Header("Requirements")]
    public bool requiresLight = false;
    public bool requiresFairypetalPollen = false;
    public List<string> requiredIlluminatedFlowers = new List<string>();

    [Header("Icons")] 
    public Transform iconDisplayParent;
    public GameObject iconPrefab;

    private HashSet<string> fulfilledRequirements = new HashSet<string>();
    private bool lightProvided = false;
    private bool pollenProvided = false;

    private void Start()
    {
        DisplayAllRequirementIcons();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            FlowerHolder holder = other.GetComponentInParent<FlowerHolder>();
            if (holder != null && holder.HasFlower())
            {
                string flower = holder.currentFlowerType;
                if (requiredIlluminatedFlowers.Contains(flower))
                {
                    fulfilledRequirements.Add(flower);
                    RemoveIcon(flower);
                    holder.DropFlower();
                    TryUnlock();
                }
            }

            FairyPollenStatus pollen = other.GetComponent<FairyPollenStatus>();
            if (pollen != null && pollen.HasFairypetalPollen())
            {
                pollenProvided = true;
                RemoveIcon("FairypetalPollen");
                TryUnlock();
            }
        }
    }

    public void ProvideLight()
    {
        if (requiresLight && !lightProvided)
        {
            lightProvided = true;
            RemoveIcon("Light");
            TryUnlock();
        }
    }

    private void TryUnlock()
    {
        bool flowersFulfilled = fulfilledRequirements.Count >= requiredIlluminatedFlowers.Count;
        bool lightOK = !requiresLight || lightProvided;
        bool pollenOK = !requiresFairypetalPollen || pollenProvided;

        if (flowersFulfilled && lightOK && pollenOK)
        {
            gameObject.SetActive(false);
        }
    }

    private void DisplayAllRequirementIcons()
    {
        if (requiresLight) CreateIcon("Light");
        if (requiresFairypetalPollen) CreateIcon("FairypetalPollen");

        foreach (string flower in requiredIlluminatedFlowers)
        {
            CreateIcon(flower);
        }
    }

    private void CreateIcon(string name)
    {
        GameObject icon = Instantiate(iconPrefab, iconDisplayParent);
        icon.name = name + "Icon";
        // Optionally set sprite based on name here
    }

    private void RemoveIcon(string name)
    {
        Transform icon = iconDisplayParent.Find(name + "Icon");
        if (icon != null)
        {
            Destroy(icon.gameObject);
        }
    }
}
