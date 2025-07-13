using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UnitData", menuName = "Farming Roguelike/Unit Data")]
public class UnitData : ScriptableObject
{
    public static readonly int DEFAULT_POINT_SCORE = 10;

    [SerializeField]
    private string unitName;
    public string UnitName => unitName;

    [SerializeField]
    private Sprite image;
    public Sprite Image => image;

    [SerializeField]
    private GameObject unitPrefab;
    public GameObject UnitPrefab => unitPrefab;

    [SerializeField]
    private string text;
    public string Text => text;

    [SerializeField]
    private List<SeasonType> seasonTypes;
    public List<SeasonType> SeasonTypes => seasonTypes;

    [SerializeField]
    private int pointScore = DEFAULT_POINT_SCORE;
    public int PointScore => pointScore;
}
