using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Player Settings")]
    [SerializeField]
    private int startingMaxEnergy = 3;
    public int StartingMaxEnergy => startingMaxEnergy;

    [SerializeField]
    private int startingMoney = 100;
    public int StartingMoney => startingMoney;

    private int currentMaxEnergy = 0;
    public int CurrentMaxEnergy => currentMaxEnergy;

    private int currentEnergy = 0;
    public int CurrentEnergy => currentEnergy;

    private int currentMoney = 0;
    public int CurrentMoney => currentMoney;

    // Events
    [NonSerialized]
    public UnityEvent<int> OnEnergyChanged = new();

    [NonSerialized]
    public UnityEvent<int> OnMoneyChanged = new();

    protected override void Awake()
    {
        base.Awake();

        currentMaxEnergy = startingMaxEnergy;
        currentEnergy = startingMaxEnergy;
        currentMoney = startingMoney;
    }

    public bool GainEnergy(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot gain negative or zero energy");
            return false;
        }

        int previousEnergy = currentEnergy;
        currentEnergy = Mathf.Min(currentEnergy + amount, currentMaxEnergy);
        int actualGained = currentEnergy - previousEnergy;

        if (actualGained > 0)
        {
            OnEnergyChanged?.Invoke(currentEnergy);
        }

        return actualGained > 0;
    }

    public bool SpendEnergy(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot spend negative or zero energy");
            return false;
        }

        if (currentEnergy < amount)
        {
            Debug.LogWarning(
                $"Insufficient energy. Required: {amount}, Available: {currentEnergy}"
            );
            return false;
        }

        currentEnergy -= amount;
        OnEnergyChanged?.Invoke(currentEnergy);

        return true;
    }

    public void SetEnergy(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot set negative energy");
            return;
        }

        currentEnergy = Mathf.Min(amount, currentMaxEnergy);
        OnEnergyChanged?.Invoke(currentEnergy);
    }

    public void SetMaxEnergy(int maxEnergy)
    {
        if (maxEnergy < 0)
        {
            Debug.LogWarning("Cannot set negative max energy");
            return;
        }

        currentMaxEnergy = maxEnergy;

        // If current energy exceeds new max, reduce it
        if (currentEnergy > currentMaxEnergy)
        {
            currentEnergy = currentMaxEnergy;
            OnEnergyChanged?.Invoke(currentEnergy);
        }
    }

    public bool HasEnoughEnergy(int amount)
    {
        return currentEnergy >= amount;
    }

    public void ResetEnergy()
    {
        SetEnergy(currentMaxEnergy);
    }

    // Money Management Methods
    public bool GainMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot gain negative or zero money");
            return false;
        }

        currentMoney += amount;
        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot spend negative or zero money");
            return false;
        }

        if (currentMoney < amount)
        {
            Debug.LogWarning($"Insufficient money. Required: {amount}, Available: {currentMoney}");
            return false;
        }

        currentMoney -= amount;
        OnMoneyChanged?.Invoke(currentMoney);

        return true;
    }

    public void SetMoney(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot set negative money");
            return;
        }

        currentMoney = amount;
        OnMoneyChanged?.Invoke(currentMoney);
    }

    public bool HasEnoughMoney(int amount)
    {
        return currentMoney >= amount;
    }

    public void ResetMoney()
    {
        SetMoney(startingMoney);
    }
}
