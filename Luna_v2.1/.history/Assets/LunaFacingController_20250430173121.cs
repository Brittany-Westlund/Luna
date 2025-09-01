using UnityEngine;

public class LunaFacingByMovement : MonoBehaviour
{
    public GameObject faceLeftObject;
    public GameObject faceRightObject;

    private bool spriteShown = false;
    private Vector2 lastPosition;

    void Start()
    {
        if (faceLeftObject) faceLeftObject.SetActive(false);
        if (faceRightObject) faceRightObject.SetActive(false);

        lastPosition = transform.position;
    }

    void Update()
    {
        Vector2 currentPosition = transform.position;
        float deltaX = currentPosition.x - lastPosition.x;

        if (!spriteShown && Mathf.Abs(deltaX) > 0.01f)
        {
            ShowSprite(deltaX);
            spriteShown = true;
        }

        lastPosition = currentPosition;
    }

    void ShowSprite(float deltaX)
    {
        bool isRight = deltaX > 0f;

        if (faceLeftObject) faceLeftObject.SetActive(!isRight);
        if (faceRightObject) faceRightObject.SetActive(isRight);
    }
}
