using UnityEngine;
using System.Collections;
using PixelCrushers.DialogueSystem; // Make sure to include this if using Dialogue System

public class GroundRakeInteraction : MonoBehaviour
{
    public GameObject rakeObject; // The rake object that triggers the interaction
    public float disappearDelay = 2f; // Delay before the ground disappears
    public string nextDialogueTitle; // Title of the next dialogue

    private SpriteRenderer groundSprite; // SpriteRenderer of the ground

    private void Start()
    {
        groundSprite = GetComponent<SpriteRenderer>(); // Get the SpriteRenderer component of the ground
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject == rakeObject) // Check if the colliding object is the rake
        {
            StartCoroutine(InteractionSequence());
        }
    }

    IEnumerator InteractionSequence()
    {
        yield return new WaitForSeconds(disappearDelay); // Wait for specified delay
        groundSprite.enabled = false; // Disable the sprite to make ground disappear
        TriggerNextDialogue();
    }

    void TriggerNextDialogue()
    {
        if (!string.IsNullOrEmpty(nextDialogueTitle))
        {
            DialogueManager.StartConversation(nextDialogueTitle); // Start the next dialogue
        }
    }
}
