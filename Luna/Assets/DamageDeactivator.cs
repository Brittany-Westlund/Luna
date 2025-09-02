using UnityEngine;
using MoreMountains.CorgiEngine;

public class DamageDeactivator : MonoBehaviour
{
    private DamageOnTouch _damageOnTouch;
    private SimpleDamageReaction _damageReaction;

    private void Start()
    {
        _damageOnTouch = GetComponent<DamageOnTouch>();
        _damageReaction = FindObjectOfType<SimpleDamageReaction>(); // Adjust if needed to find the correct instance
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && _damageOnTouch != null)
        {
            // Disable the DamageOnTouch component
            _damageOnTouch.enabled = false;

            // Trigger the damage reaction, only once
            _damageReaction?.ShowDamageReaction();

            Debug.Log("Damage on touch deactivated after triggering once.");
        }
    }
}
