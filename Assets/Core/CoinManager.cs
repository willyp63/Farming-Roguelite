using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class CoinManager : Singleton<CoinManager>
{
    [Header("Coin Settings")]
    [SerializeField]
    private int startingCoins = 100;

    private int currentCoins;

    public UnityEvent<int> OnCoinsChanged;
    public UnityEvent<int> OnCoinsGained;
    public UnityEvent<int> OnCoinsSpent;
    public UnityEvent OnInsufficientCoins;

    public int CurrentCoins => currentCoins;

    protected override void Awake()
    {
        base.Awake();
        currentCoins = startingCoins;
    }

    void Start()
    {
        OnCoinsChanged?.Invoke(currentCoins);
    }

    public int GetCoins()
    {
        return currentCoins;
    }

    public bool GainCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot gain negative or zero coins");
            return false;
        }

        currentCoins += amount;
        OnCoinsGained?.Invoke(amount);
        OnCoinsChanged?.Invoke(currentCoins);

        return true;
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning("Cannot spend negative or zero coins");
            return false;
        }

        if (currentCoins < amount)
        {
            OnInsufficientCoins?.Invoke();
            Debug.LogWarning($"Insufficient coins. Required: {amount}, Available: {currentCoins}");
            return false;
        }

        currentCoins -= amount;
        OnCoinsSpent?.Invoke(amount);
        OnCoinsChanged?.Invoke(currentCoins);

        return true;
    }

    public void SetCoins(int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning("Cannot set negative coins");
            return;
        }

        int previousAmount = currentCoins;
        currentCoins = amount;

        if (currentCoins > previousAmount)
        {
            OnCoinsGained?.Invoke(currentCoins - previousAmount);
        }
        else if (currentCoins < previousAmount)
        {
            OnCoinsSpent?.Invoke(previousAmount - currentCoins);
        }

        OnCoinsChanged?.Invoke(currentCoins);
    }

    public bool HasEnoughCoins(int amount)
    {
        return currentCoins >= amount;
    }

    public void ResetCoins()
    {
        SetCoins(startingCoins);
    }
}
