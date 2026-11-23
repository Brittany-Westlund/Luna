using System.Collections.Generic;
using UnityEngine;

// Add to the root of each book page prefab.
// The prefab asset's name is its persistent page ID.
public class BookPagePrefab : MonoBehaviour
{
    [Tooltip("Artwork belonging to this prefab. References must point to children of this prefab.")]
    public List<IntroBookSpreadSequenceController.ArtworkEntry> artwork =
        new List<IntroBookSpreadSequenceController.ArtworkEntry>();
}