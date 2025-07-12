using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EnergyManager : Singleton<EnergyManager>
{
    [Header("Energy Settings")]
    [SerializeField]
    private int startingEnergy = 0;

    [SerializeField]
    private int maxEnergy = 30;
    public int MaxEnergy => maxEnergy;

    private Dictionary<SeasonType, int> energyLevels = new Dictionary<SeasonType, int>();

    [NonSerialized]
    public UnityEvent<SeasonType, int> OnEnergyChanged = new UnityEvent<SeasonType, int>();

    protected override void Awake()
    {
        base.Awake();
        InitializeEnergyLevels();
    }

    private void InitializeEnergyLevels()
    {
        // Initialize energy for all season types
        energyLevels[SeasonType.Spring] = startingEnergy;
        energyLevels[SeasonType.Summer] = startingEnergy;
        energyLevels[SeasonType.Autumn] = startingEnergy;
        energyLevels[SeasonType.Winter] = startingEnergy;
        energyLevels[SeasonType.Death] = startingEnergy;
    }

    public void AddEnergy(SeasonType seasonType, int amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Tried to add non-positive energy amount: {amount}");
            return;
        }

        if (!energyLevels.ContainsKey(seasonType))
        {
            Debug.LogError($"SeasonType {seasonType} not found in energy levels dictionary");
            return;
        }

        energyLevels[seasonType] += amount;
        OnEnergyChanged?.Invoke(seasonType, energyLevels[seasonType]);

        Debug.Log($"Added {amount} energy to {seasonType}. Total: {energyLevels[seasonType]}");
    }

    public void SetEnergy(SeasonType seasonType, int amount)
    {
        if (amount < 0)
        {
            Debug.LogWarning($"Tried to set energy to negative number: {amount}");
            return;
        }

        if (!energyLevels.ContainsKey(seasonType))
        {
            Debug.LogError($"SeasonType {seasonType} not found in energy levels dictionary");
            return;
        }

        energyLevels[seasonType] = amount;
        OnEnergyChanged?.Invoke(seasonType, energyLevels[seasonType]);
    }

    public int GetEnergy(SeasonType seasonType)
    {
        if (!energyLevels.ContainsKey(seasonType))
        {
            Debug.LogError($"SeasonType {seasonType} not found in energy levels dictionary");
            return 0;
        }

        return energyLevels[seasonType];
    }

    public Dictionary<SeasonType, int> GetAllEnergyLevels()
    {
        return new Dictionary<SeasonType, int>(energyLevels);
    }

    public void ResetAllEnergy()
    {
        var seasonTypes = new List<SeasonType>(energyLevels.Keys);
        foreach (var seasonType in seasonTypes)
        {
            energyLevels[seasonType] = startingEnergy;
            OnEnergyChanged?.Invoke(seasonType, energyLevels[seasonType]);
        }
    }

    public void LogEnergyLevels()
    {
        Debug.Log("Current Energy Levels:");
        foreach (var kvp in energyLevels)
        {
            Debug.Log($"  {kvp.Key}: {kvp.Value}");
        }
    }
}
