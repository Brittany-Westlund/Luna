using UnityEngine;
using MoreMountains.Tools; // Make sure you have the MMProgressBar namespace available

public class PlayerHealth : MonoBehaviour
{
    public float currentHealth = 100f; // Example health value
    public float maxHealth = 100f; // Set the maximum health value
    public MMProgressBar healthBar; // Reference to the MMProgressBar for health

    private void Start()
    {
        // Initialize the health bar value
        UpdateHealthBar();
    }

    // Method to reduce health by a specified amount
    public void ReduceHealth(float amount)
    {
        currentHealth -= amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Clamp health to ensure it stays within bounds
        UpdateHealthBar();
    }

    // Method to halve health (can be called directly from other scripts)
    public void HalveHealth()
    {
        currentHealth /= 2;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth); // Ensure health does not go below zero
        Debug.Log("Player health halved to: " + currentHealth);
        UpdateHealthBar();
    }

    // Method to update the health bar based on current health
    private void UpdateHealthBar()
    {
        if (healthBar != null)
        {
            // Update the MMProgressBar's value with the current health percentage
            healthBar.UpdateBar(currentHealth, 0f, maxHealth);
        }
        else
        {
            Debug.LogWarning("HealthBar reference is missing. Please assign the MMProgressBar in the inspector.");
        }
    }
}
