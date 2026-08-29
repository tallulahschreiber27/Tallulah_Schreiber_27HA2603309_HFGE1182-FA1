using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI; // REQUIRED: For Slider control elements

public class HealthHandler : MonoBehaviour
{
    [Header("HEALTH PARAMETERS")]
    [SerializeField] private string[] damageTags;
    [SerializeField] private float health;
    [SerializeField] private float healthMax = 100f;
    
    [Header("UI ELEMENTS")]
    [SerializeField] private Slider healthSlider; // Drag and drop your health bar Slider here!

    [Header("DECAY PARAMETERS")]
    [SerializeField] private bool doesDecay = false;
    [SerializeField] private float decayTickRate = 1f;
    [SerializeField] private float decayDamage = 2f;
    private float decayTimer; // FIXED: Replaces the broken Update-coroutine frame loop
    
    [Header("DEATH PARAMETERS")]
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float destroyOnDeathDelay = 0f;
    
    private bool isDead = false; // Prevents death functions from spamming continuously

    void Start()
    {
        health = healthMax;
        UpdateHealthBarUI();
    }

    void Update()
    {
        if (isDead) return;

        // FIXED: Safe, frame-rate independent decay accumulation tracking
        if (doesDecay)
        {
            decayTimer += Time.deltaTime;
            if (decayTimer >= decayTickRate)
            {
                DamageHandler(decayDamage);
                decayTimer = 0f; // Reset tick interval
            }
        }
    }

    // Refreshes the physical filling slider value accurately
    private void UpdateHealthBarUI()
    {
        if (healthSlider != null)
        {
            // Always set Slider Min Value to 0 and Max Value to 1 in the inspector!
            healthSlider.value = health / healthMax;
        }
    }

    public void DamageHandler(string damageTag, float damageAmount)
    {
        if (isDead) return;

        if (damageTags.Contains(damageTag))
        {
            ApplyDamage(damageAmount);
        }
    }

    public void DamageHandler(float damageAmount)
    {
        if (isDead) return;

        ApplyDamage(damageAmount);
    }

    private void ApplyDamage(float amount)
    {
        if (amount <= health)
        {
            health -= amount;
        }
        else
        {
            health = 0;
        }

        UpdateHealthBarUI();

        if (health <= 0)
        {
            DeathHandler();
        }
    }

    public void HealHandler(float healAmount)
    {
        if (isDead) return;

        if (health + healAmount >= healthMax)
        {
            health = healthMax;
        }
        else
        {
            health += healAmount;
        }

        UpdateHealthBarUI();
    }

    public void DeathHandler()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log($"[{gameObject.name}] has died.");

        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyOnDeathDelay);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
