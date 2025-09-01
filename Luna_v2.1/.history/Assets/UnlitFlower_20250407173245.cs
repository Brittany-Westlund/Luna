using UnityEngine;

public class UnlitFlower : MonoBehaviour
{
    public GameObject litFlowerVisual;    // Set in the inspector
    public GameObject unlitFlowerVisual;  // Set in the inspector
    private bool isLit = false;

    public void Illuminate()
    {
        if (isLit) return;

        if (litFlowerVisual != null) litFlowerVisual.SetActive(true);
        if (unlitFlowerVisual != null) unlitFlowerVisual.SetActive(false);

        isLit = true;
        Debug.Log("🌼 Flower illuminated!");
    }
}
