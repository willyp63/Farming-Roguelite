using UnityEngine;

public enum GridTileType
{
    None,
    Crop,
    Animal,
    Building,
}

[CreateAssetMenu(fileName = "GridTileData", menuName = "Farming Roguelike/Grid Tile Data")]
public class GridTileData : ScriptableObject
{
    [SerializeField]
    private string tileName;
    public string TileName => tileName;

    [SerializeField]
    private GameObject objectPrefab;
    public GameObject ObjectPrefab => objectPrefab;

    [SerializeField]
    private GridTileType type = GridTileType.None;
    public GridTileType Type => type;

    [SerializeField]
    private SeasonType season = SeasonType.None;
    public SeasonType Season => season;

    [SerializeField]
    private int pointScore = 0;
    public int PointScore => pointScore;

    [SerializeField]
    private int multiScore = 0;
    public int MultiScore => multiScore;

    [SerializeField]
    [TextArea(4, 8)]
    private string text;
    public string Text => text;

    // TODO: add effects
}
