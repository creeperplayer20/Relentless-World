using System;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 100;

    [Header("Stamina")]
    [SerializeField] private float maxStamina = 100f;
    [SerializeField] private float staminaRegenPerSecond = 15f;

    [Header("Needs")]
    [SerializeField] private float maxHunger = 100f;
    [SerializeField] private float maxThirst = 100f;

    public int MaxHealth => maxHealth;
    public int Health { get; private set; }

    public float MaxStamina => maxStamina;
    public float Stamina { get; private set; }

    public float MaxHunger => maxHunger;
    public float Hunger { get; private set; }

    public float MaxThirst => maxThirst;
    public float Thirst { get; private set; }

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        Health = maxHealth;
        Stamina = maxStamina;
        Hunger = maxHunger;
        Thirst = maxThirst;
    }

    public void TakeDamage(int amount)
    {
        if (amount <= 0 || Health <= 0) return;

        Health = Mathf.Max(Health - amount, 0);
        HealthChanged?.Invoke(Health, maxHealth);

        if (Health == 0) Died?.Invoke();
    }

    public void Heal(int amount)
    {
        if (amount <= 0 || Health <= 0) return;

        Health = Mathf.Min(Health + amount, maxHealth);
        HealthChanged?.Invoke(Health, maxHealth);
    }

    public bool TrySpendStamina(float amount)
    {
        if (amount <= 0f) return true;
        if (Stamina < amount) return false;

        Stamina = Mathf.Max(Stamina - amount, 0f);
        return true;
    }

    public void RegenStamina(float dt)
    {
        if (dt <= 0f) return;
        Stamina = Mathf.Min(Stamina + staminaRegenPerSecond * dt, maxStamina);
    }

    public void AddHunger(float amount)
    {
        Hunger = Mathf.Clamp(Hunger + amount, 0f, maxHunger);
    }

    public void AddThirst(float amount)
    {
        Thirst = Mathf.Clamp(Thirst + amount, 0f, maxThirst);
    }
}
