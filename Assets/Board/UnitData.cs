using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Farming Roguelike/Unit Data")]
public class UnitData : ScriptableObject
{
    [SerializeField]
    private GameObject unitPrefab;
    public GameObject UnitPrefab => unitPrefab;

    [SerializeField]
    private List<SeasonType> seasonTypes;
    public List<SeasonType> SeasonTypes => seasonTypes;
}
