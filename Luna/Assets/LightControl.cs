using System.Collections;
using UnityEngine;
using MoreMountains.Tools;

public class LightControl : MonoBehaviour
{
    public MMProgressBar LightBar;
    public SpriteRenderer LightDoorRenderer;
    public SpriteRenderer LightKeyRenderer;
    public GameObject Vine;
    public float LoweringOffsetY = -3f;
    public float LightCost = 0.1f;
    public float LoweringSpeed = 1f;

    private bool isLowering = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (LightBar.BarProgress >= LightCost && !isLowering)
            {
                StartCoroutine(LowerVineRoutine());
                // Flicker the key icon and turn off the light door simultaneously
                StartCoroutine(FlickerKeyAndTurnOffDoorRoutine());
            }
        }
    }

    private IEnumerator LowerVineRoutine()
    {
        isLowering = true;
        Vector3 targetPosition = Vine.transform.position + new Vector3(0, LoweringOffsetY, 0);
        
        while (!Mathf.Approximately(Vine.transform.position.y, targetPosition.y))
        {
            Vine.transform.position = Vector3.MoveTowards(Vine.transform.position, targetPosition, LoweringSpeed * Time.deltaTime);
            yield return null;
        }

        isLowering = false;
    }

    private IEnumerator FlickerKeyAndTurnOffDoorRoutine()
    {
        LightKeyRenderer.enabled = true; // Turn on the key icon
        yield return new WaitForSeconds(0.2f); // Wait for a very short time to give the appearance of a flicker
        LightKeyRenderer.enabled = false; // Turn off the key icon
        LightDoorRenderer.enabled = false; // Also turn off the light door renderer at the same time
    }
}
