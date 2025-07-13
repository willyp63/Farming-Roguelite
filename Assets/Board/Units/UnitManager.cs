using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitManager : Singleton<UnitManager>
{
    [SerializeField]
    private List<UnitData> allUnits;

    public List<UnitData> GetRandomUnits(int amount)
    {
        List<UnitData> result = new List<UnitData>();

        if (allUnits == null || allUnits.Count == 0)
        {
            Debug.LogWarning("No units available in UnitManager!");
            return result;
        }

        // Create a copy of the list to shuffle
        List<UnitData> availableUnits = new List<UnitData>(allUnits);

        // Shuffle the list
        for (int i = 0; i < availableUnits.Count; i++)
        {
            int randomIndex = Random.Range(i, availableUnits.Count);
            UnitData temp = availableUnits[i];
            availableUnits[i] = availableUnits[randomIndex];
            availableUnits[randomIndex] = temp;
        }

        // Take the first 'amount' units
        for (int i = 0; i < amount && i < availableUnits.Count; i++)
        {
            result.Add(availableUnits[i]);
        }

        return result;
    }
}
