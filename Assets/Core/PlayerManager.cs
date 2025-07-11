using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PlayerManager : Singleton<PlayerManager>
{
    [Header("Player Settings")]
    [SerializeField]
    private List<GridTileData> tiles = new();
    public List<GridTileData> Tiles => tiles;

    [SerializeField]
    private int startingMoney = 100;
    public int StartingMoney => startingMoney;

    private int currentMoney = 0;
    public int CurrentMoney => currentMoney;

    [NonSerialized]
    public UnityEvent OnMoneyChanged = new();

    protected override void Awake()
    {
        base.Awake();

        currentMoney = startingMoney;
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
        OnMoneyChanged?.Invoke();

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
        OnMoneyChanged?.Invoke();

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
        OnMoneyChanged?.Invoke();
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
