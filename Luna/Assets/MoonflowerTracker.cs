using System;
using System.Collections.Generic;
using UnityEngine;

public static class MoonflowerTracker
{
    public static readonly List<FlowerPickup> ActiveMoonflowers = new List<FlowerPickup>();

    public static event Action<FlowerPickup> OnMoonflowerAdded;
    public static event Action<FlowerPickup> OnMoonflowerRemoved;

    public static void Register(FlowerPickup flower)
    {
        if (flower == null || ActiveMoonflowers.Contains(flower)) return;

        if (flower.flowerType.Equals("Moonflower", StringComparison.OrdinalIgnoreCase))
        {
            ActiveMoonflowers.Add(flower);
            OnMoonflowerAdded?.Invoke(flower);
        }
    }

    public static void Unregister(FlowerPickup flower)
    {
        if (flower == null) return;

        if (ActiveMoonflowers.Remove(flower))
        {
            OnMoonflowerRemoved?.Invoke(flower);
        }
    }
}
